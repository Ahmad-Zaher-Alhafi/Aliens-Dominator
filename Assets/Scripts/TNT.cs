using Arrows;
using UnityEngine;
public class TNT : InteractableObjectBase {
    public override void HandleArrowHit(ArrowBase arrow) {
        foreach (GameObject enemy in GameHandler.AllEnemies.ToArray()) CalculateDamageByDistance(enemy.GetComponent<Creature.Creature>());
        ;

        Destroy(gameObject);
    }
}