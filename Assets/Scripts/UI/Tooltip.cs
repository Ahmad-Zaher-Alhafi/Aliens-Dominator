using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {
    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField] private string tipToShow;

        public void OnPointerEnter(PointerEventData eventData) {
            TooltipPanel.Instance.ShowTooltip(tipToShow);
        }

        public void OnPointerExit(PointerEventData eventData) {
            TooltipPanel.Instance.HideTooltip();
        }
    }
}