using Context;

namespace Base {
    public class BaseGate : DamageableObject {
        public override void TakeDamage(IDamager damager, int damageWeight) {
            base.TakeDamage(damager, damageWeight);
            if (Health <= 0) {
                Ctx.Deps.GameController.GameOver(false);
            }
        }
    }
}