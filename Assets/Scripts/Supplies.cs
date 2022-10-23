using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Supplies : MonoBehaviour
{
    [SerializeField] private Constants.SuppliesTypes suppliesType;
    [Header("For Ammo supplies only")]
    [SerializeField] private int numOfAmmoInThePack;

    void OnCollisionEnter(Collision other)
   {
       
        if (other.gameObject.tag != "Arrow")
        {
            return;
        }

        if (suppliesType == Constants.SuppliesTypes.ArrowUpgrade)
        {
            EventsManager.OnGatheringSupplies();
        }
        else
        {
            EventsManager.OnTakingAmmo(suppliesType, numOfAmmoInThePack);
        }

        Destroy(gameObject);
    }
}
