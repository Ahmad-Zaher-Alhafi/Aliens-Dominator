using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnPointPath : MonoBehaviour {
    [FormerlySerializedAs("pointPath")]
    [SerializeField] private PointPathType pathType;
    public PointPathType PathType => pathType;

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