using UnityEngine;
public class Gizmo : MonoBehaviour {
    public Color Color;

    private void OnDrawGizmos() {
        Gizmos.color = Color;
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
}