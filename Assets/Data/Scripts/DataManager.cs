using Utils.DesignPattern.Singleton;
using Data.Config;
using Data.Game;
using UnityEngine;
using System.Collections.Generic;
using Game.Boostrap;
using System.IO;
using System;

namespace Data.Manager
{
    public class DataManager : SingletonMono<DataManager>
    {
        [Header(" Running Game ")]
        public GameData GameData;
        public GameConfig GameConfig; 

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

            // Read Init Resource or null
            if (File.Exists(GameBoostrap.Instance.FilePath))
            {
                string json = File.ReadAllText(GameBoostrap.Instance.FilePath);
                this.GameData = JsonUtility.FromJson<GameData>(json);
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
    }
}