using Arrows;
using UnityEngine;

namespace Creatures {
    public class BodyPart : MonoBehaviour {
        public enum CreatureBodyPart {
            Head,
            Body,
            Leg,
            Arm,
            Foot
        }

        [Range(1f, 5f)]
        [SerializeField] int damageWeight = 1;
        [SerializeField] CreatureBodyPart type;
        [SerializeField] private Creature creature;

        private void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.layer == Constants.Arrow_LAYER_ID) {
                creature.GetHurt(collision.gameObject.GetComponent<ArrowBase>(), damageWeight);
            }
        }
    }
}