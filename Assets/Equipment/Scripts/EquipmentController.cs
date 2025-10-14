using Bag.Controller;
using Data.Config;
using Data.Manager;
using Equipment.Model;
using Equipment.View;
using Plots.Controller;
using Plots.Model;
using Sell.Controller;
using UnityEngine;
using Utils.DesignPattern.Singleton;
using Worker.Controller;

namespace Equipment.Controller
{
    public class EquipmentController : SingletonMono<EquipmentController>
    {
        [Header(" EquipmentController Setting ")]
        [SerializeField] private EquipmentView equipmentViewPrefab;
        [SerializeField] private Transform parentAllView;

        [Header(" Running Game ")]
        public EquipmentModel Equipment;

        private void Start()
        {
            this.Init();
            BagController.Instance.Init();
            this.FillData();
            SellController.Instance.Init();
            WorkerController.Instance.Init();
            DataManager.Instance.FillIntoSell();
        }

        public void Init()
        {
            this.Equipment = new EquipmentModel();
            this.Equipment.Init(DataManager.Instance.GameData.Equipment);

            EquipmentView view = Instantiate(this.equipmentViewPrefab, this.parentAllView);
            this.Equipment.InitView(view);
        }

        public void CaculateLevel(int value)
        {
            int tmpLevel = this.Equipment.Data.CurrentLevel;
            this.Equipment.CaculateLevel(value);

            if (tmpLevel == this.Equipment.Data.CurrentLevel) 
            {
                return;
            }

            this.FillData();

        }

        public void FillData()
        {
            if (this.Equipment.Data.CurrentLevel <= 1)
            {
                return;
            }
            //Debug.Log(this.Equipment.Data.CurrentLevel);
            float percent = (DataManager.Instance.GameConfig.EquipmentConfig.Percent / 100) * this.Equipment.Data.CurrentLevel;
            
            foreach (ProductConfig config in DataManager.Instance.GameConfig.ProductConfigList)
            {
                int newInterval = config.Interval - (int)(config.Interval * percent);
                config.Interval = newInterval;
                Debug.Log(config.Interval);
            }

            if (PlotController.Instance.PlotModelList == null)
            {
                return;
            }

            foreach (PlotModel plotModel in PlotController.Instance.PlotModelList)
            {
                if (plotModel.Data.CurLife < 0 && plotModel.Data.CurTime < 0) { continue; }

                int newInterval = plotModel.Interval - (int)(plotModel.Interval * percent);
                plotModel.Interval = newInterval;
                Debug.Log(plotModel.Data.ProductType + " " + plotModel.Interval);
            }
        }
    }
}