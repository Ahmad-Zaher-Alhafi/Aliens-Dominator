using UnityEngine;

namespace EditorHelpers {
    [ExecuteInEditMode]
    public class BordPartsAdder : MonoBehaviour {

        /*private void OnEnable() {
            List<Creature> creatures = FindObjectsOfType<Creature>().ToList();
            foreach (Creature creature in creatures) {
                List<Collider> parts = creature.GetComponentsInChildren<Collider>().ToList();
                foreach (Collider part in parts) {
                    BodyPart bodyPart = null;

                    bodyPart = part.gameObject.TryGetComponent(out BodyPart bodyPartComponent) 
                        ? bodyPartComponent 
                        : part.gameObject.AddComponent<BodyPart>();
                
                    bodyPart.creature = creature;

                    if (part.gameObject.name.Contains("Head") || part.gameObject.name.Contains("Neck")) {
                        bodyPart.damageWeight = 5;
                        bodyPart.type = BodyPart.CreatureBodyPart.Head;
                    } else if (part.gameObject.name.Contains("Pelvis") || part.gameObject.name.Contains("Spine") ||
                               part.gameObject.name.Contains("RigMain") || part.gameObject.name.Contains("RigRibcage")) {
                        bodyPart.damageWeight = 4;
                        bodyPart.type = BodyPart.CreatureBodyPart.Body;
                    } else if (part.gameObject.name.Contains("Arm")) {
                        bodyPart.damageWeight = 3;
                        bodyPart.type = BodyPart.CreatureBodyPart.Arm;
                    } else if (part.gameObject.name.Contains("Leg")) {
                        bodyPart.type = BodyPart.CreatureBodyPart.Leg;
                        bodyPart.damageWeight = 2;
                    } else if (part.gameObject.name.Contains("Foot") || part.gameObject.name.Contains("Claw")) {
                        bodyPart.type = BodyPart.CreatureBodyPart.Foot;
                        bodyPart.damageWeight = 1;
                    } else if (part.gameObject.name.Contains("Tail")) {
                        bodyPart.type = BodyPart.CreatureBodyPart.Tail;
                        bodyPart.damageWeight = 1;
                    }
                }
            }
        }*/
    }
}