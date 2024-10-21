using System;
using UnityEngine;

public class DamageableObject : MonoBehaviour, IDamageable {
    [SerializeField] private int health;
    public int Health => health;
    public GameObject GameObject => gameObject;
    public bool IsDestroyed => health <= 0;

    public virtual void TakeDamage(int damage, Enum damagedPart = null, ulong objectDamagedWithClientID = default) {
        int totalDamage = damage;
        health -= totalDamage;
        Debug.Log($"{totalDamage} damage taken, current health = {Health}!");
    }

    public void TakeExplosionDamage(IDamager damager, int damage) {
        TakeDamage(damage);
    }
}