using Context;
using UnityEngine;

public class ScaledByCameraDistance : MonoBehaviour {
    [SerializeField] private float maxScale = 3.0f; // Maximum scale limit.
    [SerializeField] private float minDistance = 5f; // Distance at which the object has minScale.
    [SerializeField] private float maxDistance = 20f; // Distance at which the object has maxScale.

    private Vector3 initialScale;

    private void Awake() {
        initialScale = transform.localScale;
    }

    private void Update() {
        // Calculate the distance between the camera and the target object.
        float distance = Vector3.Distance(Ctx.Deps.CameraController.LocalActiveCamera.transform.position, transform.position);

        // Map the distance to a scale value between minScale and maxScale.
        float scaleX = Mathf.Lerp(initialScale.x, maxScale, (distance - minDistance) / (maxDistance - minDistance));
        float scaleY = Mathf.Lerp(initialScale.y, maxScale, (distance - minDistance) / (maxDistance - minDistance));
        float scaleZ = Mathf.Lerp(initialScale.z, maxScale, (distance - minDistance) / (maxDistance - minDistance));

        // Clamp the scale to ensure it stays within the min and max limits.
        scaleX = Mathf.Clamp(scaleX, initialScale.x, maxScale);
        scaleY = Mathf.Clamp(scaleY, initialScale.y, maxScale);
        scaleZ = Mathf.Clamp(scaleZ, initialScale.z, maxScale);

        // Apply the scale to the target object.
        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }
}