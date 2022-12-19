using UnityEngine;

public class AttackPoint : MonoBehaviour {
    [SerializeField] private GameObject targetObject;
    public GameObject TargetObject => targetObject;
    
    [SerializeField] private Color color;
    [SerializeField] private float debugDrawRadius = 1.0F;

    public void OnDrawGizmos() {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, debugDrawRadius);
    }
}