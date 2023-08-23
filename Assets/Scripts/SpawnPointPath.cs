using System.Collections.Generic;
using UnityEngine;

public class SpawnPointPath : MonoBehaviour {
    [SerializeField] private List<PathPoint> pathPoints;
    public List<PathPoint> PathPoints => pathPoints;

    private void Awake() {
        foreach (PathPoint pathPoint in pathPoints) {
            pathPoint.Init(pathPoints.IndexOf(pathPoint));
        }
    }
}