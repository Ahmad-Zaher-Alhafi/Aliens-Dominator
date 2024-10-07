using System;
using AmmoMagazines;
using Creatures;
using ScriptableObjects;
using UnityEngine;

namespace ManagersAndControllers {
    public class EventsManager : MonoBehaviour {
        public event Action<Wave> WaveStarted;
        public void TriggerWaveStarted(Wave wave) {
            WaveStarted?.Invoke(wave);
        }

        public event Action WaveFinished;
        public void TriggerWaveFinished() {
            WaveFinished?.Invoke();
        }

        public event Action<Creature> EnemyDied;
        public void TriggerEnemyDied(Creature creature) {
            EnemyDied?.Invoke(creature);
        }

        public event Action<Creature> EnemyGotHit;
        public void TriggerEnemyGotHit(Creature creature) {
            EnemyGotHit?.Invoke(creature);
        }

        public event Action<Creature, PathPoint> PathPointReached;
        public void TriggerPathPointReached(Creature creature, PathPoint pathPoint) {
            PathPointReached?.Invoke(creature, pathPoint);
        }

        public event Action<Vector3> PlayerTeleported;
        public void TriggerPlayerTeleported(Vector3 teleportPosition) {
            PlayerTeleported?.Invoke(teleportPosition);
        }

        public event Action<SuppliesController.SuppliesTypes> SupplyBalloonCollected;
        public void TriggerSupplyBalloonCollected(SuppliesController.SuppliesTypes suppliesType) {
            SupplyBalloonCollected?.Invoke(suppliesType);
        }

        public event Action<GameController.ViewMode, GameController.ViewMode> ViewModeChanged;
        public void TriggerViewModeChanged(GameController.ViewMode previousViewMode, GameController.ViewMode currentViewMode) {
            ViewModeChanged?.Invoke(previousViewMode, currentViewMode);
        }

        public event Action<Magazine.AmmoType, int> AmmoSuppliesCollected;
        public void TriggerAmmoSuppliesCollected(Magazine.AmmoType ammoType, int ammoNumber) {
            AmmoSuppliesCollected?.Invoke(ammoType, ammoNumber);
        }

        public event Action UpgradesSuppliesCollected;
        public void TriggerUpgradesSuppliesCollected() {
            UpgradesSuppliesCollected?.Invoke();
        }

        public event Action<Player.Player> PlayerSpawnedOnNetwork;
        public void TriggerPlayerSpawnedOnNetwork(Player.Player player) {
            PlayerSpawnedOnNetwork?.Invoke(player);
        }

        public event Action<Player.Player> PlayerDespawnedFromNetwork;
        public void TriggerPlayerDespawnedFromNetwork(Player.Player player) {
            PlayerDespawnedFromNetwork?.Invoke(player);
        }

        public event Action<Transform> onAirLeaderPatrolling; //event that being shot when the leader of the air creatures group reachs the last point and start to patrol around
        public void OnAirLeaderPatrolling(Transform leader) {
            onAirLeaderPatrolling?.Invoke(leader);
        }

        public event Action onAirLeaderDeath; //event that being shot when the leader of the air group dies
        public void OnAirLeaderDeath() {
            onAirLeaderDeath?.Invoke();
        }

        public event Action<Creatures.Creature> onEnemyDiesInsideSecurityArea; //event that being shot when a creature dies inside the seacurity area of the seacurity weapon
        public void OnEnemyDiesInsideSecurityArea(Creatures.Creature creature) {
            onEnemyDiesInsideSecurityArea?.Invoke(creature);
        }

        public event Action onLevelFinishs; //event that being shot when the a level finishs
        public void OnLevelFinishs() {
            onLevelFinishs?.Invoke();
        }

        public event Action onLevelStarts; //event that being shot when the a level starts
        public void OnLevelStarts() {
            onLevelStarts?.Invoke();
        }

        public event Action onSecurityWeaponDestroy; //event that being shot when one of the security weapons get destroied
        public void OnSecurityWeaponDestroy() {
            onSecurityWeaponDestroy?.Invoke();
        }

        public event Action onBossDie; //event that being shot when the boss of the levle get died
        public void OnBossDie() {
            onBossDie?.Invoke();
        }
    }
}