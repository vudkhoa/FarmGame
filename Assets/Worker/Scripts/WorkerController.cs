using Bag.Controller;
using Data.Config;
using Data.Game;
using Data.Manager;
using Plots.Controller;
using Plots.Model;
using Product.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.DesignPattern.Singleton;
using Worker.Model;
using Worker.View;

namespace Worker.Controller
{
    public enum WorkerState
    {
        None = 0,
        Idle = 1,
        Produce = 2, 
        Harvest = 3,
    }

    public class WorkerController : SingletonMono<WorkerController>
    {
        [Header(" WorkerController Setting ")]
        [SerializeField] private WorkerView WorkerItemPrefab;
        [SerializeField] private RectTransform ParentAllWorkerItem;
        public float Offset;

        [Header(" Running Game ")]
        public List<WorkerModel> WorkerList;
        public WorkerView View;
        public int CurIdle;

        public void Init()
        {
            // Get Data Để chạy mô phỏng.
            this.GetData();
            // Chạy mô phỏng tiển trình Off Game.
            this.OffGame_CheckWorking();
            // Get lại Data sau khi chạy mô phỏng.
            this.GetData(true);
            // Gán Data sau khi chạy mô phỏng cho Plot.
            PlotController.Instance.Init();
            // Init View
            this.InitView();
        }

        private void ResetData()
        {
            int countWorker = DataManager.Instance.GameData.WorkerList.Count;
            for (int i = 0; i < countWorker; i++)
            {
                WorkerDetail worker = new WorkerDetail();
                worker.Id = i.ToString();
                worker.State = WorkerState.Idle;
                DataManager.Instance.GameData.WorkerList[i] = worker;
            }
        }

        private WorkerModel SetDataForModel(WorkerDetail worker)
        {
            WorkerModel model = new WorkerModel();
            model.Id = int.Parse(worker.Id);
            model.State = worker.State;
            model.IdProduct = worker.IdProduct;
            model.IdPlot = worker.IdPlot;
            model.TimeTask = DataManager.Instance.GameConfig.WorkerConfig.TimeTask;
            model.StartTime = DateTime.Parse(worker.StartTime);
            model.NotExecute = worker.NotExcute;
            return model;
        }

        private void GetData(bool isExecute = false)
        {
            this.WorkerList = new List<WorkerModel>();
            this.CurIdle = 0;
            foreach (WorkerDetail worker in DataManager.Instance.GameData.WorkerList)
            {
                WorkerModel model = new WorkerModel();
                model = SetDataForModel(worker);
                
                float offset = 0;
                if (worker.State == WorkerState.Idle || worker.State == WorkerState.None)
                {
                    model.StateMachine.Change(model, new Idle());

                }
                else if (worker.State == WorkerState.Harvest)
                {
                    offset = (float)(DateTime.Now - model.StartTime).TotalSeconds;
                    model.StateMachine.Change(model, new Harvest());
                }
                else
                {
                    offset = (float)(DateTime.Now - model.StartTime).TotalSeconds;
                    model.StateMachine.Change(model, new Produce());
                }

                if (isExecute)
                {
                    model.StateMachine.Execute(model, offset);
                }
                if (model.State == WorkerState.Idle)
                {
                    this.CurIdle++;
                }
                this.WorkerList.Add(model);
            }
        }

        private void InitView()
        {
            this.View = Instantiate(WorkerItemPrefab, ParentAllWorkerItem);
            this.View.SetAmount(this.CurIdle, this.WorkerList.Count);
        }

        private void FixedUpdate()
        {
            if (this.WorkerList == null) { return; }
            this.OnlGame_CheckWorking();
        }

        public bool CheckExistPlotExceptId(int id, int idPlot)
        {
            foreach (WorkerModel model in this.WorkerList)
            {
                if (model.Id != id && model.IdPlot == idPlot)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckExistPlot(int idPlot)
        {
            foreach (WorkerModel model in this.WorkerList)
            {
                if (model.IdPlot != -1 && model.IdPlot == idPlot)
                {
                    return true;
                }
            }
            return false;
        }

        public int FindId_MaxStartTime()
        {
            int id = -1;
            int count = -1;
            DateTime max = DateTime.MinValue;
            foreach (WorkerModel model in this.WorkerList)
            {
                count++;
                if (model.StartTime != DateTime.MinValue && model.StartTime > max && !model.NotExecute)
                {
                    max = model.StartTime;
                    id = count;
                }
            }
            return id;
        }

        public int FindId_ByIdPlot(int id)
        {
            if (this.WorkerList == null) { return -1; }
            foreach(WorkerModel model in this.WorkerList)
            {
                if (model.IdPlot == id)
                {
                    return model.Id;
                }
            }
            return -1;
        }

        private bool Harvestable(PlotModel model)
        {
            if (model.Data.CurLife < 0 && model.Data.CurLife < 0)
            {
                return true;
            }
            return false;
        }

        public void OnlGame_CheckWorking()
        {
            foreach (WorkerModel model in this.WorkerList)
            {
                if (model.State != WorkerState.Idle) { continue; }
                Tuple<int, float> minDeadline = PlotController.Instance.GetMinDeadline();
                if (minDeadline.Item1 >= 0)
                {
                    if (
                        // Gần đến deadline
                        minDeadline.Item2 <= DataManager.Instance.GameConfig.WorkerConfig.TimeTask + Offset && 
                        // Chưa có Worker nào làm
                        !this.CheckExistPlotExceptId(model.Id, minDeadline.Item1) &&
                        // Đã chín
                        (Harvestable(PlotController.Instance.PlotModelList[minDeadline.Item1])))
                    {
                        // Thu hoạch (Harvest).
                        PlotController.Instance.PlotModelList[minDeadline.Item1].Data.WorkerId = model.Id;
                        model.IdPlot = minDeadline.Item1;
                        model.StateMachine.Change(model, new Harvest());
                        model.StateMachine.Execute(model);

                        this.CurIdle--;
                        this.View.SetAmount(this.CurIdle, this.WorkerList.Count);
                    }
                    else
                    {
                        Dictionary<int, DateTime> TimerForWorkerList = new Dictionary<int, DateTime>();
                        foreach (WorkerModel m in this.WorkerList)
                        {
                            TimerForWorkerList[m.Id] = m.StartTime;
                        }

                        // Chay Mô Phỏng, nếu chọn trồng có cây nào chết hay không.
                        // Không bị gì -> Thu hoạch.
                        if (PlotController.Instance.CheckDeadlineSkipTime(model.Id, this.WorkerList.Count - 1, TimerForWorkerList, model.TimeTask))
                        {
                            int plotIndex = PlotController.Instance.FindPlotAvai();
                            int productIndex = BagController.Instance.ChooseProduct();

                            // Không có Plot trống hoặc không còn đủ hạt giống --> Thu hoạch vì không trồng được.
                            if (plotIndex < 0 || productIndex < 0) 
                            {
                                if (Harvestable(PlotController.Instance.PlotModelList[minDeadline.Item1]))
                                {
                                    //Debug.Log(minDeadline.Item2 + " " + (DataManager.Instance.GameConfig.WorkerConfig.TimeTask + Offset));
                                    PlotController.Instance.PlotModelList[minDeadline.Item1].Data.WorkerId = model.Id;
                                    model.IdPlot = minDeadline.Item1;
                                    model.StateMachine.Change(model, new Harvest());
                                    model.StateMachine.Execute(model);

                                    this.CurIdle--;
                                    this.View.SetAmount(this.CurIdle, this.WorkerList.Count);
                                }
                                continue;
                            }
                            
                            // Trồng (Produce).
                            PlotController.Instance.PlotModelList[plotIndex].Data.WorkerId = model.Id;
                            model.IdPlot = plotIndex;
                            model.IdProduct = productIndex;
                            model.StateMachine.Change(model, new Produce());
                            model.StateMachine.Execute(model);

                            this.CurIdle--;
                            this.View.SetAmount(this.CurIdle, this.WorkerList.Count);
                        }
                        
                        // Chọn công việc sản xuất cho worker hiện tại vì sẽ có sản phẩm bị hủy nếu chọn trồng --> Harvest.
                        else
                        {
                            if (Harvestable(PlotController.Instance.PlotModelList[minDeadline.Item1]))
                            {
                                // Harvest
                                Debug.Log(minDeadline.Item2 + " " + (DataManager.Instance.GameConfig.WorkerConfig.TimeTask + Offset));
                                PlotController.Instance.PlotModelList[minDeadline.Item1].Data.WorkerId = model.Id;
                                model.IdPlot = minDeadline.Item1;
                                model.StateMachine.Change(model, new Harvest());
                                model.StateMachine.Execute(model);

                                this.CurIdle--;
                                this.View.SetAmount(this.CurIdle, this.WorkerList.Count);
                            }
                            
                        }
                    }
                }

                // Nếu không tim được MinDeadlin --> chưa có cây nào chín --> Chọn trồng.
                else
                {
                    int plotIndex = PlotController.Instance.FindPlotAvai();
                    int productIndex = BagController.Instance.ChooseProduct();
                    
                    // Không đủ điều kiện trồng, bỏ qua không làm gì.
                    if (plotIndex < 0 || productIndex < 0) { continue; }

                    PlotController.Instance.PlotModelList[plotIndex].Data.WorkerId = model.Id;
                    model.IdPlot = plotIndex;
                    model.IdProduct = productIndex;
                    model.StateMachine.Change(model, new Produce());
                    model.StateMachine.Execute(model);

                    this.CurIdle--;
                    this.View.SetAmount(this.CurIdle, this.WorkerList.Count);
                }
            }
        }


        // Chạy mô phỏng Off Game.
        public Dictionary<int, DateTime> ConvertWorkerListToDict_Sorted()
        {
            Dictionary<int, DateTime> result = new Dictionary<int, DateTime>();
            List<WorkerDetail> workerDetailList = new List<WorkerDetail>(DataManager.Instance.GameData.WorkerList);
            int n = workerDetailList.Count;

            for (int i = 0; i <= n - 2; ++i)
            {
                for (int j = 0; j <= n - 1; ++j)
                {
                    if (DateTime.Parse(workerDetailList[i].StartTime) > DateTime.Parse(workerDetailList[j].StartTime))
                    {
                        // Swap
                        WorkerDetail tmp = new WorkerDetail();
                        tmp.Id = workerDetailList[i].Id;
                        tmp.StartTime = workerDetailList[i].StartTime;

                        workerDetailList[i] = new WorkerDetail();
                        workerDetailList[i].Id = workerDetailList[j].Id;
                        workerDetailList[i].StartTime = workerDetailList[j].StartTime;

                        workerDetailList[j] = new WorkerDetail();
                        workerDetailList[j].Id = tmp.Id;
                        workerDetailList[j].StartTime = tmp.StartTime;
                    }
                }
            }

            foreach (WorkerDetail worker in workerDetailList)
            {
                if (DateTime.Parse(worker.StartTime) == DateTime.MinValue)
                {
                    result[int.Parse(worker.Id)] = DateTime.MinValue;
                }
                else
                {
                    // Gán luôn thời gian hoàn thành công việc.
                    result[int.Parse(worker.Id)] = DateTime.Parse(worker.StartTime)
                        .AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                }
            }

            return result;
        }

        public void OffGame_CheckWorking()
        {
            if (DateTime.TryParse(DataManager.Instance.GameData.Off, out DateTime result))
            {
                // Lấy PlotList từ Data lần tắt máy trước: Gồm cả trạng thái:
                // Produce chưa xong. 
                // Harvest chưa xong. 
                // Đang phát triển. 
                // Đang trống.
                List<PlotModel> plotTmp = new List<PlotModel>();
                plotTmp = PlotController.Instance.GetPlotListFromOldData();

                Dictionary<int, DateTime> workerTmp = new Dictionary<int, DateTime>();
                // Chuyển Worker --> Dictionary --> Sắp xếp thứ tự công việc.
                workerTmp = this.ConvertWorkerListToDict_Sorted();

                // Điều kiện dừng While
                bool check = true;

                int count = -1;

                //// Debug
                Debug.Log("Plot Table");

                // Mảng lưu lại thời gian bắt đầu thu hoạch --> Phục vụ cho việc Fill Data sau khi mô phỏng xong.
                List<DateTime> harvestTime = new List<DateTime>();

                // Đánh dấu lại thời giản worker có thể tác động vào mỗi Plot.
                List<DateTime> freeTimePlot = new List<DateTime>();

                foreach (PlotModel plot in plotTmp) 
                {
                    count++;
                    // Vừa Đọc Data cũ đã có Deadline.
                    if (DateTime.Parse(plot.Data.Deadline) != DateTime.MinValue)
                    {
                        // Có Worker đang tác động và Hoạt động thu hoạch.
                        if (plot.Data.WorkerId >= 0 &&
                            DataManager.Instance.GameData.WorkerList[plot.Data.WorkerId].State == WorkerState.Harvest)
                        {
                            ProductType tmpType = plotTmp[count].Data.ProductType;
                            int workerId = plot.Data.WorkerId;

                            // Free Time Plot Deadline sau khi sản phẩm đạt chuẩn.
                            freeTimePlot.Add(DateTime.Parse(plot.Data.Deadline));

                            // Thời gian bắt đầu thu hoạch.
                            harvestTime.Add(DateTime.Parse(DataManager.Instance.GameData.WorkerList[plot.Data.WorkerId].StartTime));

                            //Reset Data như đã thu hoạch.
                            plotTmp[count].ResetData(false);
                            plotTmp[count].SetNullData(false);
                            plotTmp[count].Data.ProductType = tmpType;
                            plotTmp[count].Data.WorkerId = workerId;
                            

                            ////  Debug.
                            Debug.Log(plot.Data.Deadline + " " + freeTimePlot[count] + " " + plot.DurationDeadline);
                            continue;
                        }
                        freeTimePlot.Add(DateTime.Parse(plot.Data.Deadline).AddSeconds(-plot.DurationDeadline));
                    }
                    else
                    {
                        freeTimePlot.Add(DateTime.Parse(plot.Data.Deadline));
                    }
                    harvestTime.Add(DateTime.MinValue);

                    //// Debug.
                    Debug.Log(plot.Data.Deadline + " " + freeTimePlot[count] + " " + plot.DurationDeadline + " " + harvestTime[count]);
                }

                //// Debug
                Debug.Log("Worker Table");
                foreach (int key in workerTmp.Keys)
                {
                    Debug.Log(workerTmp[key]);
                }

                while (check)
                {
                    bool progressed = false;
                    for (int i = 0; i < workerTmp.Count; ++i)
                    {
                        // Lấy thời gian gần nhất Worker có thể tác động lên Plot.
                        DateTime now = workerTmp.ElementAt(i).Value;
                        // Tìm thử thời gian gần nhất Plot rảnh.
                        if (now < FindFreetimeMin(freeTimePlot))
                        {
                            now = FindFreetimeMin(freeTimePlot);
                        }
                        if (now == DateTime.MinValue)
                        {
                            now = DateTime.Parse(DataManager.Instance.GameData.Off);
                        }


                        Debug.Log("Now: " + now);
                        if (now > DateTime.Now)
                        {
                            // Now vượt hiện tại --> dừng mô phỏng.
                            check = false;
                        }
                        else
                        {
                            // Xử lý Plot quá hạn mà worker chưa thể thu hoạch.
                            for (int j = 0; j < plotTmp.Count; ++j)
                            {
                                if (DateTime.Parse(plotTmp[j].Data.Deadline) < now)
                                {
                                    // Expired
                                    plotTmp[j].ResetData(false);
                                    plotTmp[j].SetNullData(false);
                                    harvestTime[j] = DateTime.MinValue;
                                }
                            }

                            // time thời gian nếu làm xong nếu worker nhận việc từ now.
                            DateTime time = now.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                            // Chạy mô phỏng, nếu chọn trồng có sản phẩm nào hỏng không,
                            // nếu có trả về index Plot chứa sản phẩm đó. 
                            int indexPlot = PlotController.Instance.OffGame_FindIndex_SkipHarvest
                                            (time, new List<PlotModel>(plotTmp), workerTmp.ElementAt(i).Key, 
                                            workerTmp.Count - 1, new Dictionary<int, DateTime>(workerTmp),
                                            DataManager.Instance.GameConfig.WorkerConfig.TimeTask);

                            // Nếu có và thời gian thu hoạch xong > thời gian hết hạn.
                            if (indexPlot >= 0 && time > DateTime.Parse(plotTmp[indexPlot].Data.Deadline))
                            {
                                // Harvest
                                ProductType tmpType = plotTmp[indexPlot].Data.ProductType; 
                                plotTmp[indexPlot].ResetData(false);
                                plotTmp[indexPlot].SetNullData(false);
                                plotTmp[indexPlot].Data.ProductType = tmpType;

                                // Gán Free Time Plot
                                freeTimePlot[indexPlot] = now.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);

                                var key = workerTmp.ElementAt(i).Key;
                                plotTmp[indexPlot].Data.WorkerId = i;
                                
                                // Gán thời gian làm xong cho Worker.
                                workerTmp[key] = now.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);

                                // Đã có làm việc
                                progressed = true;

                                // Thời gian bắt đầu thu hoạch.
                                harvestTime[indexPlot] = now;

                                //// Debug
                                Debug.Log(plotTmp[indexPlot].Data.ProductType + " Harvest " + indexPlot);
                            }
                            // Nếu không --> Ưu tiên chọn trồng để có nhiều sản phẩm.
                            else
                            {
                                
                                // Tìm thời gian gần now nhất tác động được lên Plot.
                                Tuple<int, bool> findPlot = FindPlotFree(now, new List<PlotModel>(plotTmp), freeTimePlot);

                                // Chọn Product từ Bag.
                                int idProduct = BagController.Instance.ChooseProduct();

                                // Nếu tìm thấy Plot và còn sản phẩm để sản xuất --> Sản xuất.
                                if (findPlot.Item2 && idProduct > 0)
                                {
                                    // Produce
                                    int indexPr = DataManager.Instance.GetIdProductConfig(BagController.Instance.BagModelList[idProduct - 1].ProductType) - 1;
                                    int interval = DataManager.Instance.GameConfig.ProductConfigList[indexPr].Interval;
                                    int lifetime = DataManager.Instance.GameConfig.ProductConfigList[indexPr].Lifetime;
                                    int durationDeadline = DataManager.Instance.GameConfig.ProductConfigList[indexPr].Deadline;

                                    plotTmp[findPlot.Item1].Interval = interval;
                                    plotTmp[findPlot.Item1].Lifetime = lifetime;
                                    plotTmp[findPlot.Item1].DurationDeadline = durationDeadline;
                                    plotTmp[findPlot.Item1].Data.ProductType =
                                        BagController.Instance.BagModelList[idProduct - 1].ProductType;

                                    // Giảm sản phẩm trong Bag
                                    BagController.Instance.CaculateProductAmountByName(
                                        plotTmp[findPlot.Item1].Data.ProductType.ToString(), -1);

                                    int timeTask = DataManager.Instance.GameConfig.WorkerConfig.TimeTask;
                                    // Tính Deadline
                                    plotTmp[findPlot.Item1].Data.Deadline = 
                                        now.AddSeconds(interval * lifetime + durationDeadline + timeTask).ToString();

                                    // Gán Free Time Plot;
                                    // Không thêm timeTask là do worker đã có thể thu hoạch từ khi sản phẩm đạt chuẩn.
                                    freeTimePlot[findPlot.Item1] = now.AddSeconds(interval * lifetime + timeTask);
                                   
                                    // Xử lý Worker và Đánh dấu đã làm 1 công việc.
                                    var key = workerTmp.ElementAt(i).Key;
                                    plotTmp[findPlot.Item1].Data.WorkerId = i;
                                    workerTmp[key] = now.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                                    progressed = true;

                                    //// Debug.
                                    Debug.Log("Produce " + findPlot.Item1 + " " + plotTmp[findPlot.Item1].Data.Deadline + " " + freeTimePlot[findPlot.Item1] + " " + plotTmp[findPlot.Item1].Data.ProductType);
                                }
                                // Ngược lại --> quay về chọn thu hoạch cái nào có thể.
                                else
                                {
                                    // Tìm sản phẩm có thể thu hoạch (tức đang có deadline).
                                    int idP = FindIndexPlotHaveDeadline(new List<PlotModel>(plotTmp));

                                    // Có sản phẩm và kịp thu hoạch.
                                    if (idP >= 0 && 
                                        DateTime.Parse(plotTmp[idP].Data.Deadline) <= now.AddSeconds(plotTmp[idP].DurationDeadline))
                                    {
                                        // Harvest
                                        ProductType tmpType = plotTmp[idP].Data.ProductType;
                                        plotTmp[idP].ResetData(false);
                                        plotTmp[idP].SetNullData(false);
                                        plotTmp[idP].Data.ProductType = tmpType;
                                        
                                        freeTimePlot[idP] = now.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                                        var key = workerTmp.ElementAt(i).Key;
                                        plotTmp[idP].Data.WorkerId = i;
                                        workerTmp[key] = now.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                                        progressed = true;

                                        harvestTime[idP] = now;

                                        //// Debug.
                                        Debug.Log(plotTmp[idP].Data.ProductType + " Harvest " + idP);
                                    }
                                }
                            }
                        }
                    }
                    if (!progressed)
                    {
                        check = false;
                    }
                }
                this.FillToPlot(plotTmp, harvestTime, freeTimePlot);
            }
        }

        // Chuẩn hóa và đẩy data sau khi chạy mo phỏng Off Game.
        private void FillToPlot(List<PlotModel> plotModelList, List<DateTime> harvestTime, List<DateTime> freeTimePlot)
        {
            DataManager.Instance.GameData.PlotList.Clear();
            // Tạo 1 List lưu lại List Worker Cũ trước khi tắt game.
            List<WorkerDetail> tmpWorkerList = new List<WorkerDetail>(DataManager.Instance.GameData.WorkerList);
            // Reset lại List Worker trong Data Manager.
            this.ResetData();

            int count = -1;
            // Duyệt Plot Model để tính toán dữ liệu mới sau chạy mô phỏng.
            foreach (PlotModel plotModel in plotModelList)
            {
                count++;
                WorkerDetail workerDetail = new WorkerDetail();
                PlotDetail plotDetail = new PlotDetail();
                if (plotModel.Data.Id.Length == 0)
                {
                    plotModel.Data.Id = count.ToString();
                }

                // Nếu Không có Deadline và đang có thể tác động 
                // ==> Rảnh rỗi.
                if (DateTime.Parse(plotModel.Data.Deadline) == DateTime.MinValue 
                    && freeTimePlot[int.Parse(plotModel.Data.Id)] <= DateTime.Now)
                {
                    // Reset Data
                    plotDetail.Id = plotModel.Data.Id;
                    plotDetail.Status = PlotStatus.IsAvai;
                    plotDetail.CurTime = -1;
                    plotDetail.CurLife = -1;
                    DataManager.Instance.GameData.PlotList.Add(plotDetail);

                    //// Debug
                    Debug.Log(plotDetail.Id + " " + plotDetail.ProductType + " " + plotDetail.Status + " " +
                    plotDetail.CurTime + " " + plotDetail.CurLife + " " + plotDetail.CurAmount + " "
                    + plotDetail.Deadline + " " + plotDetail.WorkerId);
                    continue;
                }

                // Đang Thu hoạch
                if (
                    (
                        // Không có Deadline nhưng Free Time Plot lại chưa xong 
                        // => Đang Thu hoạch.
                        DateTime.Parse(plotModel.Data.Deadline) == DateTime.MinValue
                        && freeTimePlot[int.Parse(plotModel.Data.Id)] > DateTime.Now
                    ) 
                    ||
                    (
                        // Có Deadline
                        // Có Worker Id
                        // Data Trước khi đọc vào Worker Id Ở trạng thái Thu hoạch.
                        // Không LifeTime và InterVal (Đã reset khi thực hiện hành động thu hoạch).
                        // => Đọc xuống lúc đang thu hoạch và bật game khi chưa hết thời gian thu hoạch đó.
                        // ==> Tức là thời lượng Off Game không đủ để thu hoạch xong.
                        DateTime.Parse(plotModel.Data.Deadline) != DateTime.MinValue &&
                        plotModel.Data.WorkerId >= 0 &&
                        tmpWorkerList[plotModel.Data.WorkerId].State == WorkerState.Harvest &&
                        plotModel.Lifetime < 0 && plotModel.Interval < 0
                    ) 
                   )
                {

                    //// Debug
                    Debug.Log("Harvest");

                    // Tránh trường hợp đã qua thời gian thu hoạch khi chạy mô phỏng nhưng chưa có worker nào nhận việc.
                    if (plotModel.Data.WorkerId >= 0)
                    {
                        // Gán lại thông tin cho worker để tiếp tục thu hoạch
                        workerDetail.Id = plotModel.Data.WorkerId.ToString();
                        // mảng harvestTime lưu thời gian bắt đầu thu hoạch như ở hàm OffGame_CheckWorking đã giải thích.
                        workerDetail.StartTime = harvestTime[int.Parse(plotModel.Data.Id)].ToString();
                        workerDetail.IdProduct = BagController.Instance.GetIndex(plotDetail.ProductType) + 1;
                        workerDetail.IdPlot = int.Parse(plotModel.Data.Id);
                        workerDetail.State = WorkerState.Harvest;
                        workerDetail.NotExcute = false;
                        // Đưa lại vào mảng DataManager Đã Reset ban nảy.
                        DataManager.Instance.GameData.WorkerList[int.Parse(workerDetail.Id)] = workerDetail;

                        //// Debug
                        Debug.Log(workerDetail.Id + " " + workerDetail.IdPlot + " " + workerDetail.IdProduct + " " +
                            workerDetail.State + " " + workerDetail.StartTime + " " + workerDetail.NotExcute);
                            plotDetail.WorkerId = plotModel.Data.WorkerId;

                    }
                    else
                    {
                        // Trường hợp đã qua thời gian thu hoạch khi chạy mô phỏng nhưng chưa có worker nào nhận việc.
                        plotDetail.Deadline = plotModel.Data.Deadline;
                    }

                    // Gán lại Data Plot
                    plotDetail.ProductType = plotModel.Data.ProductType;
                    if (plotDetail.ProductType != ProductType.None)
                    {
                        //// Debug
                        Debug.Log(plotDetail.ProductType);

                        // Lấy thông tin Product Config
                        ProductConfig config = DataManager.Instance.GetProductConfig(plotDetail.ProductType);
                        plotDetail.Id = plotModel.Data.Id;
                        plotDetail.CurAmount = config.Lifetime - plotModel.Data.CurLife;
                        plotDetail.Status = PlotStatus.NotIsAvai;
                        plotDetail.CurLife = -1;
                        plotDetail.CurTime = -1;
                    }
                    
                }
                // Không phải đang thu hoạch
                else
                {
                    DateTime deadline = DateTime.Parse(plotModel.Data.Deadline);

                    // Tính Start Time bằng cách dùng deadline trừ:
                    // Độ dài hỏng sản phẩm sau chín.
                    // Độ dài phát triển.
                    // Độ dài để worker sản xuất.
                    DateTime startTime = deadline;
                    startTime = startTime.AddSeconds(-plotModel.DurationDeadline);
                    startTime = startTime.AddSeconds(-plotModel.Interval * plotModel.Lifetime);
                    startTime = startTime.AddSeconds(-DataManager.Instance.GameConfig.WorkerConfig.TimeTask);

                    // Mốc thời gian hoàn thành sản xuất.
                    DateTime doneProduce = startTime.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                    
                    // Mốc thời gian hoàn thành phát triển.
                    DateTime doneDevProduct = doneProduce.AddSeconds(plotModel.Interval * plotModel.Lifetime + 
                        plotModel.DurationDeadline);

                    // Chưa vượt qua doneProduce --> Đang Produce.
                    if (DateTime.Now < doneProduce)
                    {
                        //// Debug
                        Debug.Log("Produce");

                        // Worker
                        if (plotModel.Data.WorkerId >= 0)
                        {
                            workerDetail.Id = plotModel.Data.WorkerId.ToString();
                            workerDetail.StartTime = startTime.ToString();
                            workerDetail.IdProduct = BagController.Instance.GetIndex(plotModel.Data.ProductType) + 1;
                            workerDetail.IdPlot = int.Parse(plotModel.Data.Id);
                            workerDetail.State = WorkerState.Produce;
                            workerDetail.NotExcute = false;

                            // Đã trừ khi qua chạy mô phỏng nhưng chưa trồng xong --> trả lại cho Bag.
                            BagController.Instance.CaculateProductAmountByName(plotModel.Data.ProductType.ToString(), 1);

                            // Đưa lại Data Worker
                            DataManager.Instance.GameData.WorkerList[int.Parse(workerDetail.Id)] = workerDetail;

                            //// Debug
                            Debug.Log(workerDetail.Id + " " + workerDetail.IdPlot + " " + workerDetail.IdProduct + " " +
                                workerDetail.State + " " + workerDetail.StartTime + " " + workerDetail.NotExcute);
                        }

                        // Gán Data Plot
                        PlotModel plotTmp = new PlotModel();
                        plotTmp.Data = plotModel.Data;
                        plotTmp.ResetData(false);
                        plotTmp.SetNullData(false);
                        plotDetail = plotTmp.Data;
                        plotDetail.Id = plotModel.Data.Id;
                        plotDetail.WorkerId = plotModel.Data.WorkerId;

                    }
                    // Chưa qua doneDev tức đang phát triển.
                    else if (DateTime.Now < doneDevProduct)
                    {
                        //// Debug
                        Debug.Log("Dev");

                        // Tính và gán lại Data cho Plot
                        float dis = (float)(DateTime.Now - doneProduce).TotalSeconds;
                        int curLife = (int)(dis / plotModel.Interval);
                        int curTime = (int)(dis - curLife * plotModel.Interval);
                        plotDetail.CurLife = curLife;
                        plotDetail.CurTime = curTime;
                        plotDetail.CurAmount = curLife;
                        plotDetail.Status = PlotStatus.NotIsAvai;
                        plotDetail.ProductType = plotModel.Data.ProductType;
                        plotDetail.Id = plotModel.Data.Id;
                        plotDetail.Deadline = DateTime.MinValue.ToString();

                    }
                }
                

                //// Debug
                Debug.Log(plotDetail.Id + " " + plotDetail.ProductType + " " + plotDetail.Status + " " +
                    plotDetail.CurTime + " " + plotDetail.CurLife + " " + plotDetail.CurAmount + " " 
                    + plotDetail.Deadline + " " + plotDetail.WorkerId + " " 
                    + freeTimePlot[int.Parse(plotModel.Data.Id)]);
                // Đưa data lại cho DataManager
                DataManager.Instance.GameData.PlotList.Add(plotDetail);
            }
        }

        private Tuple<int, bool> FindPlotFree(DateTime now, List<PlotModel> plotTmp, List<DateTime> freeTimePlot)
        {
            int count = -1;
            DateTime min = DateTime.MaxValue;
            int index = -1;

            foreach (PlotModel plot in plotTmp)
            {
                count++;
                if (DateTime.Parse(plot.Data.Deadline) == DateTime.MinValue)
                {
                    if (now >= freeTimePlot[count])
                    {
                        return new Tuple<int, bool>(count, true);
                    }
                    else
                    {
                        if (min > freeTimePlot[count]) 
                        { 
                            min = freeTimePlot[count];
                            index = count;
                        }
                    }
                }
            }

            return new Tuple<int, bool>(index, false);
        }

        private int FindIndexPlotHaveDeadline(List<PlotModel> plotTmp)
        {
            int count = -1;
            DateTime min = DateTime.MaxValue;
            int index = -1;

            foreach (PlotModel plot in plotTmp) 
            {
                count++;
                if (DateTime.Parse(plot.Data.Deadline) != DateTime.MinValue)
                {
                    if (min > DateTime.Parse(plot.Data.Deadline))
                    {
                        min = DateTime.Parse(plot.Data.Deadline);
                        index = count;
                    }
                }
            }
            return index;
        }

        private DateTime FindFreetimeMin(List<DateTime> freeTimePlot)
        {
            DateTime min = DateTime.MaxValue;
            foreach (DateTime dt in freeTimePlot)
            {
                if (dt < min)
                {
                    min = dt;
                }
            }
            return min;
        }

        public void SaveData()
        {
            DataManager.Instance.GameData.WorkerList.Clear();
            foreach (WorkerModel model in this.WorkerList)
            {
                WorkerDetail worker = new WorkerDetail();
                worker.Id = model.Id.ToString();
                worker.State = model.State;
                worker.StartTime = model.StartTime.ToString();
                worker.IdProduct = model.IdProduct;
                worker.IdPlot = model.IdPlot;
                worker.NotExcute = model.NotExecute;

                DataManager.Instance.GameData.WorkerList.Add(worker);
            }
        }
    }
}
