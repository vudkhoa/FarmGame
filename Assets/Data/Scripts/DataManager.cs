using Utils.DesignPattern.Singleton;
using Data.Config;
using Data.Game;
using UnityEngine;
using System.Collections.Generic;
using Game.Boostrap;
using System.IO;
using System;
using Product.Controller;
using Sell.Model;
using Sell.Controller;
using Bag.Controller;
using Plots.Model;
using Plots.Controller;

namespace Data.Manager
{
    public class DataManager : SingletonMono<DataManager>
    {
        [Header(" Running Game ")]
        public GameData GameData;
        public GameConfig GameConfig;
        public string Json;

        public void Init()
        {
            GameData = new GameData();
            GameConfig = new GameConfig();
        }

        public void ConvertData_ConfigToGame(ConfigDatabase DB)
        {
            List<ProductConfig> ProductDataList = new List<ProductConfig>();
            foreach (ProductConfig product in DB.ProductList.Values)
            {
                ProductDataList.Add(product);
            }
            this.GameConfig.ProductConfigList = ProductDataList;

            WorkerConfig workerConfig = new WorkerConfig();
            workerConfig.Name = DB.WorkerConfig.Name;
            workerConfig.Cost = DB.WorkerConfig.Cost;
            workerConfig.TimeTask = DB.WorkerConfig.TimeTask;
            workerConfig.PackSize = DB.WorkerConfig.PackSize;
            this.GameConfig.WorkerConfig = workerConfig;
            this.GameConfig.EquipmentConfig = DB.EquipmentConfig;
            this.GameConfig.PlotConfig = DB.PlotConfig;

            // Read Init Resource or null
            if (File.Exists(GameBoostrap.Instance.FilePath))
            {
                Json = File.ReadAllText(GameBoostrap.Instance.FilePath);
                this.GameData = JsonUtility.FromJson<GameData>(Json);
            }
            else
            {
                this.GameData.Off = null;
                List<ItemDetail> BagData = new List<ItemDetail>();
                List<ItemDetail> SellData = new List<ItemDetail>();
                foreach (ItemDetail bagItem in DB.BagInitList.Values)
                {
                    BagData.Add(bagItem);

                    ItemDetail sellItem = new ItemDetail
                    {
                        Id = bagItem.Id,
                        Name = bagItem.Name,
                        ProductType = bagItem.ProductType
                    };

                    SellData.Add(sellItem);

                }
                GameData.BagItemList = BagData;
                GameData.SellItemList = SellData;
                
                // Plot
                List<PlotDetail> plotData = new List<PlotDetail>();
                foreach (var plot in DB.PlotsInitList)
                {
                    plotData.Add(plot.Value);
                }
                GameData.PlotList = plotData;
            
                // Worker
                List<WorkerDetail> workerData = new List<WorkerDetail>();
                foreach (var worker in DB.WorkerInitList)
                {
                    workerData.Add(worker.Value);
                }
                GameData.WorkerList = workerData;

                // Player
                this.GameData.Player = new PlayerDetail();

                // Equipment
                this.GameData.Equipment = new EquipmentDetail();
            }
        }

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
    
        public int GetIdProductConfig(ProductType type)
        {
            foreach (ProductConfig config in this.GameConfig.ProductConfigList)
            {
                if (config.Name.ToString() == type.ToString())
                {
                    return int.Parse(config.Id);
                }
            }
            return -1;
        }

        public ProductConfig GetProductConfig(ProductType type)
        {
            foreach (ProductConfig config in this.GameConfig.ProductConfigList)
            {
                if (config.Name.ToString() == type.ToString())
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
                    if (string.Equals(plot.Data.ProductType.ToString(), bagItem.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                    }
                }
                curPlotList[index] = count;

                count = 0;
                startPlotList.Add(0);
                foreach (PlotDetail plot in gameDataTmp.PlotList)
                {
                    if (string.Equals(plot.ProductType.ToString(), bagItem.Name.ToString(), StringComparison.OrdinalIgnoreCase))
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
                if (index >= this.GameConfig.ProductConfigList.Count) { return; }
                int startBag = gameDataTmp.BagItemList[index].Amount + startPlotList[index];
                int startSell = gameDataTmp.SellItemList[index].Amount;
                int curPlot = curPlotList[index];
                int curBag = BagController.Instance.BagModelList[index].ProductAmount;
                int offsetSell = startBag - curBag - curPlot;
                Debug.Log(startBag + " " + startSell + " " + curPlot + " " + curBag + " " + offsetSell);
                if (offsetSell != 0)
                {
                    SellController.Instance.SellModelList[index].ProductAmount = (startSell + offsetSell * this.GameConfig.ProductConfigList[index].Lifetime);
                    SellController.Instance.SellModelList[index].View.SetAmount(SellController.Instance.SellModelList[index].ProductAmount);
                }
            }
        }
    }
}