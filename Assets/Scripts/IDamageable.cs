using System;
using UnityEngine;
public interface IDamageable {
    GameObject GameObject { get; }
    void TakeDamage(IDamager damager, int damageWeight, Enum damagedPart = null);
    bool IsDestroyed { get; }
    int Health { get; }
}