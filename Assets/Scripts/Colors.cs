using UnityEngine;
public class Colors : MonoBehaviour {
    public static Colors Instance;

    [SerializeField] private Color normal;
    public Color Normal => normal;

    [SerializeField] private Color error;
    public Color Error => error;

    [SerializeField] private Color blueUI;
    public Color BlueUI => blueUI;

    [SerializeField] private Color redUI;
    public Color RedUI => redUI;

    private void Awake() {
        if (Instance != null) {
            Destroy(Instance);
            return;
        }

        Instance = this;
    }
}