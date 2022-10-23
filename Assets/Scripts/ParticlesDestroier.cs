using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesDestroier : MonoBehaviour
{
    [SerializeField] private float secondsToDestroy;

    public void DestroyAfterSeconds()
    {
        StartCoroutine(DestroyAfterTime());
    }

    public IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(secondsToDestroy);
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
