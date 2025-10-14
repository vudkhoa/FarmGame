using Bag.Controller;
using Data.Config;
using Data.Manager;
using Player.Controller;
using Plots.Controller;
using Sell.Controller;
using System.IO;
using UnityEngine;
using Utils.CsvTool;
using Utils.DesignPattern.Singleton;
using Worker.Controller;

namespace Game.Boostrap
{
    public class GameBootstrap : SingletonMono<GameBootstrap>
    {
        [Header(" Running Game ")]
        public string FilePath;
        public string FolderPath;

        protected override void Awake()
        {
            base.Awake();

            // Editor: save directly in Assets/Data/Resources.
            string folderPath = Path.Combine(Application.dataPath, "Data", "Resources");
            
            // Ensure the folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            // Set the file path to a specific file in the "Data" folder
            this.FolderPath = folderPath;
            FilePath = Path.Combine(folderPath, "GameData.json");

            this.FilePath = this.FilePath.Replace("\\", "/");
        }

        public void LoadAllData()
        {
            Directory.CreateDirectory(this.FolderPath);
            ConfigDatabase db = CsvConfigLoader.LoadFromFolder(this.FolderPath);

            DataManager.Instance.Init();
            DataManager.Instance.ConvertData_ConfigToGame(db);
        }

        public void SaveAllData() 
        {
            PlotController.Instance.SaveData();
            SellController.Instance.SaveData();
            WorkerController.Instance.SaveData();
            BagController.Instance.SaveData();
            PlayerController.Instance.SaveData();
            DataManager.Instance.SetDateTimeOff();
            string json = DataManager.Instance.ConverData_GameToJson();
            File.WriteAllText(FilePath, json);
        }
    }
}