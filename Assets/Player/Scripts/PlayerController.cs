using Data.Game;
using Data.Manager;
using Player.Model;
using Player.View;
using UnityEngine;
using Utils.DesignPattern.Singleton;

namespace Player.Controller
{
    public class PlayerController : SingletonMono<PlayerController>
    {
        [Header("PlayerController Setting ")]
        [SerializeField] private PlayerView playerViewPrefab;
        [SerializeField] private Transform parentAllPlayerView;

        [Header(" Running Game ")]
        public PlayerModel Player;

        private void Start()
        {
            this.Init();
        }

        public void Init()
        {
            this.Player = new PlayerModel();
            this.Player.Init(DataManager.Instance.GameData.Player);

            PlayerView view = Instantiate(this.playerViewPrefab, this.parentAllPlayerView);
            this.Player.InitView(view);
        }

        public int CaculateGold(int value)
        {
            int resultGold = this.Player.Data.Gold + value;
            if (resultGold < 0)
            {
                return resultGold;
            }
            else if (resultGold >= 1000000)
            {
                Debug.Log("Win Game");
            }
            this.Player.SetupData(resultGold);
            return resultGold;
        }

        public void SaveData()
        {
            DataManager.Instance.GameData.Player = new PlayerDetail();
            DataManager.Instance.GameData.Player = this.Player.Data;
        }
    }
}