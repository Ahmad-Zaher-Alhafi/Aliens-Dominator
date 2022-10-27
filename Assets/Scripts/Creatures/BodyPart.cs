using UnityEngine;

namespace Creatures {
    public class BodyPart : MonoBehaviour {
        public enum CreatureBodyPart { //Stores the body tags in enum, so its easier to setup in inspector
            Head,
            Body,
            Leg,
            Arm,
            Tail
        }

        [Range(0f, 5f)]
        public float Weight = 1f; //1 = full damage of the arrow gets applied
        public BodyPart Type; //The tag where the weight gets applied
    }
}