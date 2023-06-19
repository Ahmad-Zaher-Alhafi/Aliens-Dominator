using Creatures;
using UnityEngine;
using Utils;

namespace SecurityWeapons {
    public class AirWeaponSensor : WeaponSensor {
        private void OnTriggerEnter(Collider other) {
            

            if (other.gameObject.layer != Constants.ENEMY_LAYER_ID) return;

            Creature creature = other.GetComponentInParent<Creature>();
            if (creature is not FlyingCreature) return;

            BodyPart bodyPart = MathUtils.GetRandomObjectFromList(creature.BodyParts);
            if (targets.Contains(bodyPart)) return;

            targets.Add(bodyPart);
        }

        private void OnTriggerExit(Collider other) {

            if (other.gameObject.layer != Constants.ENEMY_LAYER_ID) return;

            BodyPart bodyPart = other.GetComponent<BodyPart>();
            if (bodyPart == null) return;

            if (!targets.Contains(bodyPart)) return;

            targets.Remove(bodyPart);
            if (bodyPart == (BodyPart) TargetToAimAt) {
                TargetToAimAt = null;
            }
        }
    }
}