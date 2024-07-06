using UnityEngine;
public class BowLaserHitPoint : MonoBehaviour {
    public static BowLaserHitPoint Instance;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Instance.gameObject.SetActive(false);
    }
}