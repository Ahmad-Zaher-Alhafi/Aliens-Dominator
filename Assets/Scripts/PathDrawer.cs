using Context;
using DG.Tweening;
using Pool;
using UnityEngine;

public class PathDrawer : PooledObject {
    [SerializeField] private float animationDuration = .5f;
    [SerializeField] private Material materialPrefab;
    [SerializeField] private LineRenderer linerRenderer;
    
    private void Awake() {
        Material material = Instantiate(materialPrefab);
        linerRenderer.material = material;
        material.DOOffset(Vector2.left, animationDuration).SetEase(Ease.Linear)
            .SetLoops(-1)
            .OnStepComplete(() => material.mainTextureOffset = Vector2.zero)
            .SetLink(gameObject)
            .Play();

        Ctx.Deps.EventsManager.WaveFinished += OnWaveFinished;
    }

    private void OnWaveFinished() {
        ReturnToPool();
    }

    private void OnDestroy() {
        Ctx.Deps.EventsManager.WaveFinished -= OnWaveFinished;
    }
}