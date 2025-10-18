using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WorkerData", menuName = "WorkerData")]
public class WorkerSO : ScriptableObject
{
    public WorkerConf WorkerConfig;
}

[Serializable]
public class WorkerConf
{
    public string Name;
    public int Cost;
    public int TimeTask;
    public int PackSize;
}