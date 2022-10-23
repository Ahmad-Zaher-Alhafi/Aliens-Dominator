using UnityEngine;
using System.Collections;

public class gizmo : MonoBehaviour
{
    public Color Color;

     void OnDrawGizmos() {
        Gizmos.color = Color;
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
}