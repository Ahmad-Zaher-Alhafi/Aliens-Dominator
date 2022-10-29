using UnityEngine;

namespace Creatures {
    public interface IFightingCreature {
        public float SecondsBetweenGunShots { get; }
    }

    public class FightingCreature : Creature, IFightingCreature {
        [SerializeField] private float secondsBetweenGunShots;
        public float SecondsBetweenGunShots => secondsBetweenGunShots;

        [SerializeField] private float gunBulletSpeed;
        [SerializeField] private CreatureWeapon[] creatureWeapons;
    }
}