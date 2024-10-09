using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class RangeVisualizer : MonoBehaviour {
    [SerializeField] private int segments = 50; // Number of segments to form the circle

    private LineRenderer lineRenderer;

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void ShowRange(Vector3 position, float radios) {
        // Set the number of points based on the number of segments
        lineRenderer.positionCount = segments + 1;

        float angle = 0f;
        for (int i = 0; i <= segments; i++) {
            // Calculate the x and z coordinates of each point on the circle
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * radios;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * radios;

            // Set the position of each point
            lineRenderer.SetPosition(i, new Vector3(x, 0, z) + position);

            // Increment the angle
            angle += 360f / segments;
        }
    }

    public void HideRange() {
        lineRenderer.positionCount = 0;
    }
}