using System.Collections;
using System.Collections.Generic;
using Context;
using Creatures.Animators;
using Pool;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Creatures {
    public abstract class Creature : PooledObject {
        public enum CreatureType {
            Garoo,
            Longtail,
            Magantee,
            Magantis,
            Scorpion,
            Serpent,
            Sicklus,
            Telekinis,
            Ulifo
        }

        public enum CreatureState {
            None,
            Idle,
            Patrolling,
            FollowingPath,
            ChasingTarget,
            GettingHit,
            Attacking,
            Chasing,
            RunningAway,
            Dead
        }

        private enum CreatureAction {
            None,
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
        public CreatureMover Mover { get; private set; }

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

        [SerializeField] private int specialActionPathPointIndex = 2;
        [SerializeField] private float secondsToDestroyDeadBody = 10;

        private int initialHealth;
        private AudioSource audioSource;
        private Rigidbody rig;
        private CreatureAnimator animator;
        private BodyPart[] bodyParts;
        /// <summary>
        /// A state that is being set by external event which overrides the current state
        /// </summary>
        private CreatureState highPriorityState = CreatureState.None;
        public GameObject ObjectToAttack {
            get;
            private set;
        }

        public AttackPoint AttackPoint {
            get;
            private set;
        }

        private void Awake() {
            rig = GetComponent<Rigidbody>();
            Mover = GetComponent<CreatureMover>();
            animator = GetComponent<CreatureAnimator>();
            bodyParts = GetComponentsInChildren<BodyPart>();
            audioSource = GetComponent<AudioSource>();

            Ctx.Deps.EventsManager.WaveStarted += RunAway;
        }

        public void Init(Vector3 spawnPosition, SpawnPointPath pathToFollow, AttackPoint attackPoint, CreatureState initialState) {
            CurrentState = CreatureState.None;
            highPriorityState = initialState;
            initialHealth = health;
            transform.position = spawnPosition;
            IsSlowedDown = false;

            if (attackPoint is not null) {
                AttackPoint = attackPoint;
                ObjectToAttack = AttackPoint.TargetObject;
            }
            
            rig.useGravity = false;
            rig.collisionDetectionMode = CollisionDetectionMode.Discrete;

            CreatureHealthBar.minValue = 0;
            CreatureStateCanves.SetActive(false);

            animator.Init();
            Mover.Init(pathToFollow);
            foreach (BodyPart bodyPart in bodyParts) {
                bodyPart.Init(bouncingMaterial);
            }

            gameObject.SetActive(true);
        }

        private void Update() {
            if (CurrentState is CreatureState.Dead) return;

            if (highPriorityState is not CreatureState.None) {
                if (CurrentState != highPriorityState) {
                    PreviousState = CurrentState;
                    CurrentState = highPriorityState;
                }
                return;
            }

            if (Mover.IsBusy) return;

            if (Mover.HasReachedAttackPoint) {
                if (CurrentState is CreatureState.ChasingTarget) {
                    Attack(ObjectToAttack);
                    return;
                }

                CurrentState = CreatureState.ChasingTarget;
                return;
            }

            PreviousState = CurrentState;
            CurrentState = GetRandomActionToDo() switch {
                CreatureAction.StayIdle => CreatureState.Idle,
                CreatureAction.Patrol => CreatureState.Patrolling,
                _ => CurrentState
            };
        }

        private CreatureAction GetRandomActionToDo() {
            int randomNumber = Random.Range(0, Ctx.Deps.CreatureSpawnController.HasWaveStarted ? 3 : 2);
            return randomNumber switch {
                0 => CreatureAction.StayIdle,
                1 => CreatureAction.Patrol,
                _ => CreatureAction.None
            };
        }

        public void OnOrderFulfilled() {
            if (CurrentState == CreatureState.RunningAway) {
                Disappear();
            }
            highPriorityState = CreatureState.None;
        }

        public void Attack(GameObject objectToAttack) {
            CurrentState = CreatureState.Attacking;
        }

        public void TakeDamage(IDamager damager, int damageWeight) {
            if (CurrentState == CreatureState.Dead) return;

            if (!CreatureStateCanves.activeInHierarchy) {
                CreatureStateCanves.SetActive(true);
            }

            int totalDamage = damager.Damage * damageWeight;
            health -= totalDamage;

            PreviousState = CreatureState.GettingHit;
            CreatureHealthBar.normalizedValue = health / initialHealth;

            if (health > 0f) return;

            Ctx.Deps.SupplyBalloonController.SpawnBalloon(transform.position, chanceOfDroppingBalloon);

            // Force to push the creature away once get killed (More realistic)
            rig.AddForce(damager.Transform.forward * pushForceWhenDead);

            Die();
        }

        private void RunAway() {
            Mover.FulfillCurrentOrder();
            highPriorityState = CreatureState.RunningAway;
        }

        private void PlayDeathSound() {
            audioSource.Play();
        }

        private void Die() {
            CurrentState = CreatureState.Dead;
            PlayDeathSound();

            rig.useGravity = true;
            rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            health = initialHealth;

            Mover.OnDeath();
            animator.OnDeath();
            foreach (BodyPart bodyPart in bodyParts) {
                bodyPart.OnDeath();
            }

            StartCoroutine(DestroyObjectDelayed(secondsToDestroyDeadBody));
        }

        private IEnumerator DestroyObjectDelayed(float secondsToDestroyDeadBody = 0) {
            Ctx.Deps.CreatureSpawnController.OnCreatureDeath(this);
            yield return new WaitForSeconds(secondsToDestroyDeadBody);
            ReturnToPool();
        }

        private void Disappear() {
            CurrentState = CreatureState.Dead;
            StartCoroutine(DestroyObjectDelayed());
        }

        private void OnDestroy() {
            Ctx.Deps.EventsManager.WaveStarted -= RunAway;
        }
    }
}