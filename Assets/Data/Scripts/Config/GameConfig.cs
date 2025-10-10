using System;
using System.Collections.Generic;
using Data.Game;

namespace Data.Config
{
    [Serializable]
    public class GameConfig
    {
        public List<ProductConfig> ProductConfigList;
        public WorkerConfig WorkerConfig;
    }

    [Serializable]
    public class ProductConfig
    {
        public string Id;
        public string Name;
        public int Interval;
        public int Lifetime;
        public int Cost;
        public int Price;
        public int PackSize;
        public int Deadline;
    }

    [Serializable]
    public class WorkerConfig
    {
        public string Name;
        public int Cost;
        public int TimeTask;
        public int PackSize;
    }

    public class ConfigDatabase
    {
        public Dictionary<string, ProductConfig> ProductList { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, ItemDetail> BagInitList { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, PlotDetail> PlotsInitList { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, WorkerDetail> WorkerInitList { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public WorkerConfig WorkerConfig { get; set; }
    }
}
