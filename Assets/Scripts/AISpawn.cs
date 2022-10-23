using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class AISpawn : MonoBehaviour
{
private GameObject objSpawn;
private int SpawnerID;

void Start () {
	objSpawn = (GameObject) GameObject.FindWithTag ("Spawner");
}

void removeMe ()
{
	objSpawn.BroadcastMessage("killEnemy", SpawnerID);
	Destroy(gameObject);
}

void setName(int sName)
{
	SpawnerID = sName;
}
}
