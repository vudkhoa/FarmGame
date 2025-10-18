using Utils.DesignPattern.Singleton;
using Data.Game;
using UnityEngine;
using System.Collections.Generic;
using Game.Boostrap;
using System.IO;
using System;
using Sell.Model;
using Sell.Controller;
using Bag.Controller;
using Plots.Model;
using Plots.Controller;
using Data.Product;

namespace Data.Manager
{
    public class DataManager : SingletonMono<DataManager>
    {
        [Header(" Setting DatamManager ")]
        [SerializeField] public ProductSO ProductConfigData;
        [SerializeField] public ResourcesInitSO ResourcesInitData;
        [SerializeField] public PlotSO PlotConfigData;
        [SerializeField] public EquipmentSO EquipmentConfigData;
        [SerializeField] public WorkerSO WorkerConfigData;

        [Header(" Running Game ")]
        public GameData GameData;
        public string Json;

        public void Init()
        {
            GameData = new GameData();
            SetupProductConfigData();
        }

        private ProductTypeConf GetProductTypeConfig(int index)
        {
            return this.ProductConfigData.ListProductType[index];
        }

        private void SetupProductConfigData()
        {
            foreach (ProductTypeConf config in this.ProductConfigData.ListProductType)
            {
                Debug.Log(config.Id + " " + config.Name);
            }

            foreach (ProductConf config in this.ProductConfigData.ListProductConf)
            {
                ProductTypeConf type = this.GetProductTypeConfig(config.ProductType.Id - 1);
                config.ProductType.Name = type.Name;
                Debug.Log(  config.Id + " " + 
                            config.ProductType.Id + " " + config.ProductType.Name + " " +
                            config.Interval + " " +
                            config.Price + " " +
                            config.Cost);
            }
        }

        public void tmpCheck()
        {
            foreach (ResourceProduct rsInit in this.ResourcesInitData.ListResourceInit)
            {
                Debug.Log(rsInit.ProductType.Id + " " + rsInit.ProductType.Name);
            }
        }

        public void CreateData()
        {
            if (File.Exists(GameBootstrap.Instance.FilePath))
            {
                Json = File.ReadAllText(GameBootstrap.Instance.FilePath);
                this.GameData = JsonUtility.FromJson<GameData>(Json);
            }
            else
            {
                this.GameData.Off = null;
                List<ItemDetail> BagData = new List<ItemDetail>();
                List<ItemDetail> SellData = new List<ItemDetail>();
                foreach (ProductConf bagItem in this.ProductConfigData.ListProductConf)
                {
                    ItemDetail itemDetail = new ItemDetail();
                    int amount = 0;
                    foreach (ResourceProduct resourceProduct in this.ResourcesInitData.ListResourceInit)
                    {
                        if (bagItem.ProductType.Id == resourceProduct.ProductType.Id)
                        {
                            amount = resourceProduct.Amount;
                        }
                    }

                    itemDetail.SetItemDetail(bagItem.Id, bagItem.ProductType, amount);

                    BagData.Add(itemDetail);

                    ItemDetail sellItem = new ItemDetail
                    {
                        Id = bagItem.Id,
                        ProductType = bagItem.ProductType,
                        Amount = 0
                    };
                    SellData.Add(sellItem);

                }
                GameData.BagItemList = BagData;
                GameData.SellItemList = SellData;


                int plotCount = -1;
                int workerCount = -1;
                // Plot
                foreach (ResourceByName rsName in this.ResourcesInitData.ListResourceByName)
                {
                    if (string.Equals(rsName.Name.ToString(), "Plot", StringComparison.OrdinalIgnoreCase))
                    {
                        plotCount = rsName.Amount;
                    }
                    else 
                    if (string.Equals(rsName.Name.ToString(), "Worker", StringComparison.OrdinalIgnoreCase))
                    {
                        workerCount = rsName.Amount;
                    }
                }

                List<PlotDetail> plotData = new List<PlotDetail>();
                for (int i = 0; i < plotCount; i++)
                {
                    PlotDetail plot = new PlotDetail();
                    plot.Id = i;
                    plot.Status = PlotStatus.IsAvai;
                    plot.CurTime = -1;
                    plot.CurLife = -1;
                    plot.ProductType = ProductConfigData.ListProductType[0];
                    plotData.Add(plot);
                }
                GameData.PlotList = plotData;

                // Worker
                List<WorkerDetail> workerData = new List<WorkerDetail>();
                for (int i = 0; i < workerCount; i++)
                {
                    WorkerDetail worker = new WorkerDetail();
                    worker.Id = i.ToString();
                    worker.State = Worker.Controller.WorkerState.Idle;
                    workerData.Add(worker);
                }
                GameData.WorkerList = workerData;

                // Player
                this.GameData.Player = new PlayerDetail();

                // Equipment
                this.GameData.Equipment = new EquipmentDetail();
            }
        }

        //public void ConvertData_ConfigToGame(ConfigDatabase DB)
        //{
        //    //List<ProductConfig> ProductDataList = new List<ProductConfig>();
        //    //foreach (ProductConfig product in DB.ProductList.Values)
        //    //{
        //    //    ProductDataList.Add(product);
        //    //}
        //    //this.GameConfig.ProductConfigList = ProductDataList;

        //    //WorkerConfig workerConfig = new WorkerConfig();
        //    //workerConfig.Name = DB.WorkerConfig.Name;
        //    //workerConfig.Cost = DB.WorkerConfig.Cost;
        //    //workerConfig.TimeTask = DB.WorkerConfig.TimeTask;
        //    //workerConfig.PackSize = DB.WorkerConfig.PackSize;
        //    //this.GameConfig.WorkerConfig = workerConfig;
        //    //this.GameConfig.EquipmentConfig = DB.EquipmentConfig;
        //    //this.GameConfig.PlotConfig = DB.PlotConfig;

        //    // Read Init Resource or null
        //    //if (File.Exists(GameBootstrap.Instance.FilePath))
        //    //{
        //    //    Json = File.ReadAllText(GameBootstrap.Instance.FilePath);
        //    //    this.GameData = JsonUtility.FromJson<GameData>(Json);
        //    //}
        //    //else
        //    //{
        //    //    this.GameData.Off = null;
        //    //    List<ItemDetail> BagData = new List<ItemDetail>();
        //    //    List<ItemDetail> SellData = new List<ItemDetail>();
        //    //    foreach (Re bagItem in DB.BagInitList.Values)
        //    //    {
        //    //        BagData.Add(bagItem);

        //    //        ItemDetail sellItem = new ItemDetail
        //    //        {
        //    //            Id = bagItem.Id,
        //    //            ProductType = bagItem.ProductType,
        //    //            Amount = 0
        //    //        };
        //    //        SellData.Add(sellItem);

        //    //    }
        //    //    GameData.BagItemList = BagData;
        //    //    GameData.SellItemList = SellData;
                
        //    //    // Plot
        //    //    List<PlotDetail> plotData = new List<PlotDetail>();
        //    //    foreach (var plot in DB.PlotsInitList)
        //    //    {
        //    //        plotData.Add(plot.Value);
        //    //    }
        //    //    GameData.PlotList = plotData;
            
        //    //    // Worker
        //    //    List<WorkerDetail> workerData = new List<WorkerDetail>();
        //    //    foreach (var worker in DB.WorkerInitList)
        //    //    {
        //    //        workerData.Add(worker.Value);
        //    //    }
        //    //    GameData.WorkerList = workerData;

        //    //    // Player
        //    //    this.GameData.Player = new PlayerDetail();

        //    //    // Equipment
        //    //    this.GameData.Equipment = new EquipmentDetail();
        //    //}
        //}

        public void SetDateTimeOff()
        {
            DateTime off = DateTime.Now;
            string offStr = off.ToString();
            this.GameData.Off = offStr;
        }

        public string ConverData_GameToJson()
        {
            string json = JsonUtility.ToJson(GameData, true);
            return json;
        }
    
        public int GetIdProductConfig(ProductTypeConf type)
        {
            foreach (ProductConf config in this.ProductConfigData.ListProductConf)
            {
                if (config.ProductType.Id == type.Id)
                {
                    return config.Id;
                }
            }
            return -1;
        }

        public ProductConf GetProductConfig(ProductTypeConf type)
        {
            foreach (ProductConf config in this.ProductConfigData.ListProductConf)
            {
                if (config.ProductType.Id == type.Id)
                {
                    return config;
                }
            }
            return null;
        }

        public void FillIntoSell()
        {
            GameData gameDataTmp = JsonUtility.FromJson<GameData>(this.Json);
            List<int> curPlotList = new List<int>();
            List<int> startPlotList = new List<int>();
            int index = -1;
            if (gameDataTmp == null) { return; }
            foreach (ItemDetail bagItem in gameDataTmp.BagItemList)
            {
                index++;
                int count = 0;
                curPlotList.Add(0);
                foreach(PlotModel plot in PlotController.Instance.PlotModelList)
                {
                    if (string.Equals(plot.Data.ProductType.Name.ToString(), bagItem.ProductType.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                    }
                }
                curPlotList[index] = count;

                count = 0;
                startPlotList.Add(0);
                foreach (PlotDetail plot in gameDataTmp.PlotList)
                {
                    if (string.Equals(plot.ProductType.Name.ToString(), bagItem.ProductType.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                    }
                }
                startPlotList[index] = count;
            }
            index = -1;
            foreach (SellModel sellModel in SellController.Instance.SellModelList)
            {
                index++;
                if (index >= this.ProductConfigData.ListProductConf.Count) { return; }
                int startBag = gameDataTmp.BagItemList[index].Amount + startPlotList[index];
                int startSell = gameDataTmp.SellItemList[index].Amount;
                int curPlot = curPlotList[index];
                int curBag = BagController.Instance.BagModelList[index].ProductAmount;
                int offsetSell = startBag - curBag - curPlot;
                Debug.Log(startBag + " " + startSell + " " + curPlot + " " + curBag + " " + offsetSell);
                if (offsetSell != 0)
                {
                    SellController.Instance.SellModelList[index].ProductAmount = (startSell + offsetSell * this.ProductConfigData.ListProductConf[index].Lifetime);
                    SellController.Instance.SellModelList[index].View.SetAmount(SellController.Instance.SellModelList[index].ProductAmount);
                }
            }
        }
    }
}