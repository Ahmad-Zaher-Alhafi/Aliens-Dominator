using System;
using System.Collections;
using System.Collections.Generic;
using Context;
using Creatures.Animators;
using FiniteStateMachine;
using Pool;
using FiniteStateMachine.CreatureStateMachine;
using UnityEngine;

namespace Creatures {
    public abstract class Creature : PooledObject, IDamager, IDamageable, IAutomatable {
        public bool HasPushingForce => false;
        public bool IsDestroyed => IsDead;
        public int Damage => attackDamage;
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;
        public Enum CurrentStateType => creatureStateMachine.PrimaryState.Type;
        public bool IsSlowedDown { get; private set; }
        public CreatureMover Mover { get; private set; }
        public GameObject ObjectToAttack { get; private set; }
        public TargetPoint TargetPoint { get; private set; }
        public bool HasToDisappear { get; set; }

        [SerializeField] private int pushForceWhenDead = 1000;
        public int PushForceWhenDead => pushForceWhenDead;

        [SerializeField] private List<Material> colors;
        [SerializeField] private PhysicMaterial bouncingMaterial;

        [SerializeField] private int slowdownTimer = 6;
        [SerializeField] private int attackDamage = 10;
        protected Constants.ObjectsColors CreatureColor => creatureColor;
        [SerializeField]
        private Constants.ObjectsColors creatureColor;

        //[SerializeField] private GameObject CreatureStateCanves;
        //[SerializeField] private Slider CreatureHealthBar;

        [Range(1, 5)]
        [SerializeField] private int health = 5;
        public int Health {
            get => health;
            set => health = value;
        }

        public IDamager ObjectDamagedWith { get; private set; }

        [Range(1, 100)]
        [SerializeField] private int chanceOfDroppingBalloon;
        public int ChanceOfDroppingBalloon => chanceOfDroppingBalloon;

        [SerializeField] private int specialActionPathPointIndex = 2;
        [SerializeField] private int secondsToDestroyDeadBody = 10;
        public int SecondsToDestroyDeadBody => secondsToDestroyDeadBody;

        public int InitialHealth { get; set; }
        private AudioSource audioSource;
        public Rigidbody Rig { get; private set; }
        public CreatureAnimator Animator { get; private set; }
        public IReadOnlyList<BodyPart> BodyParts { get; private set; }
        public bool HasToFollowPath { get; set; }

        public bool IsCinematic { get; private set; }
        public bool IsPoisoned { get; private set; }
        public bool TargetReached { get; set; }
        public bool IsDead { get; set; }
        public bool PathFinished { get; private set; }
        private CreatureStateMachine creatureStateMachine;

        private void Awake() {
            creatureStateMachine = GetComponent<CreatureStateMachine>();
            Rig = GetComponent<Rigidbody>();
            Mover = GetComponent<CreatureMover>();
            Animator = GetComponent<CreatureAnimator>();
            BodyParts = GetComponentsInChildren<BodyPart>();
            audioSource = GetComponent<AudioSource>();

            InitialHealth = health;
        }

        public void Init(Vector3 spawnPosition, SpawnPointPath pathToFollow, bool isCinematic, TargetPoint targetPoint, CreatureStateType initialCreatureState) {
            Rig.isKinematic = false;
            transform.position = spawnPosition;
            IsSlowedDown = false;
            IsDead = false;
            IsCinematic = isCinematic;
            HasToFollowPath = pathToFollow != null;
            HasToDisappear = false;
            TargetReached = false;

            if (targetPoint is not null) {
                TargetPoint = targetPoint;
                ObjectToAttack = TargetPoint.TargetObject;
            }

            Rig.collisionDetectionMode = CollisionDetectionMode.Discrete;

            //CreatureHealthBar.minValue = 0;
            //CreatureStateCanves.SetActive(false);

            Animator.Init();
            Mover.Init(pathToFollow);

            gameObject.SetActive(true);

            foreach (BodyPart bodyPart in BodyParts) {
                bodyPart.Init(bouncingMaterial);
            }

            creatureStateMachine.Init(this, initialCreatureState);
        }

        public void OnMoverOrderFulfilled() {
            if (IsCinematic) {
                creatureStateMachine.SetNextCinematicState();
            }
        }

        public void TakeDamage(IDamager damager, int damageWeight) {
            ObjectDamagedWith = damager;
            creatureStateMachine.GetState<GettingHitState>().GotHit(ObjectDamagedWith, damageWeight);

            /*if (!CreatureStateCanves.activeInHierarchy) {
                CreatureStateCanves.SetActive(true);
            }*/

            //CreatureHealthBar.normalizedValue = health / InitialHealth;
        }

        public void PlayDeathSound() {
            audioSource.Play();
        }

        public void OnDeath() {
            Mover.OnDeath();
            Animator.OnDeath();
            Rig.isKinematic = true;
            foreach (BodyPart bodyPart in BodyParts) {
                bodyPart.OnDeath();
            }

            Ctx.Deps.GameController.StartCoroutine(DestroyObjectDelayed(SecondsToDestroyDeadBody));
        }

        private IEnumerator DestroyObjectDelayed(float secondsToDestroyDeadBody = 0) {
            Ctx.Deps.CreatureSpawnController.OnCreatureDeath(this);
            yield return new WaitForSeconds(secondsToDestroyDeadBody);
            ReturnToPool();
            Debug.Log($"Creature {this} disappeared");
        }

        public void Disappear() {
            Ctx.Deps.GameController.StartCoroutine(DestroyObjectDelayed());
        }
    }
}