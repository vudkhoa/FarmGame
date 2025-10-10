using Data.Config;
using Data.Game;
using Data.Manager;
using Game.Manager;
using Plots.Model;
using Plots.View;
using Product.Controller;
using Sell.Controller;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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

        private void Start()
        {
            this.Init();
        }

        public void Init() 
        {
            Debug.Log("Init");
            this.IsActing = false;
            this.PlotModelList = new List<PlotModel>();
            this.GetData();
            this.CheckWorking();
        }

        private void GetData()
        {
            int count = -1;
            foreach (PlotDetail plot in DataManager.Instance.GameData.PlotList)
            {
                count++;
                PlotModel model = new PlotModel();
                PlotView view = Instantiate(PlotPrefab, ParentAllView);
                model.Init(plot, view, count);

                if (plot.Type != ProductType.None)
                {
                    int interval = 0;
                    int lifetime = 0;
                    int durationDeadline = 0;

                    List<ProductConfig> productConfigs = new List<ProductConfig>();
                    productConfigs = DataManager.Instance.GameConfig.ProductConfigList;
                    foreach (ProductConfig product in productConfigs)
                    {
                        if (product.Name.ToString() == plot.Type.ToString())
                        {
                            interval = product.Interval;
                            lifetime = product.Lifetime;
                            durationDeadline = product.Deadline;
                            break;
                        }
                    }
                       
                    model.Setup(plot.Type, interval, lifetime); 

                    if (plot.CurLife < 0 && plot.CurTime < 0)
                    {
                        model.SetCurLife(0);
                        model.SetCurTime(0f, 0);
                        model.ResetData();
                    }
                }

                PlotModelList.Add(model);
            }
            this.IsActing = true;
        }

        private void CheckWorking()
        {
            if (DateTime.TryParse(DataManager.Instance.GameData.Off, out DateTime result))
            {
                double timeDistance = GameManager.Instance.GetDistanceWithNow(result);
                Debug.Log(timeDistance);
                foreach (PlotModel model in this.PlotModelList)
                {
                    if (model.Data.Status != PlotStatus.NotIsAvai) { continue; }

                    if (model.Data.CurLife < 0 && model.Data.CurTime < 0) { continue; }

                    int countProduct = (int)(timeDistance / model.Interval);
                    int remaining = model.Lifetime - model.Data.CurLife;
                    Debug.Log(remaining);
                    if (countProduct >= remaining)
                    {
                        DateTime timeForModel = DateTime.Parse(DataManager.Instance.GameData.Off);
                        timeForModel.AddSeconds(remaining * model.Interval);
                        model.Data.Deadline = timeForModel.ToString();

                        model.SetCurLife(0);
                        model.SetCurTime(-1, remaining);
                        model.ResetData();
                        model.CaculateDeadline();
                    }
                    else if (countProduct < remaining)
                    {
                        float number = (float)(timeDistance - countProduct * model.Interval);
                        number += model.Data.CurTime;

                        if (number > model.Interval)
                        {
                            model.Data.CurTime = 0;
                            number = number - model.Interval;
                            countProduct += 1;
                        }

                        int integerPart = (int)number;
                        float fractionPart = number - integerPart;

                        Debug.Log(number + " " + integerPart + " " + fractionPart + " " + countProduct + " " + (model.Data.CurTime + integerPart));
                        model.SetCurLife(countProduct);
                        model.SetCurTime(model.Data.CurTime + integerPart, countProduct);
                        if (model.Data.CurAmount == model.Lifetime) 
                        {
                            DateTime timeForModel = DateTime.Parse(DataManager.Instance.GameData.Off);
                            timeForModel.AddSeconds(remaining * model.Interval); 
                            model.ResetData();
                            model.CaculateDeadline();
                            continue; 
                        }

                        model.Countdown(1 - fractionPart);
                    }
                }
            }
        }

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

                if (model.Data.Status == PlotStatus.IsAvai && !WorkerController.Instance.CheckExistIdPlot(int.Parse(model.Data.Id)))
                {
                    return count;
                }
            }
            if (!isClick || coutNotAvai == this.PlotModelList.Count)
            {
                return -1;
            }
            count = WorkerController.Instance.FindId_MaxStartTime();
            WorkerController.Instance.WokerList[count].NotExecute = true;

            return WorkerController.Instance.WokerList[count].IdPlot;
        }
    
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
    
        public void IncreaseTime(float time, int id)
        {
            float timeModel = this.PlotModelList[id].Data.CurTime + time;
            if (timeModel < this.PlotModelList[id].Interval)
            {
                this.PlotModelList[id].SetCurTime(timeModel, 0);
            }
            else
            {
                if (this.PlotModelList[id].Data.CurLife + 1 <
                    this.PlotModelList[id].Lifetime)
                {
                    this.PlotModelList[id].SetCurLife(1);
                    this.PlotModelList[id].SetCurTime(0f, 1);
                }
                else
                {
                    this.PlotModelList[id].CaculateDeadline();
                    this.PlotModelList[id].SetCurLife((-1) * (this.PlotModelList[id].Data.CurLife + 1));
                    this.PlotModelList[id].SetCurTime(0f, 1);
                    this.PlotModelList[id].ResetData();
                }
            }
        }

        public void MoveToSell(int id)
        {
            if (this.PlotModelList[id].Data.CurAmount <= 0)
            {
                return;
            }
            int index = SellController.Instance.FindByProductType(this.PlotModelList[id].Data.Type);
            if (index == -1) { return; }

            //Debug.Log("Click Harvest Done");
            SellController.Instance.IncreaseByIndex(index, this.PlotModelList[id].Data.CurAmount);
            this.PlotModelList[id].ReduceAmount(this.PlotModelList[id].Data.CurAmount);
            if (WorkerController.Instance.FindIdWorkerByIdPlot(id) < 0) { return; }
            WorkerController.Instance.WokerList[WorkerController.Instance.FindIdWorkerByIdPlot(id)].NotExecute = true;
            //WorkerController.Instance.WokerList[WorkerController.Instance.FindIdWorkerByIdPlot(id)].StopWork();
        }

        public void SaveData()
        {
            DataManager.Instance.GameData.PlotList.Clear();

            foreach (PlotModel model in this.PlotModelList)
            {
                PlotDetail plot = new PlotDetail();
                plot.Id = model.Data.Id;
                plot.Type = model.Data.Type;
                plot.Status = model.Data.Status;
                plot.CurTime = model.Data.CurTime;
                plot.CurLife = model.Data.CurLife;
                plot.CurAmount = model.Data.CurAmount;
                plot.Deadline = model.Data.Deadline;
                plot.WorkerId = model.Data.WorkerId;
                DataManager.Instance.GameData.PlotList.Add(plot);
            }
        }
    
        public bool CheckModel(PlotModel model)
        {
            if (model.Data.CurLife > 0 && model.Data.CurTime > 0) { return true; }
            return false;
        }

        public Tuple<int, float> CheckMinDeadline()
        {
            int i = -1;
            int index = -1;
            float minDeadline = float.MaxValue;
            if (this.PlotModelList == null) return new Tuple<int, float>(index, minDeadline);
            foreach (PlotModel model in this.PlotModelList)
            {
                i++;
                if (!CheckHaveDeadline(model) || model.Data.WorkerId != -1) { continue; }
                //Debug.Log(model.Data.CurTime + " " + model.Data.CurLife);
                float deadlineDistance = (float)(DateTime.Parse(model.Data.Deadline) - DateTime.Now).TotalSeconds;
                if (deadlineDistance < minDeadline)
                {
                    minDeadline = deadlineDistance;
                    index = i;
                }
            }
            return new Tuple<int, float>(index, minDeadline);
        }

        public void SetNullById(int id)
        {
            Debug.Log(id);
            this.PlotModelList[id].ResetData();
            this.PlotModelList[id].SetNullData();
        }

        private bool CheckHaveDeadline(PlotModel model)
        {
            if (CheckModel(model)) 
            { 
                //Debug.Log(CheckModel(model));
                return false; 
            }
            if (DateTime.Parse(model.Data.Deadline) == DateTime.MinValue) { return false; }
            return true;

        }

        public bool CheckDeadlineSkipTime(int idWorker, int maxIdWorker, Dictionary<int, DateTime> TimerForWorkerList, int timeTask)
        {
            TimerForWorkerList[idWorker] = DateTime.Now;
            List<PlotModel> tmpPlotModelList = new List<PlotModel>(this.PlotModelList);
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
                if (!this.CheckHaveDeadline(tmpPlotModelList[idTask])) { return true; }
                if (TimerForWorkerList[idWorker] == DateTime.MinValue)
                {
                    TimerForWorkerList[idWorker] = DateTime.Now;
                    continue;
                }

                DateTime doneTask = TimerForWorkerList[idWorker].AddSeconds(timeTask);
                if (DateTime.Parse(tmpPlotModelList[idTask].Data.Deadline) < doneTask)
                {
                    return false;
                }
            }
            return true;
        }
    }
}