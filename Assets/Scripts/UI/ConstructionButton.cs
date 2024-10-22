using DG.Tweening;
using DoTweenAnimations;
using TMPro;
using UnityEngine;

namespace UI {
    [RequireComponent(typeof(FadeTextInOutAnimation))]
    public class ConstructionButton : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private FadeTextInOutAnimation fadeTextInOutAnimation;

        private Sequence textShakeTween;

        private void Start() {
            fadeTextInOutAnimation = GetComponent<FadeTextInOutAnimation>();
        }

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

        public void PlayErrorAnimation() {
            textShakeTween?.Complete();
            textShakeTween = DOTween.Sequence()
                .Join(priceText.transform.DOShakePosition(.5f, 5))
                .Join(priceText.DOColor(Colors.Instance.Error, .5f))
                .Append(priceText.DOColor(Colors.Instance.Normal, .25f))
                .Play();
        }

        private void OnDestroy() {
            textShakeTween.Kill();
            textShakeTween = null;
        }
    }
}