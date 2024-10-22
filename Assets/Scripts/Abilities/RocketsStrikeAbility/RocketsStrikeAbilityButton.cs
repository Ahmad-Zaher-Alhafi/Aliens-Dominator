using Context;
using DoTweenAnimations;
using TMPro;
using UnityEngine;

namespace Abilities.RocketsStrikeAbility {
    public class RocketsStrikeAbilityButton : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private float dimmingTime = .5f;
        [SerializeField] private FadeTextInOutAnimation fadeTextInOutAnimation;

        private void Update() {
            labelText.text = Ctx.Deps.AbilitiesController.CanAbilityBeUsed<RocketsStrikeAbility>() ? "Ready" : Ctx.Deps.AbilitiesController.GetAbilityTimeLeftToBeReady<RocketsStrikeAbility>().ToString(@"mm\:ss");
            if (Ctx.Deps.AbilitiesController.CanAbilityBeUsed<RocketsStrikeAbility>()) {
                fadeTextInOutAnimation.PlayFadeInOutAnimation();
            } else {
                fadeTextInOutAnimation.StopFadeInOutAnimation();
            }
        }
    }
}