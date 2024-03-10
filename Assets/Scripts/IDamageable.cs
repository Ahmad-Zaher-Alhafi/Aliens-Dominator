using UnityEngine;
public interface IDamageable {
    GameObject GameObject { get; }
    void TakeDamage(IDamager damager, int damageWeight);
    bool IsDestroyed { get; }
    int Health { get; }
}