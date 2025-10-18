using Data.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Product
{
    [CreateAssetMenu(fileName = "ProductSO", menuName = "ProductData")]
    public class ProductSO : ScriptableObject
    {
        public List<ProductTypeConf> ListProductType;
        public List<ProductConf> ListProductConf;

    }

    [Serializable] 
    public class ProductConf
    {
        public int Id;
        public ProductTypeConf ProductType;
        public int Interval;
        public int Lifetime;
        public int Cost;
        public int Price;
        public int PackSize;
        public int Deadline;
    }

    [Serializable]
    public class ProductTypeConf 
    {
        public int Id;
        public string Name;
        public ProductTypeConf()
        {
            if (DataManager.Instance != null)
            {
                this.Id = DataManager.Instance.ProductConfigData.ListProductType[0].Id;
                this.Name = DataManager.Instance.ProductConfigData.ListProductType[0].Name;
            }
        }
    }
}