using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {
    [SerializeField] private List<SpawnPointPath> groundPaths;
    [SerializeField] private SpawnPointPath airPath;
    public List<SpawnPointPath> GroundPaths => groundPaths;
    public SpawnPointPath AirPath => airPath;
}