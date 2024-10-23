using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utils.Extensions;

namespace UI {
    public class TooltipPanel : MonoBehaviour {
        public static TooltipPanel Instance;

        [SerializeField] private Canvas parentCanvas;
        [SerializeField] private GameObject tooltipBackground;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private Vector2 offset = new(0, 40);

        private RectTransform rectTransform;

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update() {
            if (!tooltipBackground.activeInHierarchy) return;

            if (!EventSystem.current.IsPointerOverGameObject()) {
                HideTooltip();
                return;
            }

            KeepTooltipInsideScreen();
        }

        public void ShowTooltip(string tooltipText) {
            tooltipBackground.SetActiveWithCheck(true);
            this.tooltipText.text = tooltipText;
        }

        private void KeepTooltipInsideScreen() {
            rectTransform.anchoredPosition = Mouse.current.position.ReadValue() + offset; // Set the tooltip to follow the mouse + offset

            // Get the canvas size and the tooltip size
            RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;
            Vector2 tooltipSize = rectTransform.sizeDelta;

            // Calculate the boundaries of the tooltip
            Vector2 minPosition = new Vector2(tooltipSize.x / 2, tooltipSize.y / 2);

            Vector2 maxPosition = new Vector2(canvasSize.x - tooltipSize.x / 2, canvasSize.y - tooltipSize.y / 2);

            // Clamp the position to keep the tooltip within the screen
            Vector2 clampedPosition = new Vector2(Mathf.Clamp(rectTransform.anchoredPosition.x, minPosition.x, maxPosition.x), Mathf.Clamp(rectTransform.anchoredPosition.y, minPosition.y, maxPosition.y));

            rectTransform.anchoredPosition = clampedPosition;
        }

        public void HideTooltip() {
            tooltipBackground.SetActiveWithCheck(false);
        }
    }
}