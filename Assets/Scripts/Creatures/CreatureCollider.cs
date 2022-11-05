using Arrows;
using UnityEngine;

namespace Creatures {
    public class CreatureCollider : MonoBehaviour {
        [SerializeField] private ParticleSystem bloodEffect; //blood effect of the creature
        private Creature Creature;

        public void InitializeCollider(Creature creature) {
            Creature = creature;
        }
        
        /// <summary>
        ///     function to play blood effect particle system when the creature get hit by an arrow
        /// </summary>
        /// <param name="damagedPosition">
        ///     it's the position where the arrow hit the creature if it was hit by an arrow otherwise
        ///     it's null and blood will be in the meddle of the creautre
        /// </param>
        public void ActivateBloodEffect(Vector3 damagedPosition) {
            if (bloodEffect != null) {
                if (damagedPosition == null) damagedPosition = transform.position;

                bloodEffect.transform.position = damagedPosition; //let the particles be in the same posiotin where the arrow hit the creature
                bloodEffect.gameObject.SetActive(true);
                bloodEffect.Play();
            }
        }
    }
}