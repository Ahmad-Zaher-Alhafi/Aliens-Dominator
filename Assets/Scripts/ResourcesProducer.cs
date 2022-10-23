using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesProducer : MonoBehaviour
{
    [SerializeField] private float numOfResourcesToProduceEachLevel;

    private GameHandler gameHandler;

    // Start is called before the first frame update
    void Start()
    {
        gameHandler = FindObjectOfType<GameHandler>();
        EventsManager.onLevelFinishs += ProduceResources;
    }


    private void ProduceResources()
    {
        gameHandler.UpdateResourcesCount(numOfResourcesToProduceEachLevel);
    }

    private void OnDestroy()
    {
        EventsManager.onLevelFinishs -= ProduceResources;
    }
}
