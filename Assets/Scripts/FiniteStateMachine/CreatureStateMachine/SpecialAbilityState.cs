using Context;
using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class SpecialAbilityState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.SpecialAbility;

        public override bool CanBeActivated() {
            if (AutomatedObject.CouldActivateSpecialAbility && HasChanceForActivation() && newPathPointReached) return true;
            newPathPointReached = false;
            return false;
        }

        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => true;

        /// <summary>
        /// This will be true only when the automated object reaches a new path point
        /// It will be false directly if the creature can not do special ability or when this state get activated
        /// As i do not want this state to be activated when the automated object is not on a path point and it shall be activated only once per path point if can be activated
        /// </summary>
        private bool newPathPointReached;

        public SpecialAbilityState(Creature creature) : base(creature) {
            Ctx.Deps.EventsManager.PathPointReached += OnPathPointReached;
        }
        private void OnPathPointReached(Creature creature, PathPoint pathPoint) {
            if (creature != AutomatedObject) return;
            newPathPointReached = true;
        }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            newPathPointReached = false;
            AutomatedObject.ExecuteSpecialAbility(OnAnimationFinished);
        }

        /// <summary>
        /// True if the random generated number is less or equal to the SpecialAbilityChance of the creature
        /// </summary>
        /// <returns></returns>
        private bool HasChanceForActivation() => Random.Range(0, 101) < AutomatedObject.SpecialAbilityChance;

        public override void Clear() {
            base.Clear();
            Ctx.Deps.EventsManager.PathPointReached -= OnPathPointReached;
        }
    }
}