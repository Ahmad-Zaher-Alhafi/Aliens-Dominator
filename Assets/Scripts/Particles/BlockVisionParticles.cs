using UnityEngine;

namespace Particles {
    public class BlockVisionParticles : MonoBehaviour {
        [SerializeField] private Constants.ObjectsColors blockVisionParticlesColor;
        public Constants.ObjectsColors BlockVisionParticlesColor => blockVisionParticlesColor;
    }
}