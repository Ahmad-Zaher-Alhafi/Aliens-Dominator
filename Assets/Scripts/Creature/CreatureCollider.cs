using Arrows;

namespace Creature {
    public class CreatureCollider : Hitable {
        private Creature Creature;

        public void InitializeCollider(Creature creature) {
            Creature = creature;
        }

        public override void HandleArrowHit(ArrowBase arrow) {
            if (!Creature)
                return;

            //Debug.Log(name);

            Creature.HandleHit(arrow, tag);
        }
    }
}