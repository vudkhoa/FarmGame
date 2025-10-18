using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlotData", menuName = "PlotData")]
public class PlotSO : ScriptableObject
{
    public PlotConf PlotConfig;
}

[Serializable]
public class PlotConf
{
    public int Cost;
    public int PackSize;
}