using System;
using System.Collections.Generic;
using System.Linq;
using Abilities;
using UnityEngine;

namespace ManagersAndControllers {
    public class AbilitiesController : MonoBehaviour {
        [SerializeField] private List<Ability> rocketsStrikeAbility;

        public bool CanAbilityBeUsed<TAbility>() where TAbility : Ability {
            return rocketsStrikeAbility.OfType<TAbility>().Single().ReadyToBeUsed;
        }

        public TimeSpan GetAbilityTimeLeftToBeReady<TAbility>() where TAbility : Ability {
            return rocketsStrikeAbility.OfType<TAbility>().Single().TimeLeftToBeReady;
        }

        public void UseAbility<TAbility>() where TAbility : Ability {
            rocketsStrikeAbility.OfType<TAbility>().Single().StartUsage();
        }
    }
}