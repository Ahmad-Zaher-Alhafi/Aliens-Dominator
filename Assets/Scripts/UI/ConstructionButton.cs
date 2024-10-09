using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI {
    public class ConstructionButton : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI priceText;

        private Sequence textShakeTween;

        public void SetText(string text) {
            priceText.text = text;
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