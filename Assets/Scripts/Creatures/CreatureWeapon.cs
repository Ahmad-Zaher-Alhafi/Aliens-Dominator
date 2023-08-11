using FMODUnity;
using UnityEngine;

namespace Creatures {
    public class CreatureWeapon : MonoBehaviour {
        [SerializeField] private Creature creature; //owner of the gun
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform bulletCreatPoint; //position where the bullet is gonna be created
        [SerializeField] private float smoothWeaponRotatingSpeed = .1f;
        [SerializeField] private StudioEventEmitter enemyBulletSound;
        private bool hasToLookAtTheTarget; //true if gun has to look towards the target (the player)
        private bool hasToShoot; //true if has to shoot
        private GroundCreatureMover groundCreatureMover;
        private Transform playerShootAtPoint; //point where the gun is gonna shoot at

        private void Start() {
            hasToShoot = false;
            hasToLookAtTheTarget = false;

            //Had to implement the NPCSimplePatrol script, otherwise it wouldnt stop shooting. Don't haunt me down, I just felt desperate and exhausted about this bug.
            //groundCreatureMover = creature.GetComponent<GroundCreatureMover>();
        }

        private void Update() {
            if (hasToLookAtTheTarget) //while still has to rotate to the player angle
                LookAtTheTarget(playerShootAtPoint); //rotate to the player angle
        }

        private void LookAtTheTarget(Transform target) //to make the Gun look toward the target (the player for now)
        {
            Vector3 oldAngle = transform.localEulerAngles;
            transform.LookAt(target);
            float targetZAngle = transform.rotation.z;
            transform.localEulerAngles = oldAngle;
            var targetAngle = new Quaternion(transform.rotation.x, transform.rotation.y, targetZAngle, transform.rotation.w);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, smoothWeaponRotatingSpeed); //rotate the Gun to the target angle

            if (Quaternion.Angle(transform.rotation, targetAngle) <= .2) //if the Gun finished rotating to the correct angle wihch is the player angle
            {
                hasToLookAtTheTarget = false; //stop the rotating
                hasToShoot = true; //allow to shoot
            }
        }

        public void PrepareToShoot(Transform target) //Order to start using Gun
        {
            hasToLookAtTheTarget = true; //the the creature rotating
            playerShootAtPoint = target; //give the target
        }

        public void StopShooting() //to stop the shooting
        {
            hasToLookAtTheTarget = false;
            hasToShoot = false; //prevent the shooting
        }
    }
}