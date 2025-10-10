using Bag.Model;
using Bag.View;
using Data.Config;
using Data.Game;
using Data.Manager;
using Plots.Controller;
using Product.Controller;
using Sell.Controller;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.DesignPattern.Singleton;

namespace Bag.Controller
{
    public class BagController : SingletonMono<BagController>
    {
        [Header(" BagController Setting ")]
        public BagItemView BagViewPrefab;
        public RectTransform ParentBagView;
        public List<BagModel> BagModelList;

        private void Start()
        {
            this.Init();
            SellController.Instance.Init();
        }

        public void Init()
        {
            // Model
            this.BagModelList = new List<BagModel>();
            foreach (ProductConfig component in DataManager.Instance.GameConfig.ProductConfigList)
            {
                BagModel bagModel = new BagModel();
                BagItemView view = Instantiate(this.BagViewPrefab, this.ParentBagView);

                ItemDetail bagItem = new ItemDetail();
                foreach (ItemDetail item in DataManager.Instance.GameData.BagItemList)
                {
                    if (component.Name.ToString() == item.Name.ToString()) 
                    {
                        bagItem.Amount = item.Amount;
                    }
                }

                bagItem.Name = component.Name;
                bagItem.Id = component.Id;
                bagItem.ProductType = (ProductType)Enum.Parse(typeof(ProductType), bagItem.Name);

                bagModel.Setup(bagItem, view);
                BagModelList.Add(bagModel);
            }
        }

        public void MoveToPlot(int id, int idPlot = -1, bool isClick = false)
        {
            if (this.BagModelList[id].ProductAmount <= 0)
            {
                return;
            }

            int indexPlot;
            if (idPlot == -1)
            {
                indexPlot = PlotController.Instance.FindPlotAvai(isClick);
            }
            else
            {
                indexPlot = idPlot;
            }

            if (indexPlot == -1) { return; }
            PlotController.Instance.SetPlotByIndex(indexPlot, this.BagModelList[id].ProductType);
            this.BagModelList[id].ReduceAmount(1);
        }

        public int ChooseProduct()
        {
            if (this.BagModelList == null) {  return -1; }
            foreach (BagModel model in this.BagModelList)
            {
                if (model.ProductAmount > 0)
                {
                    return model.Id;
                }
            }
            return -1;
        }
    }
}
