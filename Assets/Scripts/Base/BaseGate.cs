using UnityEngine;

namespace Base {
    public class BaseGate : MonoBehaviour, IDamageable {
        [SerializeField] private int health;
        public GameObject GameObject => gameObject;

        public bool IsDestroyed => health <= 0;

        public void TakeDamage(IDamager damager, int damageWeight) {
            int totalDamage = damager.Damage * damageWeight;
            health -= totalDamage;
            if (health <= 0) {
                Debug.Log("Game Over, Gate was destroyed!");
            }
        }
    }
}