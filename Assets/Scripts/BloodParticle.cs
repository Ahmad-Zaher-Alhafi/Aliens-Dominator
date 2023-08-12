using UnityEngine;
public class BloodParticle : Particle {
    private ParticleSystemRenderer particleSystemRenderer;
    private ParticleSystemRenderer subEmitterParticleSystemRenderer;

    protected override void Awake() {
        base.Awake();
        particleSystemRenderer = GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>();
        subEmitterParticleSystemRenderer = GetComponent<ParticleSystem>().subEmitters.GetSubEmitterSystem(0).GetComponent<ParticleSystemRenderer>();
    }

    public void SetColor(Color color) {
        particleSystemRenderer.material.color = color;
        subEmitterParticleSystemRenderer.material.color = color;
    }
}