using System;
using DG.Tweening;
using QuickOutline.Scripts;
using SecurityWeapons;
using UnityEngine;
public class WeaponPlaceholder : MonoBehaviour {
    public static WeaponPlaceholder Instance { get; private set; }

    [Header("Placeholders")]
    [SerializeField] private GameObject groundWeaponMesh;
    [SerializeField] private GuardingComponent groundGuardingComponent;

    [SerializeField] private GameObject airWeaponMesh;
    [SerializeField] private GuardingComponent airGuardingComponent;

    [SerializeField] private GameObject fighterPlaneMesh;

    [Header("Others")]
    [SerializeField] private float outlineBlinkSpeed = .5f;
    [SerializeField] private RangeVisualizer rangeVisualizer;

    public Quaternion ActivePlaceholderRotation => activeMesh != null ? activeMesh.transform.rotation : Quaternion.identity;

    private GameObject activeMesh;
    private GuardingComponent activeGuardingComponent;
    private Sequence blinkTween;
    private Outline activeMeshOutline;

    private void Awake() {
        if (Instance is not null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update() {
        if (activeMesh != null && activeMesh.activeInHierarchy) {
            activeGuardingComponent.Guard();
        }
    }

    public void ShowPlaceholder(DefenceWeapon.WeaponsType weaponType, Vector3 position, Quaternion rotation) {
        HidePlaceholder();

        switch (weaponType) {
            case DefenceWeapon.WeaponsType.Ground:
                groundWeaponMesh.SetActive(true);
                activeMesh = groundWeaponMesh;
                activeGuardingComponent = groundGuardingComponent;
                break;
            case DefenceWeapon.WeaponsType.Air:
                airWeaponMesh.SetActive(true);
                activeMesh = airWeaponMesh;
                activeGuardingComponent = airGuardingComponent;
                break;
            case DefenceWeapon.WeaponsType.FighterPlane:
                fighterPlaneMesh.SetActive(true);
                activeMesh = fighterPlaneMesh;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(weaponType), weaponType, null);
        }

        activeGuardingComponent?.Init(rotation, SharedWeaponSpecifications.Instance.GetWeaponRotateOnYAxisRange(weaponType), SharedWeaponSpecifications.Instance.GetWeaponRotateOnXAxisRange(weaponType));

        activeMesh.transform.position = position;
        activeMesh.transform.rotation = rotation;

        activeMeshOutline = activeMesh.GetComponent<Outline>();
        activeMeshOutline.enabled = false;
        activeMeshOutline.OutlineColor = SharedWeaponSpecifications.Instance.SelectionOutlineColor;

        blinkTween.Kill();
        blinkTween = DOTween.Sequence()
            .AppendCallback(() => {
                activeMeshOutline.enabled = !activeMeshOutline.enabled;
                if (activeMeshOutline.enabled) {
                    rangeVisualizer.ShowRange(position, SharedWeaponSpecifications.Instance.GetWeaponRange(weaponType));
                } else {
                    rangeVisualizer.HideRange();
                }
            }).AppendInterval(outlineBlinkSpeed)
            .SetLoops(-1); // Wait for 1 second;
    }

    public void HidePlaceholder() {
        activeMesh?.SetActive(false);
        blinkTween.Kill();

        rangeVisualizer.HideRange();
    }

    private void OnDestroy() {
        blinkTween.Kill();
        blinkTween = null;
    }
}