using UnityEngine;
using UnityEngine.Serialization;

public class PathPoint : MonoBehaviour {
    [SerializeField] private float debugDrawRadius = 1.0F;
    [FormerlySerializedAs("Color")]
    [SerializeField] private Color color;

    public void OnDrawGizmos() {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, debugDrawRadius);
    }
}