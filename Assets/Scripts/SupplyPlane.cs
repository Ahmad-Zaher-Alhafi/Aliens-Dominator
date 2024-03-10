using System;
using System.Collections;
using FMODUnity;
using Multiplayer;
using Unity.Netcode;
using UnityEngine;
public class SupplyPlane : NetworkBehaviour {
    [SerializeField] private GameObject arrowUpgradePrefab;
    [SerializeField] private GameObject rocketsAmmoPrefab;
    [SerializeField] private GameObject bulletsAmmoPrefab;

    [SerializeField] private float airplaneSpeed;
    [SerializeField] private float secondsToDropSupplies = 4;
    [SerializeField] private float secondsToDespawn = 15;
    [SerializeField] private Transform dropCreatingPoint;
    [SerializeField] private float throwingForce = 5;
    [SerializeField] private SmokeParticles smokeParticlePrefab;
    [SerializeField] private Transform smokeParticlePoint;

    private StudioEventEmitter airplaneSound;
    private readonly NetworkVariable<Vector3> networkPosition = new();
    private SmokeParticles launchSmokeParticle;

    private void Awake() {
        airplaneSound = GetComponent<StudioEventEmitter>();
    }

    private void Update() {
        if (IsServer) {
            transform.position += transform.forward * airplaneSpeed * Time.deltaTime;
            networkPosition.Value = transform.position;
        } else {
            transform.position = Vector3.LerpUnclamped(transform.position, networkPosition.Value, .1f);
        }
    }

    /// <summary>
    /// To let the plane move to the area where to drop the supplies
    /// </summary>
    public void MoveToDropArea(Constants.SuppliesTypes suppliesType) {
        PlayAirplaneSoundClientRPC();
        CreateLaunchSmokeParticleClientRPC();
        StartCoroutine(DropSupplies(suppliesType));
        StartCoroutine(DestroyDelayed());
    }

    [ClientRpc]
    private void PlayAirplaneSoundClientRPC() {
        airplaneSound.Play();
    }

    /// <summary>
    /// To let the plane drops the supplies after it reaches the dropping area
    /// </summary>
    private IEnumerator DropSupplies(Constants.SuppliesTypes suppliesType) {
        yield return new WaitForSeconds(secondsToDropSupplies);

        var position = dropCreatingPoint.position;
        NetworkObject supplies = suppliesType switch {
            Constants.SuppliesTypes.ArrowUpgrade => NetworkObjectPool.Singleton.GetNetworkObject(arrowUpgradePrefab, position, Quaternion.identity),
            Constants.SuppliesTypes.RocketsAmmo => NetworkObjectPool.Singleton.GetNetworkObject(rocketsAmmoPrefab, position, Quaternion.identity),
            Constants.SuppliesTypes.BulletsAmmo => NetworkObjectPool.Singleton.GetNetworkObject(bulletsAmmoPrefab, position, Quaternion.identity),
            _ => throw new ArgumentException($"Unknown {suppliesType} supplies type")
        };

        supplies.Spawn();
        var supRig = supplies.GetComponent<Rigidbody>();
        supRig.AddForceAtPosition(transform.forward * throwingForce, dropCreatingPoint.position, ForceMode.Impulse);
    }

    private void CreateLaunchSmokeParticle() {
        launchSmokeParticle = smokeParticlePrefab.GetObject<SmokeParticles>(transform);
        launchSmokeParticle.transform.position = smokeParticlePoint.position;
        launchSmokeParticle.transform.rotation = smokeParticlePoint.rotation;
        launchSmokeParticle.Play();
    }

    [ClientRpc]
    private void CreateLaunchSmokeParticleClientRPC() {
        CreateLaunchSmokeParticle();
    }

    [ClientRpc]
    private void ReleaseSmokeParticleFromParentClientRPC() {
        launchSmokeParticle.transform.parent = null;
        launchSmokeParticle.Stop();
    }

    private IEnumerator DestroyDelayed() {
        yield return new WaitForSeconds(secondsToDespawn);
        ReleaseSmokeParticleFromParentClientRPC();
        airplaneSound.Stop();
        NetworkObject.Despawn();
    }
}