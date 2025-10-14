using Bag.Model;
using Bag.View;
using Data.Config;
using Data.Game;
using Data.Manager;
using Plots.Controller;
using Product.Controller;
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
                //Debug.Log(this.BagModelList[id] + " " + this.BagModelList[id].ProductName);
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
            Debug.Log("Plot Index: " +  indexPlot);
            if (indexPlot == -1) { return; }
            PlotController.Instance.SetPlotByIndex(indexPlot, this.BagModelList[id].ProductType);
            this.BagModelList[id].CaculateAmount(-1);
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

        public void CaculateProductAmountByName(string name, int value)
        {
            int index = -1;
            foreach (BagModel model in this.BagModelList)
            {
                index++;
                if (string.Equals(model.ProductName.ToString(), name.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    this.BagModelList[index++].CaculateAmount(value);
                    return;
                }
            }
        }

        private void FillOffset()
        {

        }

        public int GetIndex(ProductType type)
        {
            int index = -1;
            foreach (BagModel model in this.BagModelList)
            {
                index++;
                if (model.ProductType == type)
                {
                    return index;
                }
            }
            return -1;
        }

        public void SaveData()
        {
            DataManager.Instance.GameData.BagItemList.Clear();

            foreach (BagModel model in this.BagModelList)
            {
                ItemDetail bagItem = new ItemDetail();
                bagItem.Id = model.Id.ToString();
                bagItem.ProductType = model.ProductType;
                bagItem.Name = model.ProductName;
                bagItem.Amount = model.ProductAmount;
                DataManager.Instance.GameData.BagItemList.Add(bagItem);
            }
        }
    }
}
