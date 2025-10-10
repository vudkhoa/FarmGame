using System.Collections.Generic;
using System.IO;
using Data.Config;
using Data.Game;
using System;
using Product.Controller;
using Plots.Controller;

namespace Utils.CsvTool
{
    public static class CsvConfigLoader
    {
        public static ConfigDatabase db;

        public static ConfigDatabase LoadFromFolder(string folderPath)
        {
            db = new ConfigDatabase();
            LoadDataProductConfig(folderPath);
            LoadDataResourceInit(folderPath);
            LoadDataWorkerConfig(folderPath);
            return db;
        }

        private static void LoadDataProductConfig(string folderPath)
        {
            List<ProductConfig> productConfigList = new List<ProductConfig>();
            var productsPath = Path.Combine(folderPath, "products.csv");
            foreach (var r in CsvTool.Read(productsPath))
            {
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

        private static void LoadDataResourceInit(string folderPath)
        {
            var resourcesInitPath = Path.Combine(folderPath, "resources_init.csv");
            foreach (var r in CsvTool.Read(resourcesInitPath))
            {
                if (Enum.TryParse(r["name"].ToString(), out ProductType productType))
                {
                    ItemDetail bagInit = new ItemDetail
                    {
                        Id = r["id"],
                        Name = r["name"],
                        ProductType = productType,
                        Amount = int.Parse(r["amount"])
                    };
                    db.BagInitList[bagInit.Id] = bagInit;
                }
                else if (r["name"].ToString() == "Plots")
                {
                    for (int i = 0; i < int.Parse(r["amount"]); i++)
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
                    for (int i = 0; i < int.Parse(r["amount"]); i++)
                    {
                        WorkerDetail worker = new WorkerDetail();
                        worker.Id = i.ToString();
                        worker.State = Worker.Controller.WorkerState.Idle;
                        db.WorkerInitList[worker.Id] = worker;
                    }
                }
            }

        }
    
        private static void LoadDataWorkerConfig(string folderPath)
        {
            List<WorkerConfig> workerConfigList = new List<WorkerConfig>();
            string workerConfigPath = Path.Combine(folderPath, "worker.csv");
            foreach (var read in CsvTool.Read(workerConfigPath))
            {
                WorkerConfig worker = new WorkerConfig
                {
                    Name = read["name"],
                    TimeTask = int.Parse(read["time_task"]),
                    Cost = int.Parse(read["cost"]),
                    PackSize = int.Parse(read["pack_size"])
                };
                db.WorkerConfig = worker;
            }
        }
    }
}
