using Bag.Controller;
using Data.Config;
using Data.Game;
using Data.Manager;
using Game.Manager;
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
            this.GetData();
            this.OffGame_CheckWorking();
            this.GetData(true);
            PlotController.Instance.Init();
            this.InitView();
        }

        private void GetData(bool isExecute = false)
        {
            this.WorkerList = new List<WorkerModel>();
            this.CurIdle = 0;
            foreach (WorkerDetail worker in DataManager.Instance.GameData.WorkerList)
            {
                WorkerModel model = new WorkerModel();
                model.Id = int.Parse(worker.Id);
                model.State = worker.State;
                model.IdProduct = worker.IdProduct;
                model.IdPlot = worker.IdPlot;
                model.TimeTask = DataManager.Instance.GameConfig.WorkerConfig.TimeTask;
                model.StartTime = DateTime.Parse(worker.StartTime);
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
                //Debug.Log(model.StartTime);
                if (isExecute)
                {
                    model.StateMachine.Execute(model, offset);
                }
                model.NotExecute = worker.NotExcute;
                if (model.State == WorkerState.Idle)
                {
                    this.CurIdle++;
                }
                this.WorkerList.Add(model);
            }
        }

        private void InitGetData()
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

        public bool CheckExist(int id, int idPlot)
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

        public bool CheckExistIdPlot(int idPlot)
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
                    //id = model.IdPlot;
                }
            }
            return id;
        }

        public int FindIdWorkerByIdPlot(int id)
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

        public void OnlGame_CheckWorking()
        {
            foreach (WorkerModel model in this.WorkerList)
            {
                if (model.State != WorkerState.Idle) { continue; }
                Tuple<int, float> minDeadline = PlotController.Instance.CheckMinDeadline();
                if (minDeadline.Item1 >= 0)
                {
                    if (
                        (
                        minDeadline.Item2 <= DataManager.Instance.GameConfig.WorkerConfig.TimeTask + Offset && !this.CheckExist(model.Id, minDeadline.Item1)
                        ) &&
                        (
                        PlotController.Instance.PlotModelList[minDeadline.Item1].Data.CurLife < 0 &&
                        PlotController.Instance.PlotModelList[minDeadline.Item1].Data.CurLife < 0
                        )
                        
                        )
                    {
                        //Debug.Log(minDeadline.Item2 + " " + (DataManager.Instance.GameConfig.WorkerConfig.TimeTask + Offset));
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

                        if (PlotController.Instance.CheckDeadlineSkipTime(model.Id, this.WorkerList.Count - 1, TimerForWorkerList, model.TimeTask))
                        {
                            int plotIndex = PlotController.Instance.FindPlotAvai();
                            int productIndex = BagController.Instance.ChooseProduct();

                            if (plotIndex < 0 || productIndex < 0) 
                            {
                                if (
                                    (PlotController.Instance.PlotModelList[minDeadline.Item1].Data.CurLife < 0 &&
                                    PlotController.Instance.PlotModelList[minDeadline.Item1].Data.CurLife < 0)
                                    )
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

                            PlotController.Instance.PlotModelList[plotIndex].Data.WorkerId = model.Id;
                            model.IdPlot = plotIndex;
                            model.IdProduct = productIndex;
                            model.StateMachine.Change(model, new Produce());
                            model.StateMachine.Execute(model);

                            this.CurIdle--;
                            this.View.SetAmount(this.CurIdle, this.WorkerList.Count);
                        }
                        else
                        {
                            if (
                                (PlotController.Instance.PlotModelList[minDeadline.Item1].Data.CurLife < 0 &&
                                PlotController.Instance.PlotModelList[minDeadline.Item1].Data.CurLife < 0)
                                )
                            {
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
                else
                {
                    int plotIndex = PlotController.Instance.FindPlotAvai();
                    int productIndex = BagController.Instance.ChooseProduct();
                    
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

        public Dictionary<int, DateTime> OffGame_GetWorkerTmpSorted()
        {
            Dictionary<int, DateTime> result = new Dictionary<int, DateTime>();
            List<WorkerDetail> workerDetailList = new List<WorkerDetail>(DataManager.Instance.GameData.WorkerList);
            int n = workerDetailList.Count;

            for (int i = 0; i <= n - 2; ++i)
            {
                for (int j = 0; j <= n - 1; ++j)
                {
                    if (
                        (
                            DateTime.Parse(workerDetailList[i].StartTime) > DateTime.Parse(workerDetailList[j].StartTime)
                        )
                       )
                    {

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
                double timeDistance = GameManager.Instance.GetDistanceWithNow(result);
                Debug.Log(timeDistance);
                List<PlotModel> plotTmp = new List<PlotModel>();

                plotTmp = PlotController.Instance.OffGame_GetPlotTmp();
                List<DateTime> freeTimePlot = new List<DateTime>();

                int c = -1;
                Debug.Log("Plot Table");
                List<DateTime> harvestTime = new List<DateTime>();
                foreach (PlotModel plot in plotTmp) 
                {
                    c++;
                    if (DateTime.Parse(plot.Data.Deadline) != DateTime.MinValue)
                    {
                        if (plot.Data.WorkerId >= 0 &&
                            DataManager.Instance.GameData.WorkerList[plot.Data.WorkerId].State == WorkerState.Harvest)
                        {
                            ProductType tmpType = plotTmp[c].Data.ProductType;
                            int workerId = plot.Data.WorkerId;
                            freeTimePlot.Add(DateTime.Parse(plot.Data.Deadline));
                            harvestTime.Add(DateTime.Parse(DataManager.Instance.GameData.WorkerList[plot.Data.WorkerId].StartTime));
                            plotTmp[c].ResetData(false);
                            plotTmp[c].SetNullData(false);
                            plotTmp[c].Data.ProductType = tmpType;
                            plotTmp[c].Data.WorkerId = workerId;

                            
                            Debug.Log(plot.Data.Deadline + " " + freeTimePlot[c] + " " + plot.DurationDeadline);
                            continue;
                        }
                        freeTimePlot.Add(DateTime.Parse(plot.Data.Deadline).AddSeconds(-plot.DurationDeadline));
                    }
                    else
                    {
                        freeTimePlot.Add(DateTime.Parse(plot.Data.Deadline));
                    }
                    harvestTime.Add(DateTime.MinValue);
                    Debug.Log(plot.Data.Deadline + " " + freeTimePlot[c] + " " + plot.DurationDeadline + " " + harvestTime[c]);
                }

                Dictionary<int, DateTime> workerTmp = new Dictionary<int, DateTime>();
                workerTmp = this.OffGame_GetWorkerTmpSorted();
                Debug.Log("Worker Table");
                foreach (int key in workerTmp.Keys)
                {
                    Debug.Log(workerTmp[key]);
                }


                bool check = true;
                DateTime off = DateTime.MinValue;

                while (check)
                {
                    bool progressed = false;
                    for (int i = 0; i < workerTmp.Count; ++i)
                    {
                        DateTime now = workerTmp.ElementAt(i).Value;
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
                            check = false;
                        }
                        else
                        {
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


                            DateTime time = now.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                            int indexPlot = PlotController.Instance.GameOff_FindIndex_SkipHarvest(time, new List<PlotModel>(plotTmp), workerTmp.ElementAt(i).Key, workerTmp.Count - 1, new Dictionary<int, DateTime>(workerTmp),
                                            DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                            if (indexPlot >= 0 && time > DateTime.Parse(plotTmp[indexPlot].Data.Deadline))
                            {
                                // Harvest
                                ProductType tmpType = plotTmp[indexPlot].Data.ProductType; 
                                plotTmp[indexPlot].ResetData(false);
                                plotTmp[indexPlot].SetNullData(false);

                                plotTmp[indexPlot].Data.ProductType = tmpType;
                                freeTimePlot[indexPlot] = now.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                                var key = workerTmp.ElementAt(i).Key;
                                plotTmp[indexPlot].Data.WorkerId = i;
                                workerTmp[key] = now.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                                progressed = true;

                                off = now;
                                harvestTime[indexPlot] = now;
                                Debug.Log(plotTmp[indexPlot].Data.ProductType + " Harvest " + indexPlot);
                            }
                            else
                            {
                                // Produce
                                Tuple<int, bool> findPlot = FindPlotFree(now, new List<PlotModel>(plotTmp), freeTimePlot);
                                int idProduct = BagController.Instance.ChooseProduct();
                                if (findPlot.Item2 && idProduct > 0)
                                {
                                    int indexPr = DataManager.Instance.GetIdProductConfig(BagController.Instance.BagModelList[idProduct - 1].ProductType) - 1;
                                    int interval = DataManager.Instance.GameConfig.ProductConfigList[indexPr].Interval;
                                    int lifetime = DataManager.Instance.GameConfig.ProductConfigList[indexPr].Lifetime;
                                    int durationDeadline = DataManager.Instance.GameConfig.ProductConfigList[indexPr].Deadline;

                                    plotTmp[findPlot.Item1].Interval = interval;
                                    plotTmp[findPlot.Item1].Lifetime = lifetime;
                                    plotTmp[findPlot.Item1].DurationDeadline = durationDeadline;
                                    plotTmp[findPlot.Item1].Data.ProductType =
                                        BagController.Instance.BagModelList[idProduct - 1].ProductType;
                                    BagController.Instance.CaculateProductAmountByName(plotTmp[findPlot.Item1].Data.ProductType.ToString(), -1);

                                    int timeTask = DataManager.Instance.GameConfig.WorkerConfig.TimeTask;
                                    plotTmp[findPlot.Item1].Data.Deadline = now.AddSeconds(interval * lifetime + durationDeadline + timeTask).ToString();
                                    freeTimePlot[findPlot.Item1] = now.AddSeconds(interval * lifetime + timeTask);
                                   

                                    var key = workerTmp.ElementAt(i).Key;
                                    plotTmp[findPlot.Item1].Data.WorkerId = i;
                                    workerTmp[key] = now.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                                    progressed = true;

                                    off = now;
                                    Debug.Log("Produce " + findPlot.Item1 + " " + plotTmp[findPlot.Item1].Data.Deadline + " " + freeTimePlot[findPlot.Item1] + " " + plotTmp[findPlot.Item1].Data.ProductType);

                                }
                                else
                                {
                                    int idP = FindIndexPlotHaveDeadline(new List<PlotModel>(plotTmp));

                                    if (idP >= 0 && DateTime.Parse(plotTmp[idP].Data.Deadline) <= now.AddSeconds(plotTmp[idP].DurationDeadline))
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

                                        off = now;
                                        harvestTime[idP] = now;
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
                //PlotController.Instance.Init();
                if (off == DateTime.MinValue)
                {
                    off = DateTime.Parse(DataManager.Instance.GameData.Off);
                }
                Debug.Log(off.ToString());
                //PlotController.Instance.CheckWorking(off);
            }
            else
            {
                //PlotController.Instance.Init();
            }
        }

        private void FillToPlot(List<PlotModel> plotModelList, List<DateTime> harvestTime, List<DateTime> freeTimePlot)
        {
            DataManager.Instance.GameData.PlotList.Clear();
            List<WorkerDetail> tmpWorkerList = new List<WorkerDetail>(DataManager.Instance.GameData.WorkerList);
            this.InitGetData();
            int count = -1;
            foreach (PlotModel plotModel in plotModelList)
            {
                count++;
                WorkerDetail workerDetail = new WorkerDetail();
                PlotDetail plotDetail = new PlotDetail();
                if (plotModel.Data.Id.Length == 0)
                {
                    plotModel.Data.Id = count.ToString();
                }
                if (DateTime.Parse(plotModel.Data.Deadline) == DateTime.MinValue 
                    && freeTimePlot[int.Parse(plotModel.Data.Id)] <= DateTime.Now)
                {
                    plotDetail.Id = plotModel.Data.Id;
                    plotDetail.Status = PlotStatus.IsAvai;
                    plotDetail.CurTime = -1;
                    plotDetail.CurLife = -1;
                    DataManager.Instance.GameData.PlotList.Add(plotDetail);
                    Debug.Log(plotDetail.Id + " " + plotDetail.ProductType + " " + plotDetail.Status + " " +
                    plotDetail.CurTime + " " + plotDetail.CurLife + " " + plotDetail.CurAmount + " "
                    + plotDetail.Deadline + " " + plotDetail.WorkerId);
                    continue;
                }


                if ((DateTime.Parse(plotModel.Data.Deadline) == DateTime.MinValue
                    && freeTimePlot[int.Parse(plotModel.Data.Id)] > DateTime.Now
                    ) ||
                    (DateTime.Parse(plotModel.Data.Deadline) != DateTime.MinValue &&
                    plotModel.Data.WorkerId >= 0 &&
                    tmpWorkerList[plotModel.Data.WorkerId].State == WorkerState.Harvest &&
                    plotModel.Lifetime < 0 &&
                    plotModel.Interval < 0) 

                    )
                {
                    Debug.Log("Harvest");
                    if (plotModel.Data.WorkerId >= 0)
                    {
                        workerDetail.Id = plotModel.Data.WorkerId.ToString();
                        workerDetail.StartTime = harvestTime[int.Parse(plotModel.Data.Id)].ToString();
                        workerDetail.IdProduct = BagController.Instance.GetIndex(plotDetail.ProductType) + 1;
                        workerDetail.IdPlot = int.Parse(plotModel.Data.Id);
                        workerDetail.State = WorkerState.Harvest;
                        workerDetail.NotExcute = false;
                        DataManager.Instance.GameData.WorkerList[int.Parse(workerDetail.Id)] = workerDetail;
                        Debug.Log(workerDetail.Id + " " + workerDetail.IdPlot + " " + workerDetail.IdProduct + " " +
                            workerDetail.State + " " + workerDetail.StartTime + " " + workerDetail.NotExcute);
                        plotDetail.WorkerId = plotModel.Data.WorkerId;
                    }
                    else
                    {
                        plotDetail.Deadline = plotModel.Data.Deadline;
                    }

                    // Plot
                    plotDetail.ProductType = plotModel.Data.ProductType;
                    if (plotDetail.ProductType != ProductType.None)
                    {
                        Debug.Log(plotDetail.ProductType);
                        ProductConfig config = DataManager.Instance.GetProductConfig(plotDetail.ProductType);
                        plotDetail.Id = plotModel.Data.Id;
                        plotDetail.CurAmount = config.Lifetime - plotModel.Data.CurLife;
                        plotDetail.Status = PlotStatus.NotIsAvai;
                        plotDetail.CurLife = -1;
                        plotDetail.CurTime = -1;
                    }
                    
                }
                else
                {
                    DateTime deadline = DateTime.Parse(plotModel.Data.Deadline);
                    DateTime startTime = deadline;
                    startTime = startTime.AddSeconds(-plotModel.DurationDeadline);
                    startTime = startTime.AddSeconds(-plotModel.Interval * plotModel.Lifetime);
                    startTime = startTime.AddSeconds(-DataManager.Instance.GameConfig.WorkerConfig.TimeTask);

                    DateTime doneProduce = startTime.AddSeconds(DataManager.Instance.GameConfig.WorkerConfig.TimeTask);
                    DateTime doneDevProduct = doneProduce.AddSeconds(plotModel.Interval * plotModel.Lifetime + plotModel.DurationDeadline);
                    if (DateTime.Now < doneProduce)
                    {
                        Debug.Log("Produce");
                        //Worker
                        if (plotModel.Data.WorkerId >= 0)
                        {
                            workerDetail.Id = plotModel.Data.WorkerId.ToString();
                            workerDetail.StartTime = startTime.ToString();
                            // model.Id
                            workerDetail.IdProduct = BagController.Instance.GetIndex(plotModel.Data.ProductType) + 1;
                            BagController.Instance.CaculateProductAmountByName(plotModel.Data.ProductType.ToString(), 1);
                            workerDetail.IdPlot = int.Parse(plotModel.Data.Id);
                            workerDetail.State = WorkerState.Produce;
                            workerDetail.NotExcute = false;
                            DataManager.Instance.GameData.WorkerList[int.Parse(workerDetail.Id)] = workerDetail;
                            Debug.Log(workerDetail.Id + " " + workerDetail.IdPlot + " " + workerDetail.IdProduct + " " +
                                workerDetail.State + " " + workerDetail.StartTime + " " + workerDetail.NotExcute);
                        }


                        // Plot
                        PlotModel plotTmp = new PlotModel();
                        plotTmp.Data = plotModel.Data;

                        plotTmp.ResetData(false);
                        plotTmp.SetNullData(false);
                        plotDetail = plotTmp.Data;
                        plotDetail.Id = plotModel.Data.Id;
                        plotDetail.WorkerId = plotModel.Data.WorkerId;


                    }
                    else if (DateTime.Now < doneDevProduct)
                    {
                        Debug.Log("Dev");
                        // Plot
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
                
                Debug.Log(plotDetail.Id + " " + plotDetail.ProductType + " " + plotDetail.Status + " " +
                    plotDetail.CurTime + " " + plotDetail.CurLife + " " + plotDetail.CurAmount + " " 
                    + plotDetail.Deadline + " " + plotDetail.WorkerId + " " 
                    + freeTimePlot[int.Parse(plotModel.Data.Id)]);
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
