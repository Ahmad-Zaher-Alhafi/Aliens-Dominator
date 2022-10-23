using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventsManager
{
    public static event Action onStartEnemyDeath;//event that beaing shot when one of the start enemies being died
    public static event Action<Constants.SuppliesTypes> onCallingSupplies;//event that beaing shot when you takes the object that brought you supplies bu the aireplane
    public static event Action onGatheringSupplies;//event that beaing shot when you gather a supplies object
    public static event Action<Constants.ObjectsColors> onStinkyBallHit;//event that beaing shot when the player get hit by a stinky ball
    public static event Action<Transform> onAirLeaderPatrolling;//event that beaing shot when the leader of the air creatures group reachs the last point and start to patrol around
    public static event Action onAirLeaderDeath;//event that beaing shot when the leader of the air group dies
    public static event Action<Creature> onEnemyDiesInsideSecurityArea;//event that beaing shot when a creature dies inside the seacurity area of the seacurity weapon
    public static event Action<Constants.SuppliesTypes, int> onTakingAmmo;//event that beaing shot when the player takes a rocket ammo
    public static event Action onLevelFinishs;//event that beaing shot when the a level finishs
    public static event Action onLevelStarts;//event that beaing shot when the a level starts
    public static event Action onSecurityWeaponDestroy;//event that beaing shot when one of the security weapons get destroied
    public static event Action onBossDie;//event that beaing shot when the boss of the levle get died



    public static void OnStartEnemyDeath()
    {
        onStartEnemyDeath?.Invoke();
    }

    public static void OnCallingSupplies(Constants.SuppliesTypes suppliesType)
    {
        onCallingSupplies?.Invoke(suppliesType);
    }

    public static void OnGatheringSupplies()
    {
        onGatheringSupplies?.Invoke();
    }

    public static void OnStinkyBallHit(Constants.ObjectsColors stinkyBallColor)
    {
        onStinkyBallHit?.Invoke(stinkyBallColor);
    }

    public static void OnAirLeaderPatrolling(Transform leader)
    {
        onAirLeaderPatrolling?.Invoke(leader);
    }

    public static void OnAirLeaderDeath()
    {
        onAirLeaderDeath?.Invoke();
    }

    public static void OnEnemyDiesInsideSecurityArea(Creature creature)
    {
        onEnemyDiesInsideSecurityArea?.Invoke(creature);
    }

    public static void OnTakingAmmo(Constants.SuppliesTypes ammoType, int ammoNumber)
    {
        onTakingAmmo?.Invoke(ammoType, ammoNumber);
    }

    public static void OnLevelFinishs()
    {
        onLevelFinishs?.Invoke();
    }

    public static void OnLevelStarts()
    {
        onLevelStarts?.Invoke();
    }

    public static void OnSecurityWeaponDestroy()
    {
        onSecurityWeaponDestroy?.Invoke();
    }

    public static void OnBossDie()
    {
        onBossDie?.Invoke();
    }
}
