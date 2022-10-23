using System.Collections;
using UnityEngine;
public class DestroyAfterTime : MonoBehaviour {
    public void OrderToDestroy(float secondsToDestroy) {
        StartCoroutine(DestroyAfterSeconds(secondsToDestroy));
    }

    private IEnumerator DestroyAfterSeconds(float secondsToDestroy) {
        yield return new WaitForSeconds(secondsToDestroy);
        Destroy(gameObject);
    }
}