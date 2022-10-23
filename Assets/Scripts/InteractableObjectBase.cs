using ManagersAndControllers;
using UnityEngine;
public abstract class InteractableObjectBase : Hitable {
    public float Range = 50f;
    public float Damage = 250f;
    public float Force = 1000f;

    protected GameHandler GameHandler;

    private void Start() {
        GameHandler = FindObjectOfType<GameHandler>();
    }

    protected void CalculateDamageByDistance(Creature.Creature creature) {
        float distance = Vector3.Distance(transform.position, creature.transform.position);
        if (distance > Range) return;

        creature.ReceiveDamageFromObject(Damage, Force);
    }
}