using System.Collections;
using System.Collections.Generic;
using Creatures.Animators;
using Pool;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Creatures {
    public abstract class Creature : PooledObject {
        public enum CreatureType {
            Grounded,
            Flying
        }

        public enum CreatureState {
            Idle,
            Patrolling,
            FollowingPath,
            GettingHit,
            Attacking,
            Chasing,
            Dead
        }

        public enum CreatureAction {
            StayIdle,
            Patrol,
            FollowPath,
            GetHit,
            Attack,
            Chase,
            Die
        }

        public CreatureState CurrentState { get; private set; }
        public CreatureState PreviousState { get; private set; }
        public bool IsSlowedDown { get; private set; }

        [SerializeField] private float pushForceWhenDead = 1000;
        [SerializeField] private List<Material> colors;
        [SerializeField] private PhysicMaterial bouncingMaterial;

        [SerializeField] private float slowdownTimer = 6f;
        [SerializeField] private float attackDamage = 10f;
        protected Constants.ObjectsColors CreatureColor => creatureColor;
        [SerializeField]
        private Constants.ObjectsColors creatureColor;

        [SerializeField] private GameObject CreatureStateCanves;
        [SerializeField] private Slider CreatureHealthBar;

        [Range(1, 5)]
        [SerializeField] private int health = 5;
        [Range(1, 100)]
        [SerializeField] private int chanceOfDroppingBalloon;

        [SerializeField] private int waypointMinIndexToActivateSpecialActions = 2; // We dont want the enemies to start shooting from the spawn waypoint
        [SerializeField] private float secondsToDestroyDeadBody = 10; //time needed before destroying the dead body of the creature

        private int initialHealth;
        private AudioSource audioSource;
        private Rigidbody rig;
        public CreatureMover mover;
        private CreatureAnimator animator;
        private BodyPart[] bodyParts;

        private void Awake() {
            rig = GetComponent<Rigidbody>();
            mover = GetComponent<CreatureMover>();
            animator = GetComponent<CreatureAnimator>();
            bodyParts = GetComponentsInChildren<BodyPart>();
            audioSource = GetComponent<AudioSource>();
        }
        
        public void Init(Vector3 position) {
            CurrentState = CreatureState.Idle;
            initialHealth = health;
            transform.position = position;
            IsSlowedDown = false;


            rig.useGravity = false;
            rig.collisionDetectionMode = CollisionDetectionMode.Discrete;

            CreatureHealthBar.minValue = 0;
            CreatureStateCanves.SetActive(false);

            animator.Init();
            mover.Init();
            foreach (BodyPart bodyPart in bodyParts) {
                bodyPart.Init(bouncingMaterial);
            }

            gameObject.SetActive(true);
        }

        private void Update() {
            if (mover.IsBusy || CurrentState == CreatureState.Dead) return;

            PreviousState = CurrentState;
            CurrentState = GetRandomActionToDo() switch {
                CreatureAction.StayIdle => CreatureState.Idle,
                CreatureAction.Patrol => CreatureState.Patrolling,
                _ => CurrentState
            };
        }
        
        private CreatureAction GetRandomActionToDo() {
            int randomNumber = Random.Range(0, 2);
            return randomNumber == 0 ? CreatureAction.StayIdle : CreatureAction.Patrol;
        }
        
        public void ApplyDamage(IDamager damager, int damageWeight) {
            if (CurrentState == CreatureState.Dead) return;

            if (!CreatureStateCanves.activeInHierarchy) {
                CreatureStateCanves.SetActive(true);
            }
            
            int totalDamage = damager.Damage * damageWeight;
            health -= totalDamage;
            
            PreviousState = CreatureState.GettingHit;
            CreatureHealthBar.normalizedValue = health / initialHealth;

            if (health > 0f) return;

            int rand = Random.Range(1, 101);
            if (rand <= chanceOfDroppingBalloon) {
                SpawnBalloon();
            }

            // Force to push the creature away once get killed (More realistic)
            rig.AddForce(damager.Transform.forward * pushForceWhenDead);

            OnDie();
        }

        private void SpawnBalloon() { }

        private void PlayDeathSound() {
            audioSource.Play();
        }
        
        private void OnDie() {
            CurrentState = CreatureState.Dead;
            PlayDeathSound();

            rig.useGravity = true;
            rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            mover.OnDie();
            animator.OnDie();
            foreach (BodyPart bodyPart in bodyParts) {
                bodyPart.OnDie();
            }

            StartCoroutine(DestroyObjectDelayed());
        }

        private IEnumerator DestroyObjectDelayed() {
            yield return new WaitForSeconds(secondsToDestroyDeadBody);
            ReturnToPool();
        }
    }
}