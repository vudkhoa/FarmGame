using Bag.Controller;
using Bag.Model;
using Data.Config;
using Data.Game;
using Data.Manager;
using Equipment.Controller;
using Player.Controller;
using Plots.Controller;
using Plots.Model;
using Plots.View;
using Shop.Model;
using Shop.View;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.DesignPattern.Singleton;
using Worker.Controller;
using Worker.Model;

namespace Shop.Controller
{
    public class ShopController : SingletonMono<ShopController>
    {
        [Header(" ShopController Setting ")] 
        [SerializeField] private ShopItemView ItemViewPrefab;
        [SerializeField] private RectTransform ParentAllItemView;

        [Header("Running Game")]
        public List<ShopModel> ShopItemList;

        private void Start()
        {
            this.Init();
        }

        public void Init()
        {
            this.ShopItemList = new List<ShopModel>();  
            int index = -1;
            foreach(ProductConfig config in DataManager.Instance.GameConfig.ProductConfigList)
            {
                index++;
                ShopModel item = new ShopModel();
                ShopItemDetail data = new ShopItemDetail();
                data.Id = index;
                data.Price = config.Cost;
                data.NameProduct = config.Name;
                data.PackSize = config.PackSize;

                BagModel bagModel = new BagModel();
                ShopItemView view = Instantiate(this.ItemViewPrefab, this.ParentAllItemView);

                item.InitData(data);
                item.InitView(view);
                item.InitDataView(index);
                this.ShopItemList.Add(item);

            }
            // worker
            WorkerConfig workerConfig = new WorkerConfig();
            workerConfig = DataManager.Instance.GameConfig.WorkerConfig;

            index++;
            ShopModel itemWorker = new ShopModel();
            ShopItemDetail dataWorker = new ShopItemDetail();
            dataWorker.Id = index;
            dataWorker.Price = workerConfig.Cost;
            dataWorker.NameProduct = "Worker";
            dataWorker.PackSize = workerConfig.PackSize;
            ShopItemView viewWorker = Instantiate(this.ItemViewPrefab, this.ParentAllItemView);

            itemWorker.InitData(dataWorker);
            itemWorker.InitView(viewWorker);
            itemWorker.InitDataView(index);
            this.ShopItemList.Add(itemWorker);

            // plot
            PlotConfig plotConfig = new PlotConfig();
            plotConfig = DataManager.Instance.GameConfig.PlotConfig;

            index++;
            ShopModel itemPlot = new ShopModel();
            ShopItemDetail dataPlot = new ShopItemDetail();
            dataPlot.Id = index;
            dataPlot.Price = plotConfig.Cost;
            dataPlot.NameProduct = "Plot";
            dataPlot.PackSize = plotConfig.PackSize;
            ShopItemView viewPlot = Instantiate(this.ItemViewPrefab, this.ParentAllItemView);

            itemPlot.InitData(dataPlot);
            itemPlot.InitView(viewPlot);
            itemPlot.InitDataView(index);
            this.ShopItemList.Add(itemPlot);

            // euqipment
            EquipmentConfig equipment = new EquipmentConfig();
            equipment = DataManager.Instance.GameConfig.EquipmentConfig;

            index++;
            ShopModel itemEquipment = new ShopModel();
            ShopItemDetail dataEquipment = new ShopItemDetail();
            dataEquipment.Id = index;
            dataEquipment.Price = equipment.Cost;
            dataEquipment.NameProduct = "Equipment";
            dataEquipment.PackSize = equipment.PackSize;
            ShopItemView viewEquipment = Instantiate(this.ItemViewPrefab, this.ParentAllItemView);

            itemEquipment.InitData(dataEquipment);
            itemEquipment.InitView(viewEquipment);
            itemEquipment.InitDataView(index);
            this.ShopItemList.Add(itemEquipment);

        }

        public void PurchaseItem(int index)
        {
            int resultGold = PlayerController.Instance.CaculateGold(-this.ShopItemList[index].Data.Price);

            if (resultGold > 0) 
            {
                string name = this.ShopItemList[index].Data.NameProduct;
                int packSize = this.ShopItemList[index].Data.PackSize;
                if (string.Equals(name, "Worker".ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    WorkerDetail worker = new WorkerDetail();
                    worker.Id = WorkerController.Instance.WorkerList.Count.ToString();
                    worker.State = Worker.Controller.WorkerState.Idle;
                    WorkerModel model = new WorkerModel();

                    model.Id = int.Parse(worker.Id);
                    model.State = worker.State;
                    model.IdProduct = -1;
                    model.IdPlot = -1;
                    model.TimeTask = DataManager.Instance.GameConfig.WorkerConfig.TimeTask;
                    model.StartTime = DateTime.MinValue;
                    model.NotExecute = false;

                    model.StateMachine.Change(model, new Idle());
                    WorkerController.Instance.View.SetAmount(WorkerController.Instance.CurIdle, WorkerController.Instance.WorkerList.Count + 1);
                    WorkerController.Instance.WorkerList.Add(model);

                }
                else if (string.Equals(name, "Plot".ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    int count = PlotController.Instance.PlotModelList.Count;
                    PlotDetail plot = new PlotDetail();
                    plot.Status = PlotStatus.IsAvai;
                    plot.CurTime = -1;
                    plot.CurLife = -1;

                    PlotModel model = new PlotModel();
                    plot.Id = count.ToString();

                    PlotView view = Instantiate(PlotController.Instance.PlotPrefab, PlotController.Instance.ParentAllView);
                    model.Init(plot, view, count);
                    model.DurationDeadline = 0;
                    PlotController.Instance.PlotModelList.Add(model);
                }
                else if (string.Equals(name, "Equipment".ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    EquipmentController.Instance.CaculateLevel(1);
                }
                else
                {
                    BagController.Instance.CaculateProductAmountByName(name, packSize);
                }
                
            }
        }
    }
}