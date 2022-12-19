using ManagersAndControllers;
using UnityEngine;
public abstract class InteractableObjectBase : Hitable {
    public float Range = 50f;
    public int Damage = 250;
    public int Force = 1000;

    protected GameController GameController;

    private void Start() {
        GameController = FindObjectOfType<GameController>();
    }

    protected void CalculateDamageByDistance(Creatures.Creature creature) {
        float distance = Vector3.Distance(transform.position, creature.transform.position);
        if (distance > Range) return;
    }
}