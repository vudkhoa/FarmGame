using Plots.Controller;
using Product.Controller;
using System;
using System.Collections.Generic;
using Worker.Controller;

namespace Data.Game
{
    [Serializable]
    public class GameData
    {
        public string Off;
        public List<ItemDetail> BagItemList;
        public List<ItemDetail> SellItemList;
        public List<PlotDetail> PlotList;
        public List<WorkerDetail> WorkerList;
    }

    [Serializable] 
    public class ItemDetail
    {
        public string Id;
        public string Name;
        public ProductType ProductType;
        public int Amount;
    }

    [Serializable]
    public class PlotDetail
    {
        public string Id;
        public ProductType Type;
        public PlotStatus Status;
        public float CurTime;
        public int CurLife;
        public int CurAmount;
        public string Deadline;
        public int WorkerId;

        public PlotDetail()
        {
            this.CurTime = -1;
            this.CurLife = -1;
            this.WorkerId = -1;
            this.Deadline = DateTime.MinValue.ToString();
        }
    }

    [Serializable]
    public class WorkerDetail
    {
        public string Id;
        public WorkerState State;
        public int PlotId;
        public int CurTime;
    }
}