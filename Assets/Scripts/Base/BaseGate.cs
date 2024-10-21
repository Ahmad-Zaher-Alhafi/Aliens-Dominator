using System;
using Context;

namespace Base {
    public class BaseGate : DamageableObject {
        public override void TakeDamage(int damage, Enum damagedPart = null, ulong objectDamagedWithClientID = default) {
            base.TakeDamage(damage, damagedPart, objectDamagedWithClientID);
            if (Health <= 0) {
                Ctx.Deps.GameController.GameOver(false);
            }
        }
    }
}