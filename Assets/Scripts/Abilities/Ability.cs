using System;
using Unity.Netcode;
using UnityEngine;

namespace Abilities {
    public abstract class Ability : NetworkBehaviour {
        [SerializeField] private int coolDownSeconds = 240;

        public bool ReadyToBeUsed => TimeLeftToBeReady == TimeSpan.Zero;
        public TimeSpan TimeLeftToBeReady {
            get => timeLeftToBeReady;
            private set {
                timeLeftToBeReady = value;
                networkTicksLeftToBeReady.Value = timeLeftToBeReady.Ticks;
            }
        }
        private TimeSpan timeLeftToBeReady;
        private float lastActivationTime;

        private readonly NetworkVariable<long> networkTicksLeftToBeReady = new();

        protected void OnAbilityActivated() {
            lastActivationTime = Time.time;
        }

        private void Update() {
            if (!IsServer) {
                TimeLeftToBeReady = new TimeSpan(networkTicksLeftToBeReady.Value);
                return;
            }

            float elapsedTime = Time.time - lastActivationTime;
            TimeLeftToBeReady = new TimeSpan(0, 0, Mathf.CeilToInt(Mathf.Clamp(coolDownSeconds - elapsedTime, 0, coolDownSeconds)));
        }
    }
}