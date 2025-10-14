using Bag.Controller;
using Sell.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sell.View
{
    public class SellItemView : MonoBehaviour
    {
        [Header(" SellItemView Setting ")]
        [Header(" Button ")]
        [SerializeField] private Button button;

        [Header(" Name Text")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI priceText;

        [Header(" Running Game ")]
        public int Id;

        private void OnEnable()
        {
            button.onClick.AddListener(OnClickButton);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClickButton);
        }

        public void Init(string name, int amount, int id, int price)
        {
            this.SetName(name);
            this.SetAmount(amount);
            this.SetPrice(price);
            this.Id = id;
        }

        public void SetName(string name)
        {
            this.nameText.text = name;
        }

        public void SetAmount(int amount)
        {
            this.amountText.text = amount.ToString();
        }

        public void SetPrice(int price)
        {
            this.priceText.text = price.ToString(); 
        }

        private void OnClickButton()
        {
            SellController.Instance.SellItem(this.Id);
        }
    }
}