using Data.Config;
using Data.Manager;
using Plots.Controller;
using Sell.Controller;
using System.IO;
using UnityEngine;
using Utils.CsvTool;
using Utils.DesignPattern.Singleton;

namespace Game.Boostrap
{
    public class GameBoostrap : SingletonMono<GameBoostrap>
    {
        [Header(" Running Game ")]
        public string FilePath;
        public string FolderPath;

        protected override void Awake()
        {
            base.Awake();

            #if UNITY_EDITOR
                    // Editor: save directly in Assets/Data/Resources.
                    string folderPath = Path.Combine(Application.dataPath, "Data", "Resources");
            #else
                    // Build: dùng persistentDataPath
                    string folderPath = Path.Combine(Application.persistentDataPath, "Data");
            #endif
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
            DataManager.Instance.SetDateTimeOff();
            string json = DataManager.Instance.ConverData_GameToJson();
            File.WriteAllText(FilePath, json);
        }
    }
}