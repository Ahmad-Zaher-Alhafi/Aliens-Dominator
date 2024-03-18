using System.Collections.Generic;
using UnityEngine;

public class SpawnPointPath : MonoBehaviour {
    [SerializeField] private PointPathType pointPath;
    public PointPathType PointPath => pointPath;

    [SerializeField] private List<PathPoint> pathPoints;
    public List<PathPoint> PathPoints => pathPoints;

    public enum PointPathType {
        GroundPath,
        AirPath
    }

    private void Awake() {
        foreach (PathPoint pathPoint in pathPoints) {
            pathPoint.Init(pathPoints.IndexOf(pathPoint));
        }
    }
}