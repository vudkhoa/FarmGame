
using Data.Game;
using Shop.View;

namespace Shop.Model
{
    public class ShopModel
    {
        public ShopItemDetail Data;
        public ShopItemView View;

        public void InitData(ShopItemDetail data)
        {
            this.Data = new ShopItemDetail();
            this.Data.Id = data.Id;
            this.Data.NameProduct = data.NameProduct;
            this.Data.Price = data.Price;
            this.Data.PackSize = data.PackSize;
        }

        public void InitView(ShopItemView view)
        {
            this.View = view;
        }

        public void InitDataView(int index)
        {
            this.View.InitData(this.Data.NameProduct, this.Data.Price, index, this.Data.PackSize);
        }
    }
}

