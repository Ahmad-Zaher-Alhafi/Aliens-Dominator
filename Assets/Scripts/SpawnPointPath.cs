using System.Collections.Generic;
using UnityEngine;

public class SpawnPointPath : MonoBehaviour {
    [SerializeField] private List<PathPoint> pathPoints;
    public List<PathPoint> PathPoints => pathPoints;
}