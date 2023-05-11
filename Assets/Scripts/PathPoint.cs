﻿using UnityEngine;
using UnityEngine.Serialization;

public class PathPoint : MonoBehaviour {
    [FormerlySerializedAs("Color")]
    [SerializeField] private Color color;
    [SerializeField] private float debugDrawRadius = 1.0F;

    public void OnDrawGizmos() {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, debugDrawRadius);
    }
}