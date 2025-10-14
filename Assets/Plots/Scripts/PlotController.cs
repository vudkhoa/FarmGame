using Bag.Controller;
using Data.Config;
using Data.Game;
using Data.Manager;
using Plots.Model;
using Plots.View;
using Product.Controller;
using Sell.Controller;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.DesignPattern.Singleton;
using Worker.Controller;

namespace Plots.Controller
{
    public enum PlotStatus
    {
        None = 0, 
        IsAvai = 1,
        NotIsAvai = 2
    }

    public class PlotController : SingletonMono<PlotController>
    {
        [Header(" Plots Controller Setting ")]
        public PlotView PlotPrefab;
        public RectTransform ParentAllView; 

        [Header(" Running Game ")]
        public List<PlotModel> PlotModelList;
        private bool IsActing;

        public void Init() 
        {
            this.IsActing = false;
            this.PlotModelList = new List<PlotModel>();
            this.GetData();
        }

        public void GetData()
        {
            int count = -1;
            foreach (PlotDetail plot in DataManager.Instance.GameData.PlotList)
            {
                // Khởi tạo Plot ở trạng thái Idle.
                count++;
                PlotModel model = new PlotModel();
                plot.Id = count.ToString();
                PlotView view = Instantiate(PlotPrefab, ParentAllView);
                model.Init(plot, view, count);


                int interval = 0;
                int lifetime = 0;
                int durationDeadline = 0;
                
                // Nếu có Product Type --> Get các thông số từ Product Config.
                if (plot.ProductType != ProductType.None)
                {
                    interval = 0;
                    lifetime = 0;
                    durationDeadline = 0;

                    List<ProductConfig> productConfigs = new List<ProductConfig>();
                    productConfigs = DataManager.Instance.GameConfig.ProductConfigList;
                    foreach (ProductConfig product in productConfigs)
                    {
                        if (product.Name.ToString() == plot.ProductType.ToString())
                        {
                            interval = product.Interval;
                            lifetime = product.Lifetime;
                            durationDeadline = product.Deadline;
                            break;
                        }
                    }
                    
                    // Setup lại trạng thái
                    // Đang sản xuất. 
                    // Đang phát triển. 
                    // Đang thu hoạch.
                    model.Setup(plot.ProductType, interval, lifetime); 

                    // Có thể thu hoạch thì reset luôn Data.
                    if (plot.CurLife < 0 && plot.CurTime < 0)
                    {
                        model.SetCurLife(0);
                        model.SetCurTime(0f, 0);
                        model.ResetData();
                        model.Data.CurAmount = model.Lifetime;
                        model.View.SetAmount(model.Data.CurAmount);
                    }
                }
                model.DurationDeadline = durationDeadline;

                //// Debug
                Debug.Log(model.Data.Id + " "
                + model.Data.ProductType + " "
                + model.Data.Status + " "
                + model.Data.CurTime + " "
                + model.Data.CurLife + " "
                + model.Data.CurAmount + " "
                + model.Data.Deadline + " "
                + model.Data.WorkerId + " "
                + model.Interval + " "
                + model.Lifetime + " " +
                model.DurationDeadline);

                // Add Data
                PlotModelList.Add(model);
            }
            this.IsActing = true;
        }

        // Vừa tìm Plot trống vừa xử lý Click.
        // Nếu người chơi Click vào ô worker đang thu hoạch, loại bỏ hoạt động của worker và thu hoạch ngay lập tức.
        public int FindPlotAvai(bool isClick = false)
        {
            if (!this.IsActing) {  return -1; }
            if (this.PlotModelList == null) {  return -1; }
            int count = -1;
            int coutNotAvai = 0;
            foreach (PlotModel model in this.PlotModelList)
            {
                count++;
                if (model.Data.Status == PlotStatus.NotIsAvai)
                {
                    coutNotAvai++;
                }

                // IsAvai và chưa có Worker nào đang tác động, tránh 2 worker cùng tác động 1 plot.
                if (model.Data.Status == PlotStatus.IsAvai && !WorkerController.Instance.CheckExistPlot(int.Parse(model.Data.Id)))
                {
                    return count;
                }
            }
            if (!isClick || coutNotAvai == this.PlotModelList.Count)
            {
                return -1;
            }

            // Nếu là người chơi bấm --> ưu tiên người chơi.
            // Tìm Worker 
            count = WorkerController.Instance.FindId_MaxStartTime();

            // Gán Not Execute.
            WorkerController.Instance.WorkerList[count].NotExecute = true;
            int newIdPlot = WorkerController.Instance.WorkerList[count].IdPlot;

            // Dừng State (Do WorkerModel và StateMachine của WorkerModel quản lý) ngay lập tức.
            WorkerController.Instance.WorkerList[count].StateMachine
                .CancelNow(WorkerController.Instance.WorkerList[count]);

            return newIdPlot;
        }
    
        // Đưa Data qua Plot (Click trồng trọt từ người chơi).
        public void SetPlotByIndex(int index, ProductType productType)
        {
            this.PlotModelList[index].Data.Status = PlotStatus.NotIsAvai;

            int interval = 0;
            int lifetime = 0;
            int durationDeadline = 0;

            List<ProductConfig> productConfigs = new List<ProductConfig>();
            productConfigs = DataManager.Instance.GameConfig.ProductConfigList;
            foreach (ProductConfig product in productConfigs)
            {
                if (product.Name.ToString() == productType.ToString())
                {
                    interval = product.Interval;
                    lifetime = product.Lifetime;
                    durationDeadline = product.Deadline;
                    break;
                }
            }
            this.PlotModelList[index].SetCurLife(1);
            this.PlotModelList[index].Setup(productType, interval, lifetime);
            this.PlotModelList[index].SetCurTime(0f, 0);
            this.PlotModelList[index].DurationDeadline = durationDeadline;

        }

        // Đưa Data qua Sell (Click Thu Hoạch từ người chơi).
        public void MoveToSell(int id)
        {
            Debug.Log(this.PlotModelList[id].Lifetime);
            if (this.PlotModelList[id].Data.CurAmount <= 0 || this.PlotModelList[id].Data.CurAmount != this.PlotModelList[id].Lifetime)
            {
                return;
            }

            // Tìm Sell Phù hợp (cùng loại product).
            int index = SellController.Instance.FindIndexByProductType(this.PlotModelList[id].Data.ProductType);
            if (index == -1) { return; }

            // Tính lại Sell.
            SellController.Instance.CaculateAmountByIndex(index, this.PlotModelList[id].Data.CurAmount);
            
            // Giám số lượng trong Model.
            this.PlotModelList[id].ReduceAmount(this.PlotModelList[id].Data.CurAmount);

            // Loại bỏ nếu có Worker đang nhận công việc thu hoạch.
            int findId = WorkerController.Instance.FindId_ByIdPlot(id);
            if (findId < 0) { return; }
            WorkerController.Instance.WorkerList[findId].NotExecute = true;
            WorkerController.Instance.WorkerList[findId].StateMachine
                .CancelNow(WorkerController.Instance.WorkerList[findId]);
        }

        // Tăng thời gian của plot sau mỗi giây
        public void IncreaseTime(float time, int id)
        {
            float timeModel = this.PlotModelList[id].Data.CurTime + time;
            if (this.PlotModelList[id].Data.CurLife >= this.PlotModelList[id].Lifetime)
            {
                //// Debug
                Debug.Log("Harvest");

                this.PlotModelList[id].CaculateDeadline();
                this.PlotModelList[id].SetCurLife((-1) * (this.PlotModelList[id].Data.CurLife + 1));
                this.PlotModelList[id].SetCurTime(0f, 0);
                this.PlotModelList[id].ResetData();
                this.PlotModelList[id].Data.CurAmount = this.PlotModelList[id].Lifetime;
                this.PlotModelList[id].View.SetAmount(this.PlotModelList[id].Data.CurAmount);
                return;
            }

            // Trong giới hạn Interval
            if (timeModel < this.PlotModelList[id].Interval)
            {
                this.PlotModelList[id].SetCurTime(timeModel, 0);
            }
            // Vượt giới hạn Interval, tăng lifeTime.
            else
            {
                // Trong giới hạn Lifetime.
                if (this.PlotModelList[id].Data.CurLife + 1 <
                    this.PlotModelList[id].Lifetime)
                {
                    this.PlotModelList[id].SetCurLife(1);
                    this.PlotModelList[id].SetCurTime(0f, 1);
                }
                // Hoàn thành vòng đời (Vượt Lifetime).
                else
                {
                    Debug.Log("Har");
                    this.PlotModelList[id].CaculateDeadline();
                    this.PlotModelList[id].SetCurLife((-1) * (this.PlotModelList[id].Data.CurLife + 1));
                    this.PlotModelList[id].SetCurTime(0f, 1);
                    this.PlotModelList[id].ResetData(); 
                    this.PlotModelList[id].Data.CurAmount = this.PlotModelList[id].Lifetime;
                    this.PlotModelList[id].View.SetAmount(this.PlotModelList[id].Data.CurAmount);
                }
            }
        }

        // Chưa thu hoạch
        public bool CheckUnharvested(PlotModel model)
        {
            if (model.Data.CurLife > 0 && model.Data.CurTime > 0) { return true; }
            return false;
        }

        // Có Deadline
        private bool CheckHaveDeadline(PlotModel model)
        {
            if (CheckUnharvested(model))
            {
                return false;
            }
            if (DateTime.Parse(model.Data.Deadline) == DateTime.MinValue) { return false; }
            return true;
        }

        // Chọn thằng có Deadline nhỏ nhất và đủ thời gian thu hoạch, nếu không đủ thời gian (deadlineTime < timeTask của worker).
        public Tuple<int, float> GetMinDeadline()
        {
            int i = -1;
            int index = -1;
            float minDeadline = float.MaxValue;
            if (this.PlotModelList == null) return new Tuple<int, float>(index, minDeadline);
            foreach (PlotModel model in this.PlotModelList)
            {
                i++;
                if (!CheckHaveDeadline(model) || model.Data.WorkerId != -1) { continue; }

                float deadlineDistance = (float)(DateTime.Parse(model.Data.Deadline) - DateTime.Now).TotalSeconds;
                if (deadlineDistance < minDeadline &&
                    deadlineDistance >= DataManager.Instance.GameConfig.WorkerConfig.TimeTask)
                {
                    minDeadline = deadlineDistance;
                    index = i;
                }
            }
            return new Tuple<int, float>(index, minDeadline);
        }

        public void SetNullById(int id)
        {
            this.PlotModelList[id].ResetData();
            this.PlotModelList[id].SetNullData();
            int findId = WorkerController.Instance.FindId_ByIdPlot(id);
            Debug.Log(findId);
            if (findId < 0) { return; }
            WorkerController.Instance.WorkerList[findId].NotExecute = true;
            WorkerController.Instance.WorkerList[findId].StateMachine
                .CancelNow(WorkerController.Instance.WorkerList[findId]);
        }

        // Dùng 2 con trỏ xử lý Skip Time(tức chọn Produce thay vì thu hoạch) cho OnlGame_CheckWorking.
        public bool CheckDeadlineSkipTime(int idWorker, int maxIdWorker, Dictionary<int, DateTime> TimerForWorkerList, int timeTask)
        {
            // Cho là thằng Worker nay đã chọn Produce, bỏ qua nó.
            TimerForWorkerList[idWorker] = DateTime.Now;
            List<PlotModel> tmpPlotModelList = new List<PlotModel>(this.PlotModelList);
            // Sorting lại Task: Gần tới Deadline đứng trước, không có deadline đưa hết về sau.
            for (int i = 0; i <= tmpPlotModelList.Count - 2; i++)
            {
                for (int j = i + 1; j <= tmpPlotModelList.Count - 1; j++)
                {
                    if (
                        (!this.CheckHaveDeadline(tmpPlotModelList[i]) && this.CheckHaveDeadline(tmpPlotModelList[j])) ||
                        (this.CheckHaveDeadline(tmpPlotModelList[i]) && this.CheckHaveDeadline(tmpPlotModelList[j]) &&
                        DateTime.Parse(tmpPlotModelList[i].Data.Deadline) > DateTime.Parse(tmpPlotModelList[j].Data.Deadline))
                        )
                    {
                        PlotModel tmp = tmpPlotModelList[i];
                        tmpPlotModelList[i] = tmpPlotModelList[j];
                        tmpPlotModelList[j] = tmp;
                    }

                }
            }

            // Chạy từng task đã sắp.
            int idTask = -1;
            while (idTask < tmpPlotModelList.Count - 1)
            {
                idWorker++;
                idTask++;
                // Xoay vòng Worker giả xử nó luôn chọn thu hoạch, cho tới khi hết Task.
                if (idWorker > maxIdWorker)
                {
                    idWorker = 0;
                }
                DateTime doneTask;
                // Task không có deadline, tức đằng sau cũng chỉ toàn Task không có deadline.
                if (!this.CheckHaveDeadline(tmpPlotModelList[idTask])) { return true; }

                // Nếu còn task
                // Tính Time Worker rảnh để Harvest
                if (TimerForWorkerList[idWorker] == DateTime.MinValue)
                {
                    TimerForWorkerList[idWorker] = DateTime.Now;
                    doneTask = DateTime.Now;
                }
                else
                {
                    doneTask = TimerForWorkerList[idWorker].AddSeconds(timeTask);
                }

                // Nếu miss Task return false.
                if (DateTime.Parse(tmpPlotModelList[idTask].Data.Deadline) < doneTask)
                {
                    return false;
                }
                // Cho rằng worker chọn thu hoạch và gán lại thời gian khi làm xong.
                TimerForWorkerList[idWorker] = doneTask.AddSeconds(timeTask);
            }
            return true;
        }

        // Dùng 2 con trỏ xử lý Skip Time(tức chọn Produce thay vì thu hoạch) cho OffGame_CheckWorking.
        // tương tự onlgame, nhưng trả về index thằng bị miss để thu hoạch luôn trong mô phỏng.
        public int OffGame_FindIndex_SkipHarvest(DateTime time, List<PlotModel> plotList, int idWorker, int maxIdWorker, Dictionary<int, DateTime> TimerForWorkerList, int timeTask)
        {
            TimerForWorkerList[idWorker] = time;
            List<PlotModel> tmpPlotModelList = new List<PlotModel>(plotList);
            // Sorting
            for (int i = 0; i <= tmpPlotModelList.Count - 2; i++)
            {
                for (int j = i + 1; j <= tmpPlotModelList.Count - 1; j++)
                {
                    if (
                        (!this.CheckHaveDeadline(tmpPlotModelList[i]) && this.CheckHaveDeadline(tmpPlotModelList[j])) ||
                        (this.CheckHaveDeadline(tmpPlotModelList[i]) && this.CheckHaveDeadline(tmpPlotModelList[j]) &&
                        DateTime.Parse(tmpPlotModelList[i].Data.Deadline) > DateTime.Parse(tmpPlotModelList[j].Data.Deadline))
                        )
                    {
                        PlotModel tmp = tmpPlotModelList[i];
                        tmpPlotModelList[i] = tmpPlotModelList[j];
                        tmpPlotModelList[j] = tmp;
                    }

                }
            }

            int idTask = -1;
            while (idTask < tmpPlotModelList.Count - 1)
            {
                idWorker++;
                idTask++;
                if (idWorker > maxIdWorker)
                {
                    idWorker = 0;
                }

                if (!this.CheckHaveDeadline(tmpPlotModelList[idTask])) { return -1; }

                if (TimerForWorkerList[idWorker] == DateTime.MinValue)
                {
                    TimerForWorkerList[idWorker] = time;
                }
                else
                {
                    TimerForWorkerList[idWorker] = TimerForWorkerList[idWorker].AddSeconds(timeTask);
                }

                if (DateTime.Parse(tmpPlotModelList[idTask].Data.Deadline) < TimerForWorkerList[idWorker])
                {
                    return int.Parse(tmpPlotModelList[idTask].Data.Id);
                }
                TimerForWorkerList[idWorker] = DateTime.Parse(tmpPlotModelList[idTask].Data.Deadline).AddSeconds(timeTask);

            }
            return -1;
        }

        // Lấy Data đúng trạng thái từ lần OffGame trước.
        public List<PlotModel> GetPlotListFromOldData()
        {
            List<PlotModel> result = new List<PlotModel>();
            foreach (PlotDetail plot in DataManager.Instance.GameData.PlotList)
            {
                PlotModel model = new PlotModel();
                model.Data = plot;

                // Nếu có Worker (Harvest, Produce), gán lại Product Type để đủ data chạy mô phỏng.
                if (plot.WorkerId >= 0 && DataManager.Instance.GameData.WorkerList[model.Data.WorkerId].State == WorkerState.Produce)
                {
                    plot.ProductType = BagController.Instance.BagModelList[
                        DataManager.Instance.GameData.WorkerList[model.Data.WorkerId].IdProduct - 1].ProductType;
                    model.Data.ProductType = plot.ProductType;
                }

                // Harvest, Produce
                if (plot.ProductType != ProductType.None)
                {
                    // Gán thông tin từ product config.
                    int indexProductConfig = DataManager.Instance.GetIdProductConfig(plot.ProductType);
                    if (indexProductConfig > 0)
                    {
                        model.Interval = DataManager.Instance.GameConfig.ProductConfigList[indexProductConfig - 1].Interval;
                        model.Lifetime = DataManager.Instance.GameConfig.ProductConfigList[indexProductConfig - 1].Lifetime;
                        model.DurationDeadline = DataManager.Instance.GameConfig.ProductConfigList[indexProductConfig - 1].Deadline;
                    }

                    // Chưa có Worker
                    // CurTime và CurLife vẫn đang đếm
                    // => Đang Dev.
                    if (plot.WorkerId < 0 && model.Data.CurTime >= 0 && model.Data.CurLife >= 0)
                    {
                        // Tính Deadline từ khi từ hiện tại đến khi phát triển xong và hết thời gian thu hoạch. 
                        DateTime deadline = DateTime.Parse(DataManager.Instance.GameData.Off);
                        deadline = deadline.AddSeconds(model.Interval - model.Data.CurTime);
                        deadline = deadline.AddSeconds((model.Lifetime - (model.Data.CurLife + 1)) * model.Interval);
                        deadline = deadline.AddSeconds(model.DurationDeadline);
                        model.Data.Deadline = deadline.ToString();
                    }
                    // Có Worker nhận, tức Đang Produce/Harvest
                    else if (plot.WorkerId >= 0)
                    {
                        // Đang Produce
                        if (DataManager.Instance.GameData.WorkerList[model.Data.WorkerId].State == WorkerState.Produce)
                        {
                            // Tính Deadline từ khi từ khi bắt đầu trồng đến khi phát triển xong và hết thời gian thu hoạch.
                            BagController.Instance.CaculateProductAmountByName(DataManager.Instance.GameConfig.ProductConfigList[indexProductConfig - 1].Name, -1);
                            DateTime deadline = DateTime.Parse(DataManager.Instance.GameData.WorkerList[model.Data.WorkerId].StartTime);
                            deadline = deadline.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                            deadline = deadline.AddSeconds(model.Lifetime * model.Interval);
                            deadline = deadline.AddSeconds(model.DurationDeadline);
                            model.Data.Deadline = deadline.ToString();
                        }
                        // Đang Harvest
                        else if (DataManager.Instance.GameData.WorkerList[model.Data.WorkerId].State == WorkerState.Harvest)
                        {
                            // Tính Deadline khi thu hoạch xong (hết TimeTask).
                            DateTime deadline = DateTime.Parse(DataManager.Instance.GameData.WorkerList[model.Data.WorkerId].StartTime);
                            deadline = deadline.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                            model.Data.Deadline = deadline.ToString();
                        }
                    }
                    

                }

                //// Debug
                Debug.Log(DataManager.Instance.GameData.Off + "; Product: " + model.Data.ProductType.ToString() + " "
                    + model.Interval + " " + model.Lifetime + " " + model.DurationDeadline + " "
                    + model.Data.CurTime + " " + model.Data.CurLife + "Deadline: " + model.Data.Deadline
                    + " " + model.Data.WorkerId
                    );
                if (model.Data.WorkerId >= 0)
                {
                    Debug.Log(DataManager.Instance.GameData.WorkerList[model.Data.WorkerId].StartTime + " " +
                                DataManager.Instance.GameData.WorkerList[model.Data.WorkerId].IdProduct + " " +
                                DataManager.Instance.GameData.WorkerList[model.Data.WorkerId].State);
                }

                result.Add(model);
            }

            return result;
        }


        public void SaveData()
        {
            DataManager.Instance.GameData.PlotList.Clear();

            foreach (PlotModel model in this.PlotModelList)
            {
                PlotDetail plot = new PlotDetail();
                plot.Id = model.Data.Id;
                plot.ProductType = model.Data.ProductType;
                plot.Status = model.Data.Status;
                plot.CurTime = model.Data.CurTime;
                plot.CurLife = model.Data.CurLife;
                plot.CurAmount = model.Data.CurAmount;
                plot.Deadline = model.Data.Deadline;
                plot.WorkerId = model.Data.WorkerId;
                DataManager.Instance.GameData.PlotList.Add(plot);
            }
        }
    }
}