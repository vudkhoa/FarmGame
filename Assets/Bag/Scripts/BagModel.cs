using Bag.View;
using Data.Game;
using Product.Controller;

namespace Bag.Model
{
    public class BagModel
    {
        public BagItemView View;
        public int Id;
        public ProductType ProductType;
        public string ProductName;
        public int ProductAmount;

        public void Setup(ItemDetail bagItem, BagItemView view)
        {
            this.Id = int.Parse(bagItem.Id);
            this.ProductType = bagItem.ProductType;
            this.ProductName = bagItem.Name;
            this.ProductAmount = bagItem.Amount;
            this.View = view;
            this.View.Init(this.ProductName, this.ProductAmount, this.Id);
        }
    
        public void CaculateAmount(int value)
        {
            int amount = this.ProductAmount + value;
            if (amount < 0) return;

            this.ProductAmount = amount;
            this.View.SetAmount(this.ProductAmount);
        }
    }
}