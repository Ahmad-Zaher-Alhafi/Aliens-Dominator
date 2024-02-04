using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Context;
using Creatures.Animators;
using FiniteStateMachine;
using FiniteStateMachine.CreatureStateMachine;
using FMODUnity;
using Unity.Netcode;
using UnityEngine;

namespace Creatures {
    public abstract class Creature : NetworkBehaviour, IDamager, IDamageable, IAutomatable {
        public bool IsDestroyed => IsDead;
        public bool IsAutomatingEnabled { get; set; } = true;

        public int Damage => attackDamage;
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;
        public float PushingForce => 0;
        public bool IsSlowedDown { get; private set; }
        public CreatureMover Mover { get; private set; }
        public GameObject ObjectToAttack { get; private set; }
        public TargetPoint TargetPoint { get; private set; }
        public bool HasToDisappear { get; set; }
        public IDamager ObjectDamagedWith { get; private set; }
        public virtual bool HasSpawningAnimation => false;

        [SerializeField] private BloodParticle bloodParticlesPrefab;
        [SerializeField] private Transform bloodEffectCreatePoint;
        [SerializeField] private Color bloodColor;
        [SerializeField] private List<Material> colors;
        [SerializeField] private PhysicMaterial bouncingMaterial;

        [SerializeField] private int slowdownTimer = 6;
        [SerializeField] private int attackDamage = 10;

        [SerializeField]
        private Constants.ObjectsColors creatureColor;
        protected Constants.ObjectsColors CreatureColor => creatureColor;

        [SerializeField] private int health = 5;
        public int Health {
            get => health;
            set => health = value;
        }

        [Range(1, 100)]
        [SerializeField] private int chanceOfDroppingBalloon;
        public int ChanceOfDroppingBalloon => chanceOfDroppingBalloon;
        [SerializeField]
        private bool doNotMoveWhileExecutingSpecialAbility;
        [SerializeField] private bool hasSpecialAbility = true;
        [Tooltip("The min path point index that the creature has to reach in the path to be able to start special ability")]
        [SerializeField] private int specialAbilityMinPathPointIndex = 2;
        [Range(0, 100)]
        [SerializeField] private int specialAbilityChance = 50;
        public int SpecialAbilityChance => specialAbilityChance;

        [SerializeField] private int secondsToDestroyDeadBody = 10;
        private int SecondsToDestroyDeadBody => secondsToDestroyDeadBody;

        [Header("Speeds")]
        [SerializeField] private float patrolSpeed = 3;
        public float PatrolSpeed => patrolSpeed;

        [SerializeField] private float runSpeed = 6;
        public float RunSpeed => runSpeed;

        [SerializeField] protected float rotatingSpeed = 1;
        public float RotatingSpeed => rotatingSpeed;

        public CreatureAnimator Animator { get; private set; }
        public IReadOnlyList<BodyPart> BodyParts { get; private set; }
        public bool HasToFollowPath => !Mover.HasReachedPathEnd;

        public bool CouldActivateSpecialAbility {
            get {
                if (!hasSpecialAbility) return false;

                if (Mover.LastReachedPathPoint == null || PathToFollow == null) return false;
                return specialAbilityMinPathPointIndex <= Mover.LastReachedPathPoint.Index && Mover.LastReachedPathPoint.Index < PathToFollow.PathPoints.Last().Index;
            }
        }
        public bool IsCinematic { get; private set; }
        public bool IsPoisoned { get; private set; }
        public bool TargetReached { get; set; }

        public bool IsDead => IsStateActive<DeadState>();
        public bool IsIdle => IsStateActive<IdleState>();
        public bool IsAttacking => IsStateActive<AttackingState>();
        public bool IsFollowingPath => IsStateActive<FollowingPathState>();
        public float CurrentSpeed {
            get {
                if (IsStateActive<SpecialAbilityState>() && doNotMoveWhileExecutingSpecialAbility) {
                    currentSpeed = 0;
                    return 0;
                }

                if (!creatureStateMachine.PrimaryState.Speed.HasValue) return currentSpeed;
                currentSpeed = creatureStateMachine.PrimaryState.Speed.Value;
                return currentSpeed;
            }
        }
        private float currentSpeed;

        private int initialHealth;
        private StudioEventEmitter deathSound;
        private Rigidbody Rig { get; set; }
        private CreatureStateMachine creatureStateMachine;
        protected Action<bool> InformAnimationFinishedCallback;
        protected SpawnPointPath PathToFollow { get; private set; }

        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<Quaternion> networkRotation = new();

        protected virtual void Awake() {
            creatureStateMachine = GetComponent<CreatureStateMachine>();
            Rig = GetComponent<Rigidbody>();
            Mover = GetComponent<CreatureMover>();
            Animator = GetComponent<CreatureAnimator>();
            BodyParts = GetComponentsInChildren<BodyPart>();
            deathSound = GetComponent<StudioEventEmitter>();

            initialHealth = health;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (!IsServer) {
                InitClient();
            }
        }

        private void InitClient() {
            Destroy(creatureStateMachine);
            Destroy(Rig);
            Destroy(Mover);
            Destroy(Animator);
        }

        public void Init(Vector3 spawnPosition, SpawnPointPath pathToFollow, bool isCinematic, TargetPoint targetPoint, CreatureStateType initialCreatureState) {
            Rig.isKinematic = false;
            transform.position = spawnPosition;
            IsSlowedDown = false;
            IsCinematic = isCinematic;
            HasToDisappear = false;
            TargetReached = false;
            Health = initialHealth;
            ObjectDamagedWith = null;

            if (targetPoint is not null) {
                TargetPoint = targetPoint;
                ObjectToAttack = TargetPoint.TargetObject;
            }

            Rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
            PathToFollow = pathToFollow;

            Animator.Init();
            Mover.Init(pathToFollow);

            gameObject.SetActive(true);

            foreach (BodyPart bodyPart in BodyParts) {
                bodyPart.Init(bouncingMaterial);
            }

            if (initialCreatureState == CreatureStateType.None) {
                initialCreatureState = Ctx.Deps.WaveController.HasWaveStarted ? CreatureStateType.FollowingPath : CreatureStateType.Patrolling;
            }
            creatureStateMachine.Init(this, initialCreatureState);
        }

        private void Update() {
            if (IsServer) {
                networkPosition.Value = transform.position;
                networkRotation.Value = transform.rotation;
            } else {
                transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                transform.rotation = Quaternion.LerpUnclamped(transform.rotation, networkRotation.Value, .1f);
            }
        }

        public void OnMoverOrderFulfilled() {
            if (IsCinematic) {
                creatureStateMachine.SetNextCinematicState();
            }
        }

        public void TakeDamage(IDamager damager, int damageWeight) {
            ObjectDamagedWith = damager;
            creatureStateMachine.GetState<GettingHitState>().GotHit(ObjectDamagedWith, damageWeight);

            if (IsServer) {
                PlayBloodParticles();
                PlayBloodParticlesClientRPC();
            } else {
                PlayBloodParticlesServerRPC();
            }
        }

        private void PlayBloodParticles() {
            BloodParticle bloodParticle = bloodParticlesPrefab.GetObject<BloodParticle>(null);
            bloodParticle.transform.position = bloodEffectCreatePoint.position;
            bloodParticle.SetColor(bloodColor);
            bloodParticle.Play();
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlayBloodParticlesServerRPC() {
            PlayBloodParticles();
            PlayBloodParticlesClientRPC();
        }

        [ClientRpc]
        private void PlayBloodParticlesClientRPC() {
            PlayBloodParticles();
        }

        public virtual void ExecuteSpecialAbility(Action<bool> informAnimationFinishedCallback) {
            InformAnimationFinishedCallback = informAnimationFinishedCallback;
        }

        public bool IsStateActive<T>() where T : CreatureState {
            return creatureStateMachine.GetState<T>().IsActive;
        }

        public virtual void OnDeath() {
            deathSound.Play();
            Mover.OnDeath();
            Animator.OnDeath();
            Rig.isKinematic = true;

            foreach (BodyPart bodyPart in BodyParts) {
                bodyPart.OnDeath();
            }

            Ctx.Deps.SupplyBalloonController.SpawnBalloon(transform.position, ChanceOfDroppingBalloon);
            Ctx.Deps.GameController.StartCoroutine(DestroyObjectDelayed(SecondsToDestroyDeadBody));
        }

        private IEnumerator DestroyObjectDelayed(float secondsToDestroyDeadBody = 0) {
            Ctx.Deps.EventsManager.TriggerEnemyDied(this);
            yield return new WaitForSeconds(secondsToDestroyDeadBody);
            if (IsServer) {
                Despawn();
            } else {
                DespawnServerRPC();
            }
            Debug.Log($"Creature {this} disappeared");
        }

        private void Despawn() {
            gameObject.SetActive(false);
            NetworkObject.Despawn(false);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DespawnServerRPC() {
            Despawn();
        }

        public void Disappear() {
            Ctx.Deps.GameController.StartCoroutine(DestroyObjectDelayed());
        }
    }
}