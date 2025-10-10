using Product.Controller;

namespace Product.Model
{
    public class ProductModel    
    {
        public ProductType Type;
        public int idPlot;

        public void Init(ProductType type, int idPlot)
        {
            this.Type = type;
            this.idPlot = idPlot;
        }
    }
}
