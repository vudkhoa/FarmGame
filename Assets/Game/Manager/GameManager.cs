using System;
using UnityEngine;
using Utils.DesignPattern.Singleton;
using Game.Boostrap;

namespace Game.Manager
{
    public class GameManager : SingletonMono<GameManager>
    {
        public GameState State;

        private void Start()
        {
            GameModeContainer.Instance.InitGame();
            InitController();
            GameBoostrap.Instance.LoadAllData();
        }

        private void InitController()
        {
            CreateModule("UIManager", "UIManager");
            CreateModule("BagController", "BagController");
            CreateModule("SellController", "SellController");
            CreateModule("PlotController", "PlotController");
            CreateModule("WorkerController", "WorkerController");
        }

        private GameObject CreateModule(string module, string nameModule)
        {
            GameObject loginObject = GameObject.Instantiate(Resources.Load<GameObject>(module), GameModeContainer.Instance.Manager.transform);
            loginObject.name = nameModule;

            return loginObject;
        }

        public void PauseGame()
        {
            this.State = GameState.Pause;
        }

        public void ResumeGame()
        {
            this.State = GameState.Playing;
        }

        private void OnApplicationQuit()
        {
            GameBoostrap.Instance.SaveAllData();
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

