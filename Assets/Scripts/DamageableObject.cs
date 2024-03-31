using System;
using UnityEngine;

public class DamageableObject : MonoBehaviour, IDamageable {
    [SerializeField] private int health;
    public int Health => health;
    public GameObject GameObject => gameObject;
    public bool IsDestroyed => health <= 0;

    public virtual void TakeDamage(IDamager damager, int damageWeight, Enum damagedPart = null) {
        int totalDamage = damager.Damage * damageWeight;
        health -= totalDamage;
        Debug.Log($"{totalDamage} damage taken, current health = {Health}!");
    }
}