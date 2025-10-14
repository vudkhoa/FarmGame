using Game.Boostrap;
using System;
using UnityEngine;
using Utils.DesignPattern.Singleton;

namespace Game.Manager
{
    public class GameManager : SingletonMono<GameManager>
    {
        public GameState State;

        private void Start()
        {
            GameModeContainer.Instance.InitGame();
            InitController();
            GameBootstrap.Instance.LoadAllData();
        }
        private void InitController()
        {
            CreateModule("DataManager", "DataManager");
            CreateModule("UIManager", "UIManager");
            CreateModule("EquipmentController", "EquipmentController");
            CreateModule("ShopController", "ShopController");
            CreateModule("PlayerController", "PlayerController");
            CreateModule("PlotController", "PlotController");
            CreateModule("WorkerController", "WorkerController");
            CreateModule("BagController", "BagController");
            CreateModule("SellController", "SellController");
        }

        private GameObject CreateModule(string module, string nameModule)
        {
            GameObject loginObject = GameObject.Instantiate(Resources.Load<GameObject>(module), GameModeContainer.Instance.Manager.transform);
            loginObject.name = nameModule;

            return loginObject;
        }

        private void OnApplicationQuit()
        {
            GameBootstrap.Instance.SaveAllData();
        }

        public double GetDistanceWithNow(DateTime time)
        {
            return (DateTime.Now - time).TotalSeconds;
        }
    }

    [Serializable]
    public enum GameState
    {
        None = 0,
        Playing = 1,
        Pause = 2,
        GameOver = 3,
    }
}

