using UnityEngine;

public class TargetPoint : MonoBehaviour {
    [SerializeField] private GameObject targetObject;
    public IDamageable TargetObject => targetObject.GetComponent<IDamageable>();

    public bool IsExist => !TargetObject.IsDestroyed;

    [SerializeField] private Color color;
    [SerializeField] private Vector3 size = Vector3.one;

    public void OnDrawGizmos() {
        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position, size);
    }
}