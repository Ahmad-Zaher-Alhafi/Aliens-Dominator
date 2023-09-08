using System.Collections;
using Context;
using Creatures;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public class SpawningState : CreatureState {
        public override CreatureStateType Type => CreatureStateType.Spawning;
        public override bool CanBeActivated() => AutomatedObject.IsStateActive<NoneState>();
        public override float? Speed => 0;
        protected override bool WaitForMoverToFulfill => false;
        protected override bool WaitForAnimatorToFulfill => AutomatedObject.HasSpawningAnimation;

        public SpawningState(Creature creature) : base(creature) { }

        public override void Activate(bool isSecondaryState = false) {
            base.Activate(isSecondaryState);
            if (AutomatedObject.HasSpawningAnimation) {
                AutomatedObject.Animator.PlaySpawnAnimation(OnAnimationFinished);
            } else {
                Fulfil();
            }

            Ctx.Deps.GameController.StartCoroutine(ShowSkinDelayed());
        }

        public override void Fulfil() {
            base.Fulfil();
            // Needed to be called by spawn animation, as when Magantee spawns from the mouth of Magantis the mover should be initialized after spawn animation finished
            AutomatedObject.Mover.Init(null);
        }

        /// <summary>
        /// Creatures that has spawning animation might requires to hide their skin on awake and show it after a short time, like creature magantee for example
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShowSkinDelayed() {
            yield return new WaitForEndOfFrame();
            AutomatedObject.SkinnedMeshRenderer.enabled = true;
        }
    }
}