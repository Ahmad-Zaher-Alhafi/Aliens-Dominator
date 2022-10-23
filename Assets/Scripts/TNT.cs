using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TNT : InteractableObjectBase
{
    public override void HandleArrowHit(ArrowBase arrow)
    {
        foreach (GameObject enemy in GameHandler.AllEnemies.ToArray())
        {
            CalculateDamageByDistance(enemy.GetComponent<Creature>());
        };

        Destroy(gameObject);
    }
}
