using Bag.Controller;
using Bag.Model;
using Data.Game;
using Data.Manager;
using Player.Controller;
using Product.Controller;
using Sell.Model;
using Sell.View;
using System.Collections.Generic;
using UnityEngine;
using Utils.DesignPattern.Singleton;

namespace Sell.Controller
{
    public class SellController : SingletonMono<SellController>
    {
        [Header("  SellController Setting ")]
        [SerializeField] private SellItemView SellItemPrefab;
        [SerializeField] public RectTransform ParentAllSellItem;

        [Header(" Running Game ")]
        public List<SellModel> SellModelList;

        public void Init()
        {
            this.SellModelList = new List<SellModel>();
            int index = -1;
            foreach (BagModel bagItem in BagController.Instance.BagModelList) 
            {
                index++;
                SellModel sellModel = new SellModel();
                SellItemView view = Instantiate(this.SellItemPrefab, this.ParentAllSellItem);

                ItemDetail sellItem = new ItemDetail();
                foreach (ItemDetail item in DataManager.Instance.GameData.SellItemList)
                {
                    if (bagItem.ProductName.ToString() == item.Name.ToString())
                    {
                        sellItem.Amount = item.Amount;
                    }
                }

                sellItem.Name = bagItem.ProductName;
                sellItem.Id = index.ToString();
                sellItem.ProductType = bagItem.ProductType;

                int price = DataManager.Instance.GameConfig.ProductConfigList[index].Price;

                sellModel.Setup(sellItem, view, price);
                SellModelList.Add(sellModel);
            }
        }

        public int FindIndexByProductType(ProductType type)
        {
            int count = -1;
            foreach (SellModel model in this.SellModelList)
            {
                count++;
                if (model.ProductType == type)
                {
                    return count;
                }
            }
            return -1;
        }

        public void CaculateAmountByIndex(int index, int value)
        {
            this.SellModelList[index].CaculateAmount(value);
        }

        public void SellItem(int index)
        {
            Debug.Log("Sell: " + this.SellModelList[index].ProductAmount);
            if (this.SellModelList[index].ProductAmount <= 0) return;
            PlayerController.Instance.CaculateGold(this.SellModelList[index].ProductAmount * 
                                                    this.SellModelList[index].Price);
            this.SellModelList[index].CaculateAmount(-this.SellModelList[index].ProductAmount);
        }
    
        public void SaveData()
        {
            DataManager.Instance.GameData.SellItemList.Clear();

            foreach (SellModel model in this.SellModelList)
            {
                ItemDetail sell = new ItemDetail();
                sell.Id = model.Id.ToString();
                sell.ProductType = model.ProductType;
                sell.Name = model.ProductName;
                sell.Amount = model.ProductAmount;
                DataManager.Instance.GameData.SellItemList.Add(sell);
            }
        }
    }
}