using DG.Tweening;
using TMPro;
using UnityEngine;

namespace DoTweenAnimations {
    /// <summary>
    /// It will shake the text and change its color for a period of time to red (changing color is optional)
    /// </summary>
    public class TextShakeAnimation : MonoBehaviour {
        [SerializeField] private TMP_Text textToAnimate;
        [SerializeField] private float strength = 5f;
        [SerializeField] private float duration = .5f;

        private Sequence textShakeTween;


        /// <summary>
        ///
        /// </summary>
        /// <param name="changeColor">Set to ture to change color to red while shaking then back to previous color</param>
        public void PlayShakeAnimation(bool changeColor = true) {
            textShakeTween?.Complete();
            textShakeTween = DOTween.Sequence().Join(textToAnimate.transform.DOShakePosition(duration, strength));

            if (changeColor) {
                Color initialColor = textToAnimate.color;
                textShakeTween.Join(textToAnimate.DOColor(Colors.Instance.Error, duration))
                    .Append(textToAnimate.DOColor(initialColor, duration / 2));
            }

            textShakeTween
                .OnKill(() => textShakeTween = null)
                .Play();
        }

        private void OnDestroy() {
            textShakeTween?.Kill();
        }
    }
}