using Data.Product;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourcesInitData", menuName = "ResourcesInitData")]
public class ResourcesInitSO : ScriptableObject
{
    public ProductSO ProductConfig;
    public List<ResourceProduct> ListResourceInit;
    public List<ResourceByName> ListResourceByName;
}

[Serializable]
public class ResourceProduct
{
    public ProductTypeConf ProductType;
    public int Amount;
}

[Serializable]
public class ResourceByName
{
    public string Name;
    public int Amount;
}