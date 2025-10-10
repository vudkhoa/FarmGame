using Bag.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bag.View
{
    public class BagItemView : MonoBehaviour
    {
        [Header(" BagItemView Setting ")]
        [Header(" Button ")]
        [SerializeField] private Button button;

        [Header(" Name Text")]
        [SerializeField] private TextMeshProUGUI nameText;

        [Header(" Amount Text ")]
        [SerializeField] private TextMeshProUGUI amountText;

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

        public void Init(string name, int amount, int id)
        {
            this.SetName(name);
            this.SetAmount(amount);
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
    
        private void OnClickButton()
        {
            BagController.Instance.MoveToPlot(this.Id - 1, isClick: true);
        }
    }
}
