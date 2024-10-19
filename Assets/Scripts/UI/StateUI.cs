using System.Collections;
using Context;
using Placeables;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI {
    public class StateUI : NetworkPlaceableObject {
        [SerializeField] private Slider healthBar;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image fillImage;

        private StateUIPlaceable stateUIPlaceable;
        private readonly NetworkVariable<int> networkHealthBarValue = new();
        private readonly NetworkVariable<Vector3> networkPosition = new();
        private readonly NetworkVariable<bool> networkShowHealthBar = new();

        /// <summary>
        /// If ture, then the owner will send some of its transform's properties to other clients on network
        /// </summary>
        private bool hasToSyncMotion;

        public override void BeforeDespawn() {
            base.BeforeDespawn();
            hasToSyncMotion = false;

            if (IsServer) {
                networkPosition.Value = Vector3.zero;
            }
        }

        public override void SetPlaceable(AddressablePlaceable placeable) {
            base.SetPlaceable(placeable);
            hasToSyncMotion = true;
            stateUIPlaceable = (StateUIPlaceable) placeable;

            Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork += OnPlayerSpawnedOnNetwork;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsServer) {
                InitClientRPC(stateUIPlaceable.HealthBarMaxValue, stateUIPlaceable.BackgroundColor, stateUIPlaceable.FillColor);
            }
        }

        private void OnPlayerSpawnedOnNetwork(Player.Player player) {
            if (IsServer) {
                StartCoroutine(InitClientDelayed());
            }
        }

        /// <summary>
        /// Give time for the state ui to get created
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitClientDelayed() {
            yield return new WaitUntil(() => IsSpawned);
            InitClientRPC(stateUIPlaceable.HealthBarMaxValue, stateUIPlaceable.BackgroundColor, stateUIPlaceable.FillColor);
        }

        [ClientRpc]
        private void InitClientRPC(int maxHealthBarValue, Color backgroundColor, Color fillColor) {
            healthBar.minValue = 0;
            healthBar.maxValue = maxHealthBarValue;
            backgroundImage.color = backgroundColor;
            fillImage.color = fillColor;
        }

        private void Update() {
            if (IsServer) {
                healthBar.value = stateUIPlaceable.HealthBarValue;
                networkHealthBarValue.Value = stateUIPlaceable.HealthBarValue;

                transform.position = stateUIPlaceable.Position;
                if (hasToSyncMotion) {
                    networkPosition.Value = transform.position;
                }

                healthBar.gameObject.SetActiveWithCheck(stateUIPlaceable.ShowHealthBar);
                networkShowHealthBar.Value = stateUIPlaceable.ShowHealthBar;
            } else {
                healthBar.value = networkHealthBarValue.Value;
                healthBar.gameObject.SetActive(networkShowHealthBar.Value);
                if (networkPosition.Value != Vector3.zero) {
                    transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
                }
            }
        }

        public override void OnDestroy() {
            base.OnDestroy();
            Ctx.Deps.EventsManager.PlayerSpawnedOnNetwork -= OnPlayerSpawnedOnNetwork;
        }
    }
}