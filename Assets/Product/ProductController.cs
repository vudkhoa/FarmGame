using Product.Model;
using System.Collections.Generic;
using UnityEngine;
using Utils.DesignPattern.Singleton;

namespace Product.Controller 
{
    public enum ProductType
    {
        None = 0,
        Tomato = 1,
        Blueberry = 2, 
        Strawberry = 3, 
        Cow = 4
    }

    

    public class ProductController : SingletonMono<ProductController>
    {
        [Header(" Running Game ")]
        public List<ProductModel> ProductList;
        
        public void Init() 
        {  
            
        }

        private void GetData() { }

        
    }
}