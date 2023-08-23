using UnityEngine;
using UnityEngine.Serialization;

public class PathPoint : MonoBehaviour {
    [FormerlySerializedAs("Color")]
    [SerializeField] private Color color;
    [SerializeField] private float debugDrawRadius = 1.0F;

    public int Index { get; private set; }

    public void Init(int indexInList) {
        Index = indexInList;
    }

    public void OnDrawGizmos() {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, debugDrawRadius);
    }
}