using Bag.View;
using Data.Game;
using Data.Product;
using Product.Controller;

namespace Bag.Model
{
    public class BagModel
    {
        public BagItemView View;
        public int Id;
        public ProductTypeConf ProductType;
        public int ProductAmount;

        public void Setup(ItemDetail bagItem, BagItemView view)
        {
            this.Id = bagItem.Id;
            this.ProductType = bagItem.ProductType;
            this.ProductAmount = bagItem.Amount;
            this.View = view;
            this.View.Init(this.ProductType.Name, this.ProductAmount, this.Id);
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