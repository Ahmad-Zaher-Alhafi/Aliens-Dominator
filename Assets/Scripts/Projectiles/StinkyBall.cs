using System.Collections;
using Context;
using Creatures;
using SecurityWeapons;
using UnityEngine;

namespace Projectiles {
    public class StinkyBall : MonoBehaviour {
        [SerializeField] private float damageCost;
        [SerializeField] private Constants.ObjectsColors stinkyBallColor;
        public float speed;
        [SerializeField] private Vector3 targetOffset;
        [SerializeField] private float smoothRotatingSpeed; //the speed of projectilr rotating 
        [SerializeField] private DestroyAfterTime tailParticles;
        [SerializeField] private DestroyAfterTime explosionParticles;
        private bool hasToFollowTarget;

        private Transform target;
        private Vector3 wantedAngle, oldAngle; //wanted angle is the angle that the creature has to rotate to it to reach the wanted point, old angle is the current angle

        public Constants.ObjectsColors StinkyBallColor => stinkyBallColor;

        private void Start() {
            StartCoroutine(DestroyAfterTime(15));
        }

        private void Update() {
            if (hasToFollowTarget) {
                RotateToTheWantedAngle(target);

                transform.position = Vector3.Lerp(transform.position, target.transform.position, speed * Time.deltaTime / Vector3.Distance(transform.position, target.transform.position)); //make the projectile moves
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag(Constants.PlayerLookAtPoint)) //if it hit the player
            {
                //Ctx.Deps.EventsManager.TriggerSpawnedOnNetwork(stinkyBallColor); //call the event to play the block vision particles
                DestroyParticles();
                Destroy(gameObject);
            } else if (other.CompareTag(Constants.SecurityWeapon)) {
                DestroyParticles();
                Destroy(gameObject);
            } else if (other.gameObject.layer != Constants.ENEMY_LAYER_ID && other.gameObject.layer != Constants.IGNORE_RAYCAST_LAYER_ID && !other.CompareTag(Constants.SecuritySensor)) //to prevent the stinky ball from being destroied if hit a creature(prevent selt destruction)
            {
                DestroyParticles();
                Destroy(gameObject);
            }
        }


        public void FollowTarget(Transform target) {
            StartCoroutine(DestroyAfterTime(15));
            //audioSource.PlayOneShot(rocketLaunch.audioClip, rocketLaunch.volume);
            this.target = target;
            hasToFollowTarget = true;
        }


        /// <summary>
        ///     To put the projectile in the right rotation
        /// </summary>
        /// <param name="objectToLookAt">the object that you want the projectile to look at while he is moving</param>
        private void RotateToTheWantedAngle(Transform objectToLookAt) {
            if (objectToLookAt == null) return;

            //I'm doing that as a trick to get the wanted angle and after that i'm resetting the angle to it's old angle and that because we need to rotates the projectile smoothly and not suddenly which make it cooler
            oldAngle = transform.eulerAngles; //save old angle
            wantedAngle = transform.eulerAngles; //get the wanted eural angle after he looked
            transform.eulerAngles = oldAngle; //reset the angle to the old angle 

            Quaternion newAngle = Quaternion.Euler(wantedAngle); //get the new angle from the wanted eural angle (needed for the next step)
            transform.rotation = Quaternion.Lerp(transform.rotation, newAngle, smoothRotatingSpeed); //rotate the projectile smoothly from old angle to the new one
        }

        /// <summary>
        ///     the particles inside the stinky ball(trail and explosion) needs time to geet destroied so we unparent them from the
        ///     stinky ball and destroy them after time when the particle effect finish
        /// </summary>
        private void DestroyParticles() {
            tailParticles.transform.parent = null;
            ParticleSystem.MainModule mainParticles = tailParticles.GetComponent<ParticleSystem>().main;
            tailParticles.OrderToDestroy(mainParticles.startLifetime.constant - 1);
            mainParticles.loop = false;
            explosionParticles.gameObject.SetActive(true);
            explosionParticles.transform.parent = null;
            explosionParticles.OrderToDestroy(explosionParticles.GetComponent<ParticleSystem>().main.startLifetime.constant);
        }

        private IEnumerator DestroyAfterTime(float secondsToDestroy) //to not let the stinky ball flying in the air for the rest of the game if it does not hit anything
        {
            yield return new WaitForSeconds(secondsToDestroy);
            if (gameObject != null) Destroy(gameObject);
        }
    }
}