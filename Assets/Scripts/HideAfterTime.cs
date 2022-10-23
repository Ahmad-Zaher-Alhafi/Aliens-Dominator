using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideAfterTime : MonoBehaviour
{
    [SerializeField] private float timeToHideObject = 3;//time needed before hiding the object

    void OnEnable()
    {
        StartCoroutine(DisableObject());
    }

    private IEnumerator DisableObject()
    {
        yield return new WaitForSeconds(timeToHideObject);
        gameObject.SetActive(false);
    }
}
