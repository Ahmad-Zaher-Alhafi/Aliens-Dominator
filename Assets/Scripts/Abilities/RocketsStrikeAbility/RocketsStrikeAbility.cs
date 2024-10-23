using System.Collections;
using System.Collections.Generic;
using Context;
using Multiplayer;
using Projectiles;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abilities.RocketsStrikeAbility {
    public class RocketsStrikeAbility : Ability {
        [SerializeField] private List<Transform> strikeRocketCreatePoints;
        [SerializeField] private StrikeRocket strikeRocketPrefab;
        [SerializeField] private float rocketsStrikeAreaRadios = 25;
        [SerializeField] private GameObject rangeVisualizerPrefab;

        private bool isInitialized;
        private RangeVisualizer rangeVisualizer;

        private void Awake() {
            Ctx.Deps.InputActions.TopDownViewActions.ShowRocketsStrikeArea.performed += OnShowRocketsStrikeAreaPressed;
            Ctx.Deps.InputActions.SharedActions.PrimaryAction.performed += OnPrimaryActionPressed;
            Ctx.Deps.InputActions.SharedActions.SecondaryAction.performed += OnSecondaryActionPressed;
        }

        public override void StartUsage() {
            if (!ReadyToBeUsed) return;
            if (isInitialized) return;
            rangeVisualizer = NetworkObjectPool.Singleton.GetNetworkObject(rangeVisualizerPrefab, default, default).GetComponent<RangeVisualizer>();
            rangeVisualizer.ShowMouseFollowerRange(rocketsStrikeAreaRadios, true);
            isInitialized = true;
        }

        private void OnShowRocketsStrikeAreaPressed(InputAction.CallbackContext obj) {
            StartUsage();
        }

        private void OnPrimaryActionPressed(InputAction.CallbackContext obj) {
            if (!isInitialized) return;
            Vector3 mouseWorldHitPoint = Ctx.Deps.InputController.GetMouseWorldHitPoint(LayerMask.NameToLayer("Terrain"));
            SpawnStrikeRockets(mouseWorldHitPoint, rangeVisualizer);
            isInitialized = false;
            rangeVisualizer.StopFollowingMouse();
        }

        private void OnSecondaryActionPressed(InputAction.CallbackContext obj) {
            UninitializeStrikeArea();
        }

        private void SpawnStrikeRockets(Vector3 center, RangeVisualizer rangeVisualizer) {
            if (!IsServer) {
                SpawnStrikeRocketsServerRPC(center, rangeVisualizer.transform.position);
                NetworkObjectPool.Singleton.ReturnNetworkObject(rangeVisualizer.NetworkObject, rangeVisualizerPrefab);
                return;
            }

            UninitializeStrikeAreaClientRPC();
            rangeVisualizer.NetworkObject.Spawn();
            rangeVisualizer.ShowRangeClientRPC(center, rocketsStrikeAreaRadios, true);
            rangeVisualizer.PlayColorDimmingAnimationClientRPC();
            rangeVisualizer.HideRangeDelayedClientRPC(strikeRocketPrefab.ExpectedArriveTime);
            StartCoroutine(SpawnStrikeRocketsDelayed(center));

            OnAbilityActivated();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnStrikeRocketsServerRPC(Vector3 center, Vector3 visualiserPosition) {
            RangeVisualizer visualizer = NetworkObjectPool.Singleton.GetNetworkObject(rangeVisualizerPrefab, default, default).GetComponent<RangeVisualizer>();
            visualizer.transform.position = visualiserPosition;
            SpawnStrikeRockets(center, visualizer);
        }

        private IEnumerator SpawnStrikeRocketsDelayed(Vector3 center) {
            foreach (Transform strikeRocketCreatePoint in strikeRocketCreatePoints) {
                StrikeRocket rocket = NetworkObjectPool.Singleton.GetNetworkObject(strikeRocketPrefab.gameObject, strikeRocketCreatePoint.position, strikeRocketCreatePoint.rotation).GetComponent<StrikeRocket>();
                rocket.SetTargetPosition(GetRandomPositionInCircle(center));
                rocket.NetworkObject.Spawn();
                yield return new WaitForSeconds(.2f);
            }
        }

        /// <summary>
        /// Only one player can activate the ability at a time, so if some other player activated it, and if it was initialised on some other player side then it should uninitialised on his side
        /// </summary>
        [ClientRpc]
        private void UninitializeStrikeAreaClientRPC() {
            UninitializeStrikeArea();
        }

        private void UninitializeStrikeArea() {
            if (!isInitialized) return;

            rangeVisualizer?.HideRange(true);
            isInitialized = false;
        }

        /// <summary>
        /// Returns a random position on X and Z inside the given center, Y value is the y of a ray cast down hit point
        /// </summary>
        /// <param name="center">Center of the circle</param>
        /// <returns></returns>
        private Vector3 GetRandomPositionInCircle(Vector3 center) {
            // Get a random angle between 0 and 2 * PI (360 degrees)
            float angle = Random.Range(0f, Mathf.PI * 2);

            // Get a random distance from the center, weighted by the radius
            float distance = Mathf.Sqrt(Random.Range(0f, 1f)) * rocketsStrikeAreaRadios;

            // Calculate the x and y coordinates based on angle and distance
            float x = center.x + distance * Mathf.Cos(angle);
            float z = center.z + distance * Mathf.Sin(angle);

            // + 1 for the origin on y-axis to prevent starting the raycast inside the terrain otherwise it won't detect it
            Physics.Raycast(new Vector3(x, center.y + 1, z), Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain"));

            return new Vector3(x, hit.point.y, z);
        }

        public override void OnDestroy() {
            base.OnDestroy();
            Ctx.Deps.InputActions.TopDownViewActions.ShowRocketsStrikeArea.performed -= OnShowRocketsStrikeAreaPressed;
            Ctx.Deps.InputActions.SharedActions.PrimaryAction.performed -= OnPrimaryActionPressed;
            Ctx.Deps.InputActions.SharedActions.SecondaryAction.performed -= OnSecondaryActionPressed;
        }
    }
}