using Data.Game;
using Player.View;

namespace Player.Model
{
    public class PlayerModel
    {
        public PlayerDetail Data;
        public PlayerView View;

        public void Init(PlayerDetail data)
        {
            this.Data = new PlayerDetail();
            if (data == null)
            {
                this.Data.Gold = 0;
                return;
            }
            this.Data = data;
        }

        public void InitView(PlayerView view)
        {
            this.View = view;
            this.View.SetupData(this.Data.Gold);
        }

        public void SetupData(int gold)
        {
            this.Data.Gold = gold;
            this.View.SetupData(this.Data.Gold);
        }
    }
}