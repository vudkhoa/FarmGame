using System;
using UnityEngine;


[CreateAssetMenu(fileName = "EquipmentData", menuName = "EquipmentData")]
public class EquipmentSO : ScriptableObject
{
    public EquipmentConf EquipmentConfig;
}

[Serializable]
public class EquipmentConf
{
    public int Percent;
    public int LimitLevel;
    public int Cost;
    public int PackSize;
}