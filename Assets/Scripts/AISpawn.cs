using UnityEngine;
public class AISpawn : MonoBehaviour {
    private GameObject objSpawn;
    private int SpawnerID;

    private void Start() {
        objSpawn = GameObject.FindWithTag("Spawner");
    }

    private void removeMe() {
        objSpawn.BroadcastMessage("killEnemy", SpawnerID);
        Destroy(gameObject);
    }

    private void setName(int sName) {
        SpawnerID = sName;
    }
}