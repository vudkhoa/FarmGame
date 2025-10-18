using Data.Game;
using Data.Product;
using Sell.View;

namespace Sell.Model
{
    public class SellModel
    {
        public SellItemView View;
        public int Id;
        public ProductTypeConf ProductType;
        public int ProductAmount;
        public int Price;

        public void Setup(ItemDetail sellItem, SellItemView view, int price)
        {
            this.Id = sellItem.Id;
            this.ProductType = sellItem.ProductType;
            //this.ProductName = sellItem.Name;
            this.ProductAmount = sellItem.Amount;
            this.View = view;
            this.Price = price;
            this.View.Init(this.ProductType.Name, this.ProductAmount, this.Id, this.Price);
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