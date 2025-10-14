using TMPro;
using UnityEngine;

namespace Player.View
{
    public class PlayerView : MonoBehaviour
    {
        [Header(" PlayerView Setting ")]
        [SerializeField] private TextMeshProUGUI gold;

        public void SetupData(int gold)
        {
            this.gold.text = gold.ToString();
        }
    }
}

