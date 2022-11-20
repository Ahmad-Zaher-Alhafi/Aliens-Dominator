using System;
using UnityEngine;

namespace ManagersAndControllers {
    public interface IEventManager {
        public event Action WaveStarted;
        public void TriggerWaveStarted();

        public event Action<Constants.SuppliesTypes> onCallingSupplies; //event that being shot when you takes the object that brought you supplies bu the aireplane
        public void OnCallingSupplies(Constants.SuppliesTypes suppliesType);

        public event Action onGatheringSupplies; //event that being shot when you gather a supplies object
        public void OnGatheringSupplies();

        public event Action<Constants.ObjectsColors> onStinkyBallHit; //event that being shot when the player get hit by a stinky ball
        public void OnStinkyBallHit(Constants.ObjectsColors stinkyBallColor);

        public event Action<Transform> onAirLeaderPatrolling; //event that being shot when the leader of the air creatures group reachs the last point and start to patrol around
        public void OnAirLeaderPatrolling(Transform leader);

        public event Action onAirLeaderDeath; //event that being shot when the leader of the air group dies
        public void OnAirLeaderDeath();

        public event Action<Creatures.Creature> onEnemyDiesInsideSecurityArea; //event that being shot when a creature dies inside the seacurity area of the seacurity weapon
        public void OnEnemyDiesInsideSecurityArea(Creatures.Creature creature);

        public event Action<Constants.SuppliesTypes, int> onTakingAmmo; //event that being shot when the player takes a rocket ammo
        public void OnTakingAmmo(Constants.SuppliesTypes ammoType, int ammoNumber);

        public event Action onLevelFinishs; //event that being shot when the a level finishs
        public void OnLevelFinishs();

        public event Action onLevelStarts; //event that being shot when the a level starts
        public void OnLevelStarts();

        public event Action onSecurityWeaponDestroy; //event that being shot when one of the security weapons get destroied
        public void OnSecurityWeaponDestroy();

        public event Action onBossDie; //event that being shot when the boss of the levle get died
        public void OnBossDie();
    }

    public class EventsManager : MonoBehaviour, IEventManager {
        public event Action WaveStarted; 
        public void TriggerWaveStarted() {
            WaveStarted?.Invoke();
        }

        public event Action<Constants.SuppliesTypes> onCallingSupplies; //event that being shot when you takes the object that brought you supplies bu the aireplane
        public void OnCallingSupplies(Constants.SuppliesTypes suppliesType) {
            onCallingSupplies?.Invoke(suppliesType);
        }

        public event Action onGatheringSupplies; //event that being shot when you gather a supplies object
        public void OnGatheringSupplies() {
            onGatheringSupplies?.Invoke();
        }

        public event Action<Constants.ObjectsColors> onStinkyBallHit; //event that being shot when the player get hit by a stinky ball
        public void OnStinkyBallHit(Constants.ObjectsColors stinkyBallColor) {
            onStinkyBallHit?.Invoke(stinkyBallColor);
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

        public event Action<Constants.SuppliesTypes, int> onTakingAmmo; //event that being shot when the player takes a rocket ammo
        public void OnTakingAmmo(Constants.SuppliesTypes ammoType, int ammoNumber) {
            onTakingAmmo?.Invoke(ammoType, ammoNumber);
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