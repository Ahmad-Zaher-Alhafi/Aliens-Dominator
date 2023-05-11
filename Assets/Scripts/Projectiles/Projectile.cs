using System;
using System.Collections;
using Creatures;
using FiniteStateMachine.States;
using UnityEngine;

namespace Projectiles {
    public class Projectile : MonoBehaviour {
        public int DamageCost;
        [HideInInspector]
        public bool WasShoot;
        [HideInInspector]
        public bool IsUsed;
        [SerializeField] private AudioSource audioSource;
        public float speed;
        [SerializeField] private float smoothRotatingSpeed; //the speed of projectilr rotating 
        [SerializeField] private Vector3 targetOffset;
        [Header("Only for rockets")]
        [SerializeField] private bool isItRocket;
        [SerializeField] private ParticleSystem rocketSmokeParticle;
        [SerializeField] private Sound rocketLaunch;
        [SerializeField] private Sound rocketExplosion;
        [SerializeField] private MeshRenderer rocketMeshRenderer;
        private bool hasToFollowTarget;
        private CapsuleCollider rocketCapsuleCollider;

        private Creatures.Creature target;
        private Vector3 wantedAngle, oldAngle; //wanted angle is the angle that the creature has to rotate to it to reach the wanted point, old angle is the current angle

        private void Start() {
            rocketCapsuleCollider = GetComponent<CapsuleCollider>();
            IsUsed = false;
        }

        private void Update() {
            if (hasToFollowTarget) {
                transform.position += transform.forward * speed * Time.deltaTime; //make the projectile moves forward
                RotateToTheWantedAngle(target);
                smoothRotatingSpeed += Time.deltaTime * 4;
                speed += Time.deltaTime * 2;
            }
        }

        private void OnTriggerEnter(Collider other) {

            if (!WasShoot) return;

            if (!other.CompareTag(Constants.SecuritySensor) && other.gameObject.layer != Constants.IGNORE_RAYCAST_LAYER_ID && !other.CompareTag(Constants.Arrow)) //if it hit any object except the the security sensor collider or the creature internal body colliders
            {
                if (isItRocket) {
                    if (rocketSmokeParticle != null) rocketSmokeParticle.transform.parent = null;

                    rocketCapsuleCollider.enabled = false;
                    rocketMeshRenderer.enabled = false;
                    rocketSmokeParticle.GetComponent<DestroyAfterTime>().OrderToDestroy(rocketSmokeParticle.GetComponent<ParticleSystem>().main.startLifetime.constant);
                    audioSource.PlayOneShot(rocketExplosion.audioClip, rocketExplosion.volume);
                    StartCoroutine(DestroyAfterTime(3));
                } else {
                    Destroy(gameObject);
                }
            }
        }

        private IEnumerator DestroyAfterTime(float secondsToDestroy) //to not let the projectile flying in the air for the rest of the game if it does not hit anything
        {
            yield return new WaitForSeconds(secondsToDestroy);
            if (gameObject != null) Destroy(gameObject);
        }

        public void FollowTarget(Creatures.Creature target) {
            WasShoot = true;
            StartCoroutine(DestroyAfterTime(15));
            audioSource.PlayOneShot(rocketLaunch.audioClip, rocketLaunch.volume);
            hasToFollowTarget = true;
            this.target = target;
            if (isItRocket) rocketSmokeParticle.Play();
        }

        /// <summary>
        ///     To put the projectile in the right rotation
        /// </summary>
        /// <param name="objectToLookAt">the object that you want the projectile to look at while he is moving</param>
        private void RotateToTheWantedAngle(Creatures.Creature objectToLookAt) {
            if (objectToLookAt == null || objectToLookAt.CurrentState == StateType.Dead) return;

            //I'm doing that as a trick to get the wanted angle and after that i'm resetting the angle to it's old angle and that because we need to rotates the projectile smoothly and not suddenly which make it cooler
            oldAngle = transform.eulerAngles; //save old angle

            if (objectToLookAt is FlyingCreature) // if it was air creature then we need to douple the offset because of the animation problem
                transform.LookAt(objectToLookAt.transform.position + targetOffset * 2); //look at the target
            else transform.LookAt(objectToLookAt.transform.position + targetOffset); //look at the target
            wantedAngle = transform.eulerAngles; //get the wanted eural angle after he looked
            transform.eulerAngles = oldAngle; //reset the angle to the old angle 

            Quaternion newAngle = Quaternion.Euler(wantedAngle); //get the new angle from the wanted eural angle (needed for the next step)
            transform.rotation = Quaternion.Lerp(transform.rotation, newAngle, smoothRotatingSpeed * Time.deltaTime); //rotate the projectile smoothly from old angle to the new one
        }

        [Serializable]
        private class Sound {
            public AudioClip audioClip;
            public float volume;
        }
    }
}