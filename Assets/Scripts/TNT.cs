using Arrows;
using UnityEngine;
public class TNT : InteractableObjectBase {
    public override void HandleArrowHit(ArrowBase arrow) {
        foreach (GameObject enemy in GameController.AllEnemies.ToArray()) CalculateDamageByDistance(enemy.GetComponent<Creatures.Creature>());

        Destroy(gameObject);
    }
}