using System.Collections.Generic;
using System.IO;
using Data.Config;
using Data.Game;
using System;
using Product.Controller;
using Plots.Controller;
using UnityEngine;

namespace Utils.CsvTool
{
    public static class CsvConfigLoader
    {
        public static ConfigDatabase db;

        public static ConfigDatabase LoadFromFolder(string folderPath)
        {
            db = new ConfigDatabase();

            LoadProductConfig();
            LoadResourcesInit();
            LoadWorkerConfig();
            LoadEquipmentConfig();
            LoadPlotConfig();
            return db;
        }

        public static void LoadProductConfig()
        {
            TextAsset csvData = Resources.Load<TextAsset>("products");
            if (csvData == null)
            {
                Debug.LogError("No Found products.csv!");
                return;
            }

            StringReader reader = new StringReader(csvData.text);

            string headerLine = reader.ReadLine();
            string[] headers = headerLine.Split(',');

            List<ProductConfig> productConfigList = new List<ProductConfig>();


            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (values.Length != headers.Length) continue;

                Dictionary<string, string> r = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    r[headers[i]] = values[i];
                }

                ProductConfig product = new ProductConfig
                {
                    Id = r["id"],
                    Name = r["name"],
                    Interval = int.Parse(r["interval"]),
                    Lifetime = int.Parse(r["lifetime"]),
                    Cost = int.Parse(r["cost"]),
                    Price = int.Parse(r["price"]),
                    PackSize = int.Parse(r["pack_size"]),
                    Deadline = int.Parse(r["deadline"])
                };
                productConfigList.Add(product);
                db.ProductList[product.Id] = product;
            }
        }

        public static void LoadResourcesInit()
        { 
            TextAsset csvData = Resources.Load<TextAsset>("resources_init");
            if (csvData == null)
            {
                Debug.LogError("No Found resources_init.csv!");
                return;
            }

            StringReader reader = new StringReader(csvData.text);

            string headerLine = reader.ReadLine();
            string[] headers = headerLine.Split(',');

            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (values.Length != headers.Length) continue;

                Dictionary<string, string> entry = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    entry[headers[i]] = values[i];
                }

                string nameKey = entry["name"];
                if (Enum.TryParse(nameKey.ToString(), out ProductType productType))
                {
                    ItemDetail bagInit = new ItemDetail
                    {
                        Id = entry["id"],
                        Name = entry["name"],
                        ProductType = productType,
                        Amount = int.Parse(entry["amount"])
                    };
                    db.BagInitList[bagInit.Id] = bagInit;
                }
                else if (entry["name"].ToString() == "Plots")
                {
                    for (int i = 0; i < int.Parse(entry["amount"]); i++)
                    {
                        PlotDetail plot = new PlotDetail();
                        plot.Id = i.ToString();
                        plot.Status = PlotStatus.IsAvai;
                        plot.CurTime = -1;
                        plot.CurLife = -1;
                        db.PlotsInitList[plot.Id] = plot;
                    }
                }
                else
                {
                    for (int i = 0; i < int.Parse(entry["amount"]); i++)
                    {
                        WorkerDetail worker = new WorkerDetail();
                        worker.Id = i.ToString();
                        worker.State = Worker.Controller.WorkerState.Idle;
                        db.WorkerInitList[worker.Id] = worker;
                    }
                }
            }
        }
    
        public static void LoadWorkerConfig()
        {
            TextAsset csvData = Resources.Load<TextAsset>("worker");
            if (csvData == null)
            {
                Debug.LogError("No Found worker.csv!");
                return;
            }

            StringReader reader = new StringReader(csvData.text);

            string headerLine = reader.ReadLine();
            string[] headers = headerLine.Split(',');

            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (values.Length != headers.Length) continue;

                Dictionary<string, string> r = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    r[headers[i]] = values[i];
                }

                WorkerConfig worker = new WorkerConfig
                {
                    Name = r["name"],
                    TimeTask = int.Parse(r["time_task"]),
                    Cost = int.Parse(r["cost"]),
                    PackSize = int.Parse(r["pack_size"])
                };
                db.WorkerConfig = worker;
            }
        }

        public static void LoadEquipmentConfig()
        {
            TextAsset csvData = Resources.Load<TextAsset>("equipment");
            if (csvData == null)
            {
                Debug.LogError("No Found equipment.csv!");
                return;
            }

            StringReader reader = new StringReader(csvData.text);

            string headerLine = reader.ReadLine();
            string[] headers = headerLine.Split(',');

            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (values.Length != headers.Length) continue;

                Dictionary<string, string> r = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    r[headers[i]] = values[i];
                }

                EquipmentConfig equipment = new EquipmentConfig
                {
                    Percent = float.Parse(r["percent"]),
                    LimitLevel = int.Parse(r["limit_level"]),
                    PackSize = int.Parse(r["pack_size"]),
                    Cost = int.Parse(r["cost"])
                };
                db.EquipmentConfig = equipment;
            }
        }

        public static void LoadPlotConfig()
        {
            TextAsset csvData = Resources.Load<TextAsset>("plot");
            if (csvData == null)
            {
                Debug.LogError("No Found plot.csv!");
                return;
            }

            StringReader reader = new StringReader(csvData.text);

            string headerLine = reader.ReadLine();
            string[] headers = headerLine.Split(',');

            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (values.Length != headers.Length) continue;

                Dictionary<string, string> r = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    r[headers[i]] = values[i];
                }

                PlotConfig plot = new PlotConfig
                {
                    Cost = int.Parse(r["cost"]),
                    PackSize = int.Parse(r["pack_size"]),
                };
                db.PlotConfig = plot;
            }
        }
    }
}
