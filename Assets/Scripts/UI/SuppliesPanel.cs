using DG.Tweening;
using DoTweenAnimations;
using TMPro;
using UnityEngine;

namespace UI {
    [RequireComponent(typeof(TextShakeAnimation))]
    public class SuppliesPanel : AnimatedUIObject {
        [SerializeField] private TextMeshProUGUI suppliesAmountText;
        [SerializeField] private TextShakeAnimation textShakeAnimation;

        private Sequence textShakeTween;

        public void SetText(string text) {
            suppliesAmountText.text = text;
        }

        public override void PlayErrorAnimation() {
            base.PlayErrorAnimation();
            textShakeAnimation.PlayShakeAnimation();
        }
    }
}