using UnityEngine;
public class PathPoint : MonoBehaviour {
    [SerializeField]
    protected float debugDrawRadius = 1.0F;
    public Color Color;
    public GameObject LookAtGO;

    public void OnDrawGizmos() {
        Gizmos.color = Color;
        Gizmos.DrawWireSphere(transform.position, debugDrawRadius);
    }
}