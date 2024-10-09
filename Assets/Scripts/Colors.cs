using UnityEngine;
public class Colors : MonoBehaviour {
    public static Colors Instance;

    [SerializeField] private Color normal;
    public Color Normal => normal;

    [SerializeField] private Color error;
    public Color Error => error;

    private void Awake() {
        if (Instance != null) {
            Destroy(Instance);
            return;
        }

        Instance = this;
    }
}