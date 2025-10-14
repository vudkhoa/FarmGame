using Data.Game;
using Product.Controller;
using Sell.View;

namespace Sell.Model
{
    public class SellModel
    {
        public SellItemView View;
        public int Id;
        public ProductType ProductType;
        public string ProductName;
        public int ProductAmount;
        public int Price;

        public void Setup(ItemDetail sellItem, SellItemView view, int price)
        {
            this.Id = int.Parse(sellItem.Id);
            this.ProductType = sellItem.ProductType;
            this.ProductName = sellItem.Name;
            this.ProductAmount = sellItem.Amount;
            this.View = view;
            this.Price = price;
            this.View.Init(this.ProductName, this.ProductAmount, this.Id, this.Price);
        }

        public void CaculateAmount(int value)
        {
            int tmp = this.ProductAmount + value;
            if (tmp < 0)
            {
                return;
            }

            this.ProductAmount = tmp;
            this.View.SetAmount(this.ProductAmount);
        }
    }
}