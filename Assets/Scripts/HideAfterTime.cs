using System.Collections;
using UnityEngine;
public class HideAfterTime : MonoBehaviour {
    [SerializeField] private float timeToHideObject = 3; //time needed before hiding the object

    private void OnEnable() {
        StartCoroutine(DisableObject());
    }

    private IEnumerator DisableObject() {
        yield return new WaitForSeconds(timeToHideObject);
        gameObject.SetActive(false);
    }
}