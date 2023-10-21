using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Creatures {
    public interface IDiggingCreature { }

    public class DiggingCreature : Creature, IDiggingCreature {
        [SerializeField] private Transform rigParent;
        [SerializeField] private Vector3 digDownRotateAngle;
        [SerializeField] private Vector3 digUpRotateAngle;
        [SerializeField] private float digSpeed;
        [SerializeField] private ParticleSystem soilParticles;
        [SerializeField] private Vector3 digUpPositionOffset; //needed because we wanted the creature to be under the ground and moset of the waypints are not under the ground so we need this offset
        private float correctCreatureHeight; //the height that the creature should has it

        private bool hasToDigDown;
        private bool hasToDigUp;
        private bool hasToSetHeightPosition; //to make sure that the creature has set up on the ground correctly(because not all the pathPoints have the same height)
        private NavMeshAgent navMeshAgent; //the nav nesh agent changes the creature height to be on the ground so we need it to get the correct height
        private Transform pathPointToGoTo; //the pathPoint where the creature is gonna transport to

        private void Start() {
            hasToSetHeightPosition = false;
            navMeshAgent = GetComponent<NavMeshAgent>();
            hasToDigDown = false;
            hasToDigUp = false;
        }


        private void Update() {
            if (gameObject != null) {
                if (hasToDigDown) {
                    navMeshAgent.enabled = false;
                    RotateToWantedAngel(digDownRotateAngle);
                } else if (hasToDigUp) {
                    navMeshAgent.enabled = false;
                    RotateToWantedAngel(digUpRotateAngle);
                } else if (hasToSetHeightPosition) {
                    SetHeightPosition();
                }
            }
        }

        public void DigDown(Transform pathPointToGoTo) {
            if (pathPointToGoTo != null) {
                this.pathPointToGoTo = pathPointToGoTo;
                soilParticles.Play();
                hasToDigUp = false;
                hasToDigDown = true;
            }
        }

        private void DigUp() {
            transform.position = pathPointToGoTo.position + digUpPositionOffset;
            soilParticles.Play();
            hasToDigDown = false;
            hasToDigUp = true;
        }

        private void RotateToWantedAngel(Vector3 wntedAngle) {
            if (hasToDigDown) {
                if (rigParent.localEulerAngles.x < wntedAngle.x) {
                    transform.position += Vector3.down * digSpeed * Time.deltaTime;
                    rigParent.Rotate(Vector3.right * rotatingSpeed * Time.deltaTime);
                } else {
                    hasToDigDown = false;
                    soilParticles.Stop();
                    rigParent.localEulerAngles = Vector3.left * digDownRotateAngle.x;
                    DigUp();
                }
            } else if (hasToDigUp) {
                if (rigParent.localEulerAngles.x > wntedAngle.x) {
                    transform.position += Vector3.up * digSpeed * Time.deltaTime;
                    rigParent.Rotate(Vector3.right * rotatingSpeed * Time.deltaTime);
                } else {
                    rigParent.localEulerAngles = Vector3.zero;
                    hasToDigUp = false;
                    StartCoroutine(GetCorrectHeight());
                }
            }
        }

        private void SetHeightPosition() //adjest the creature height to the correctCreatureHeight
        {
            if (transform.position.y < correctCreatureHeight) {
                transform.position += Vector3.up * digSpeed * Time.deltaTime;
            } else {
                hasToSetHeightPosition = false;
                soilParticles.Stop();
            }
        }

        private IEnumerator GetCorrectHeight() {
            float oldHeight = transform.position.y;
            navMeshAgent.enabled = true;
            yield return new WaitForSeconds(0.1f); //needs time to let the creature adjest it's y axis after enabling the navAgent
            correctCreatureHeight = transform.position.y;
            navMeshAgent.enabled = false;
            transform.position = new Vector3(transform.position.x, oldHeight, transform.position.z);
            hasToSetHeightPosition = true;
        }
    }
}