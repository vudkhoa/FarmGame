using TMPro;
using UnityEngine;

namespace Equipment.View
{
    public class EquipmentView : MonoBehaviour
    {
        [Header(" PlayerView Setting ")]
        [SerializeField] private TextMeshProUGUI curLevel;

        public void SetupData(int level, int maxLevel)
        {
            this.curLevel.text = level.ToString() + "/" + maxLevel.ToString();
        }
    }
}