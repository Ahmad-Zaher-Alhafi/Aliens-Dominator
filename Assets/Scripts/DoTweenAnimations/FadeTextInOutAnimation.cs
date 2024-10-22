using DG.Tweening;
using TMPro;
using UnityEngine;

namespace DoTweenAnimations {
    public class FadeTextInOutAnimation : MonoBehaviour {
        [SerializeField] private float fadeInOutTime = .5f;
        [SerializeField] private TMP_Text textToAnimate;

        private Sequence fadeInOutTween;


        public void PlayFadeInOutAnimation() {
            if (textToAnimate == null) return;
            if (fadeInOutTween?.IsPlaying() == true) return;

            // Get the current material color
            float initialAlpha = textToAnimate.alpha;

            // Create a sequence for the fade in/out effect
            fadeInOutTween = DOTween.Sequence()
                .Append(textToAnimate.DOFade(0, fadeInOutTime)) // Fade out
                .Append(textToAnimate.DOFade(initialAlpha, fadeInOutTime)) // Fade back in
                .SetLoops(-1) // Loop infinitely
                .OnKill(() => {
                    textToAnimate.alpha = initialAlpha;
                    fadeInOutTween = null;
                })
                .Play();
        }

        public void StopFadeInOutAnimation() {
            if (fadeInOutTween?.IsPlaying() != true) return;
            fadeInOutTween.Kill();
        }

        private void OnDestroy() {
            fadeInOutTween?.Kill();
        }
    }
}