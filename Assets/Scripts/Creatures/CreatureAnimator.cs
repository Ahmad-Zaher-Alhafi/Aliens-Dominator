using UnityEngine;

namespace Creatures {
    public class CreatureAnimator : MonoBehaviour {
        public string WalkAnimationParamter;

        private Creature creature;
        private Animator animator;
        private float initialSpeed;

        private void Awake() {
            animator = GetComponent<Animator>();
            initialSpeed = animator.speed;
        }

        public void Init() {
            if (gameObject.tag != "OnStartWaves") animator.SetBool(WalkAnimationParamter, true);
        }

        private void Update() {
            if (creature.CurrentState == creature.PreviousState) {
                return;
            }

            switch (creature.IsSlowedDown) {
                case true when animator.speed.Equals(initialSpeed):
                    animator.speed /= 2;
                    break;
                case false when !animator.speed.Equals(initialSpeed):
                    animator.speed = initialSpeed;
                    break;
            }

            if (creature.PreviousState == Creature.CreatureState.GettingHit) {
                animator.Play(Constants.GetAnimationName(gameObject.name, Constants.AnimationsTypes.TakeDamage));
            } else if (creature.PreviousState == Creature.CreatureState.Dead) {
                animator.enabled = !enabled;
            }

        }
    }
}