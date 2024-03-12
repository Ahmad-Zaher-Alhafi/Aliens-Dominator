using UnityEngine;

public class TargetPoint : MonoBehaviour {
    [SerializeField] private DamageableObject targetObject;
    public DamageableObject TargetObject => targetObject;

    [SerializeField] private Color color;
    [SerializeField] private Vector3 size = Vector3.one;

    public void OnDrawGizmos() {
        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position, size);
    }
}