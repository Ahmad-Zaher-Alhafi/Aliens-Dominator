using System;
using Projectiles;
using UnityEngine;

namespace AmmoMagazines {
    public abstract class Magazine : MonoBehaviour {
        public abstract Type AmmoType { get; }
        public bool IsEmpty => CurrentProjectilesNumber == 0;
        
        [SerializeField] protected int projectilesNumberOnStart;
        [SerializeField] protected int capacity;

        protected int CurrentProjectilesNumber;

        protected virtual void Awake() {
            Refill(projectilesNumberOnStart);
        }

        public abstract Projectile GetProjectile();

        public abstract void Refill(int projectilesNumberToAdd);
    }
}