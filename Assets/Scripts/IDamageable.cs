using System;
using UnityEngine;
public interface IDamageable {
    GameObject GameObject { get; }
    void TakeDamage(int damage, Enum damagedPart = null, ulong objectDamagedWithClientID = default);
    void TakeExplosionDamage(IDamager damager, int damage);
    bool IsDestroyed { get; }
    int Health { get; }
}