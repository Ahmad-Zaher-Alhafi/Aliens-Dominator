using System.Collections;
using Context;
using DG.Tweening;
using DoTweenAnimations;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Utils.Extensions;

[RequireComponent(typeof(LineRenderer))]
public class RangeVisualizer : NetworkBehaviour {
    [SerializeField] private int segments = 50; // Number of segments to form the circle
    [SerializeField] private float dimmingTime = .5f;
    [SerializeField] private TextMeshPro icon;
    [SerializeField] private FadeTextInOutAnimation fadeTextInOutAnimation;

    private LineRenderer lineRenderer;
    private bool followMouse;
    private float radios;
    private bool showIcon;

    private readonly NetworkVariable<Vector3> networkPosition = new();

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update() {
        if (followMouse) {
            Vector3 mouseWorldPosition = Ctx.Deps.InputController.GetMouseWorldHitPoint(LayerMask.NameToLayer("Terrain"));
            ShowRange(mouseWorldPosition, radios, showIcon);
            transform.position = mouseWorldPosition;
        } else {
            if (!IsServer) {
                transform.position = networkPosition.Value;
            }
        }
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsServer) {
            networkPosition.Value = transform.position;
        }
    }

    public void ShowRange(Vector3 center, float radios, bool showIcon = false) {
        if (showIcon) {
            icon.gameObject.SetActiveWithCheck(true);
            FitIconWithinRadius(radios);
        }

        // Set the number of points based on the number of segments
        lineRenderer.positionCount = segments + 1;

        float angle = 0f;
        for (int i = 0; i <= segments; i++) {
            // Calculate the x and z coordinates of each point on the circle
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * radios;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * radios;

            // Set the position of each point
            lineRenderer.SetPosition(i, new Vector3(x, 0, z) + center);

            // Increment the angle
            angle += 360f / segments;
        }
    }

    [ClientRpc]
    public void ShowRangeClientRPC(Vector3 center, float radios, bool showIcon = false) {
        ShowRange(center, radios, showIcon);
    }

    private void FitIconWithinRadius(float radios) {
        icon.ForceMeshUpdate();

        // Get the bounds of the TextMeshPro text
        Bounds textBounds = icon.bounds;
        float maxDimension = Mathf.Max(textBounds.size.x, textBounds.size.y) * 1.3f; // Get the largest dimension

        // Calculate the scale factor to fit within the given radius
        float scaleFactor = (radios * 2) / maxDimension; // Use circle's diameter

        // Apply the scale uniformly
        icon.transform.localScale = Vector3.one * scaleFactor;
    }

    public void ShowMouseFollowerRange(float radios, bool showIcon) {
        this.radios = radios;
        followMouse = true;
        this.showIcon = showIcon;
    }

    public void HideRange(bool despawn = false) {
        lineRenderer.positionCount = 0;
        followMouse = false;
        radios = 0;
        icon.gameObject.SetActive(false);
        fadeTextInOutAnimation.StopFadeInOutAnimation();

        if (IsServer && IsSpawned && despawn) {
            NetworkObject.Despawn();
        }
    }

    [ClientRpc]
    public void HideRangeDelayedClientRPC(float secondsToHide, bool despawn = false) {
        StartCoroutine(HideRangeAfter(secondsToHide, despawn));
    }

    private IEnumerator HideRangeAfter(float secondsToHide, bool despawn = false) {
        yield return new WaitForSeconds(secondsToHide);
        HideRange(despawn);
    }

    public void StopFollowingMouse() {
        followMouse = false;
    }

    [ClientRpc]
    public void PlayColorDimmingAnimationClientRPC() {
        if (icon == null) return;
        fadeTextInOutAnimation.PlayFadeInOutAnimation();
    }
}