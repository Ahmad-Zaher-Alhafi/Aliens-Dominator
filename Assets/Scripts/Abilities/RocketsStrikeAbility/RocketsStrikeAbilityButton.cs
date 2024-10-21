using Context;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Abilities.RocketsStrikeAbility {
    public class RocketsStrikeAbilityButton : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private float dimmingTime = .5f;

        private Sequence colorDimmingTween;

        private void Update() {
            labelText.text = Ctx.Deps.AbilitiesController.CanAbilityBeUsed<RocketsStrikeAbility>() ? "Ready" : Ctx.Deps.AbilitiesController.GetAbilityTimeLeftToBeReady<RocketsStrikeAbility>().ToString(@"mm\:ss");
            if (Ctx.Deps.AbilitiesController.CanAbilityBeUsed<RocketsStrikeAbility>()) {
                PlayColorDimmingAnimation();
            } else {
                colorDimmingTween?.Kill();
            }
        }

        private void PlayColorDimmingAnimation() {
            if (labelText == null) return;
            if (colorDimmingTween?.IsPlaying() == true) return;

            // Get the current material color
            float initialAlpha = labelText.alpha;

            // Create a sequence for the fade in/out effect
            colorDimmingTween = DOTween.Sequence()
                .Append(labelText.DOFade(0, dimmingTime)) // Fade out
                .Append(labelText.DOFade(initialAlpha, dimmingTime)) // Fade back in
                .SetLoops(-1) // Loop infinitely
                .OnKill(() => labelText.alpha = initialAlpha)
                .Play();
        }

        private void OnDestroy() {
            colorDimmingTween?.Kill();
            colorDimmingTween = null;
        }
    }
}