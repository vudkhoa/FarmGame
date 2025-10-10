using Bag.Controller;
using Data.Game;
using Data.Manager;
using JetBrains.Annotations;
using Plots.Controller;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.DesignPattern.Singleton;
using Worker.Model;

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
        public float offset;

        [Header(" Running Game ")]
        public List<WorkerModel> WokerList;

        private void Start()
        {
            this.Init();
        }

        public void Init()
        {
            this.GetData();
            this.DebugModel();
        }

        private void FixedUpdate()
        {
            this.CheckWorking();
        }

        private void GetData()
        {
            this.WokerList = new List<WorkerModel>();
            foreach (WorkerDetail worker in DataManager.Instance.GameData.WorkerList)
            {
                WorkerModel model = new WorkerModel();
                model.Id = int.Parse(worker.Id);
                model.State = WorkerState.Idle;//worker.State;
                model.StateMachine.Change(model, new Idle());
                model.TimeTask = DataManager.Instance.GameConfig.WorkerConfig.TimeTask;
                this.WokerList.Add(model);
            }
        }
    
        private void DebugModel()
        {
            foreach(WorkerModel model in this.WokerList)
            {
                Debug.Log(model.Id + " " + model.State);
            }
        }
    
        public bool CheckExist(int id, int idPlot)
        {
            foreach (WorkerModel model in this.WokerList)
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
            foreach (WorkerModel model in this.WokerList)
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
            foreach (WorkerModel model in this.WokerList)
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
            if (this.WokerList == null) { return -1; }
            foreach(WorkerModel model in this.WokerList)
            {
                if (model.IdPlot == id)
                {
                    return model.Id;
                }
            }
            return -1;
        }

        public void CheckWorking()
        {
            foreach (WorkerModel model in this.WokerList)
            {
                if (model.State != WorkerState.Idle) { continue; }
                Tuple<int, float> minDeadline = PlotController.Instance.CheckMinDeadline();
                if (minDeadline.Item1 >= 0)
                {
                    if (minDeadline.Item2 <= DataManager.Instance.GameConfig.WorkerConfig.TimeTask + offset &&
                    !this.CheckExist(model.Id, minDeadline.Item1))
                    {
                        PlotController.Instance.PlotModelList[minDeadline.Item1].Data.WorkerId = model.Id;
                        model.IdPlot = minDeadline.Item1;
                        model.StateMachine.Change(model, new Harvest());
                        model.StateMachine.Execute(model);
                    }
                    else
                    {
                        Dictionary<int, DateTime> TimerForWorkerList = new Dictionary<int, DateTime>();
                        foreach (WorkerModel m in this.WokerList)
                        {
                            TimerForWorkerList[m.Id] = m.StartTime;
                        }

                        if (PlotController.Instance.CheckDeadlineSkipTime(model.Id, this.WokerList.Count - 1, TimerForWorkerList, model.TimeTask))
                        {
                            int plotIndex = PlotController.Instance.FindPlotAvai();
                            int productIndex = BagController.Instance.ChooseProduct();

                            if (plotIndex < 0 || productIndex < 0) 
                            {
                                PlotController.Instance.PlotModelList[minDeadline.Item1].Data.WorkerId = model.Id;
                                model.IdPlot = minDeadline.Item1;
                                model.StateMachine.Change(model, new Harvest());
                                model.StateMachine.Execute(model);
                                continue; 
                            }

                            model.IdPlot = plotIndex;
                            model.IdProduct = productIndex;
                            model.StateMachine.Change(model, new Produce());
                            model.StateMachine.Execute(model);
                        }
                        else
                        {
                            PlotController.Instance.PlotModelList[minDeadline.Item1].Data.WorkerId = model.Id;
                            model.IdPlot = minDeadline.Item1;
                            model.StateMachine.Change(model, new Harvest());
                            model.StateMachine.Execute(model);
                        }
                    }
                }
                else
                {
                    int plotIndex = PlotController.Instance.FindPlotAvai();
                    int productIndex = BagController.Instance.ChooseProduct();
                    //Debug.Log(plotIndex + " " + productIndex);
                    if (plotIndex < 0 || productIndex < 0) { continue; }

                    model.IdPlot = plotIndex;
                    model.IdProduct = productIndex;
                    model.StateMachine.Change(model, new Produce());
                    model.StateMachine.Execute(model);
                }
            }
        }
    }
}
