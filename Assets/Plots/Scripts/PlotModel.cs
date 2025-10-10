using Data.Game;
using Plots.Controller;
using Plots.View;
using Product.Controller;
using System;
using System.Diagnostics;

namespace Plots.Model
{
    public class PlotModel
    {
        private int id;
        public PlotDetail Data;
        public PlotView View;
        public int Interval;
        public int Lifetime;
        public int DurationDeadline;

        public void Init(PlotDetail plot, PlotView view, int id)
        {
            // Data
            Data = new PlotDetail();
            this.Data = plot;
            this.View = view;
            this.id = id;

            // View
            this.View.Show();
            this.View.SetAvai(false);
            this.View.SetResetData(false);
            this.View.SetId(id);
            this.View.SetProductType(this.Data.Type);
            this.View.Setup(0f, 0, this.Data.CurAmount);
        }

        public void Setup(ProductType type, int interval, int lifetime)
        {
            // Data
            this.Data.Type = type;
            this.Interval = interval;
            this.Lifetime = lifetime;

            // View
            this.View.Show();
            float timeView = this.Interval - this.Data.CurTime;
            int lifeView = this.Lifetime - this.Data.CurLife;
            this.View.SetProductType(this.Data.Type);
            this.View.Setup(timeView, lifeView, this.Data.CurAmount);
            this.View.SetAvai(true);
            this.View.SetResetData(false);
        }

        public void SetCurLife(int offset)
        {
            this.Data.CurLife += offset;
            float timeView = this.Interval - this.Data.CurTime;
            int lifeView = this.Lifetime - this.Data.CurLife;
            this.View.Setup(timeView, lifeView, this.Data.CurAmount);
        }

        public void SetCurTime(float curTime, int amount)
        {
            this.View.Show();
            this.Data.CurTime = curTime;
            this.Data.CurAmount += amount;
            float timeView = this.Interval - this.Data.CurTime;
            int lifeView = this.Lifetime - this.Data.CurLife;
            this.View.Setup(timeView, lifeView, this.Data.CurAmount);
        }

        public void ResetData()
        {
            this.Data.CurLife = -1;
            this.Data.CurTime = -1;
            this.View.SetResetData(true);
            this.View.ResetData();
        }
    
        public void SetNullData()
        {
            string id = this.Data.Id;
            this.Data = new PlotDetail();
            this.Data.CurTime = -1;
            this.Data.CurLife  = -1;
            this.Data.Id = id;
            this.Data.Status = PlotStatus.IsAvai;
            this.Interval = 0;
            this.Lifetime = 0;
            this.View.SetNullView();
        }

        public void ReduceAmount(int value)
        {
            int tmp = this.Data.CurAmount - value;
            if (tmp == 0 && this.Data.CurLife == -1 && this.Data.CurTime == -1)
            {
                UnityEngine.Debug.Log("Check Done");
                this.SetNullData();
            }
            else
            {
                this.Data.CurAmount = tmp;
                this.View.SetAmount(tmp);
            }
        }
    
        public void Countdown(float time)
        {
            this.View.Countdown(time);
        }
    
        public void CaculateDeadline()
        {
            DateTime now = DateTime.Now;
            this.Data.Deadline = now.AddSeconds(this.DurationDeadline).ToString();
        }
        
        public void SetDeadline(DateTime dateTime)
        {
            this.Data.Deadline = dateTime.ToString();
        }
    }
}
