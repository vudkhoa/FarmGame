using Shop.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop.View
{
    public class ShopItemView : MonoBehaviour
    {
        [Header(" ShopItemView Setting ")]
        [SerializeField] private Button PurchaseButton;
        [SerializeField] private TextMeshProUGUI productName;
        [SerializeField] private TextMeshProUGUI size;
        [SerializeField] private TextMeshProUGUI price;

        [Header(" Running Game ")]
        public int Index;

        public void InitData(string name, int price, int index, int size)
        {
            this.SetupData(name, price, size);
            this.Index = index;
        }

        public void SetupData(string name, int price, int size)
        {
            this.productName.text = name;
            this.size.text = "Size: " + size.ToString();
            this.price.text = "Price: " + price.ToString();
        }

        private void OnEnable()
        {
            PurchaseButton.onClick.AddListener(OnClickButton);
        }

        private void OnDisable()
        {
            PurchaseButton.onClick.RemoveListener(OnClickButton);
        }

        private void OnClickButton()
        {
            ShopController.Instance.PurchaseItem(this.Index);
        }
    }
}