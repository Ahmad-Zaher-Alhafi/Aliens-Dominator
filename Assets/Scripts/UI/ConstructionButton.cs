using DG.Tweening;
using DoTweenAnimations;
using TMPro;
using UnityEngine;

namespace UI {
    [RequireComponent(typeof(FadeTextInOutAnimation))]
    public class ConstructionButton : AnimatedUIObject {
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private FadeTextInOutAnimation fadeTextInOutAnimation;
        [SerializeField] private TextShakeAnimation textShakeAnimation;

        private Sequence textShakeTween;

        public void SetText(string text, Color color = default, bool startFadeAnimation = false) {
            priceText.text = text;

            if (color != default) {
                priceText.color = new Color(color.r, color.g, color.b, priceText.color.a);
            }

            if (startFadeAnimation) {
                fadeTextInOutAnimation?.PlayFadeInOutAnimation();
            } else {
                fadeTextInOutAnimation?.StopFadeInOutAnimation();
            }
        }

        public override void PlayErrorAnimation() {
            base.PlayErrorAnimation();
            textShakeAnimation.PlayShakeAnimation();
        }
    }
}