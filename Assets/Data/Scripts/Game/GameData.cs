using Data.Manager;
using Data.Product;
using Plots.Controller;
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
        public PlayerDetail Player;
        public EquipmentDetail Equipment;
    }

    [Serializable]
    public class ItemDetail
    {
        public int Id;
        public ProductTypeConf ProductType;
        public int Amount;

        public void SetItemDetail(int id, ProductTypeConf type, int amount)
        {
            this.Id = id;
            this.ProductType = type;
            this.Amount = amount;
        }
    }

    [Serializable]
    public class PlotDetail
    {
        public int Id;
        public ProductTypeConf ProductType;
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
            this.Status = PlotStatus.IsAvai;
            this.Deadline = DateTime.MinValue.ToString();
            //this.ProductType = DataManager.Instance.ProductConfigData.ListProductType[0];
        }
    }

    [Serializable]
    public class WorkerDetail
    {
        public string Id;
        public WorkerState State;
        public int IdPlot;
        public int IdProduct;
        public string StartTime;
        public bool NotExcute;

        public WorkerDetail()
        {
            this.IdPlot = -1;
            this.IdProduct = -1;
            this.StartTime = DateTime.MinValue.ToString();
            this.State = WorkerState.Idle;
            this.NotExcute = false;
        }
    }

    [Serializable]
    public class PlayerDetail
    {
        public int Gold;
        public PlayerDetail()
        {
            this.Gold = 0;
        }
    }

    [Serializable]
    public class ShopItemDetail
    {
        public int Id;
        public string NameProduct;
        public int Price;
        public int PackSize;
    }

    [Serializable]
    public class EquipmentDetail
    {
        public int CurrentLevel;

        public EquipmentDetail() 
        {
            this.CurrentLevel = 1;
        }
    }
}