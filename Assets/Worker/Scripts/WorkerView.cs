using TMPro;
using UnityEngine;

namespace Worker.View
{
    public class WorkerView : MonoBehaviour
    {
        [Header(" WorkerView Setting ")]
        [SerializeField] private TextMeshProUGUI amount;

        public void SetAmount(int curAmount, int totalAmount)
        {
            amount.text = curAmount.ToString() + "/" + totalAmount.ToString();
        }
    }
}