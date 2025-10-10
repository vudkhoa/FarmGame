using Bag.Controller;
using Plots.Controller;
using Product.Controller;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Worker.Model;

namespace Plots.View
{
    public class PlotView : MonoBehaviour
    {
        [Header(" Plots View Setting ")]
        [Header(" Button ")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI CurName;
        [SerializeField] private TextMeshProUGUI CurAmount;
        [SerializeField] private TextMeshProUGUI CurLifetime;

        [Header(" Running Game ")]
        public TextMeshProUGUI CurTime;

        private int id;
        public bool IsAvai;
        public bool IsResetData;
        public bool IsDelete;
        public DateTime tmp;

        private void OnEnable()
        {
            button.onClick.AddListener(OnClickButton);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClickButton);
        }

        public void SetId(int id)
        {
            this.id = id;
        }

        public void SetProductType(ProductType productType)
        {
            this.CurName.text = productType.ToString();
        }

        public void SetAmount(int amount)
        {
            this.CurAmount.text = amount.ToString();
        }

        public void Setup(float curTime = 0, int curLife = 0, int amount = 0)
        {
            this.CurAmount.text = amount.ToString();
            this.CurTime.text = curTime.ToString();
            this.CurLifetime.text = curLife.ToString();
            this.CurAmount.text = amount.ToString();
        }

        private void FixedUpdate()
        {
            if (DateTime.Parse(PlotController.Instance.PlotModelList[this.id].Data.Deadline) < DateTime.Now &&
                DateTime.Parse(PlotController.Instance.PlotModelList[this.id].Data.Deadline) != DateTime.MinValue)
            {
                this.DeleteProductoOnPlot();
                return;
            }

            if (!this.IsAvai || this.IsResetData)
            {
                return;
            }
            this.Countdown();
        }

        public void SetAvai(bool isAvai)
        {
            this.IsAvai = isAvai;
        }

        public void SetResetData(bool isResetData)
        {
            this.IsResetData = isResetData;
        }

        public void Countdown(float time = 1f)
        {
            this.SetAvai(false);
            Invoke(nameof(IncreaseTime), time);
        }

        private void IncreaseTime()
        {
            PlotController.Instance.IncreaseTime(1f, this.id);
            this.SetAvai(true);
        }

        private void DeleteProductoOnPlot()
        {
            PlotController.Instance.SetNullById(this.id);
        }

        public void ResetData()
        {
            this.SetAvai(false);
            this.CurLifetime.text = 0.ToString();
            this.CurTime.text = 0.ToString();
        }

        public void Show()
        {
            this.CurName.gameObject.SetActive(true);
            this.CurAmount.gameObject.SetActive(true);
            this.CurLifetime.gameObject.SetActive(true);
            this.CurTime.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.CurName.gameObject.SetActive(false);
            this.CurAmount.gameObject.SetActive(false);
            this.CurLifetime.gameObject.SetActive(false);
            this.CurTime.gameObject.SetActive(false);
        }
    
        public void SetNullView()
        {
            this.CurName.text = "None";
            this.CurTime.text = 0.ToString();
            this.CurLifetime.text = 0.ToString();
            this.CurAmount.text = 0.ToString();
        }

        private void OnClickButton()
        {
            PlotController.Instance.MoveToSell(this.id);
        }
    }
}