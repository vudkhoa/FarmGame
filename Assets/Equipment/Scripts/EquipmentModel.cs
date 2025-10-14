using Data.Game;
using Data.Manager;
using Equipment.View;

namespace Equipment.Model
{
    public class EquipmentModel
    {
        public EquipmentDetail Data;
        public EquipmentView View;

        public void Init(EquipmentDetail data)
        {
            this.Data = new EquipmentDetail();
            if (data == null)
            {
                this.Data.CurrentLevel = 0;
                return;
            }
            this.Data = data;
        }

        public void InitView(EquipmentView view)
        {
            this.View = view;
            this.View.SetupData(this.Data.CurrentLevel, DataManager.Instance.GameConfig.EquipmentConfig.LimitLevel);
        }
    
        public void CaculateLevel(int value)
        {
            float tmpLevel = this.Data.CurrentLevel + value;
            if (tmpLevel > DataManager.Instance.GameConfig.EquipmentConfig.LimitLevel)
            {
                return;
            }
            this.Data.CurrentLevel += value;
            this.View.SetupData(this.Data.CurrentLevel, DataManager.Instance.GameConfig.EquipmentConfig.LimitLevel);
        }
    }
}