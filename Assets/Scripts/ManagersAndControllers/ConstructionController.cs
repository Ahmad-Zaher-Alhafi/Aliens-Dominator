using System.Collections.Generic;
using System.Linq;
using Context;
using Placeables;
using UnityEngine;

namespace ManagersAndControllers {
    public class ConstructionController : MonoBehaviour {
        [SerializeField] private Transform topDownUI;
        [SerializeField] private Vector3 constructionPanelPositionOffset = Vector3.forward * 10;

        private List<WeaponConstructionPoint> weaponConstructionPoints = new();

        private void Awake() {
            weaponConstructionPoints = FindObjectsOfType<WeaponConstructionPoint>().ToList();
            CreateWeaponConstructionPanels(weaponConstructionPoints.Select(transform1 => Ctx.Deps.CameraController.LocalActiveCamera.WorldToScreenPoint(transform1.transform.position) + constructionPanelPositionOffset).ToList());
        }

        private void CreateWeaponConstructionPanel(Vector3 constructionPointPosition) {
            WeaponConstructionPanelPlaceable weaponConstructionPanelPlaceable = new WeaponConstructionPanelPlaceable();
            Ctx.Deps.PlaceablesController.Place<WeaponConstructionPanel>(weaponConstructionPanelPlaceable, constructionPointPosition, topDownUI);
        }

        private void CreateWeaponConstructionPanels(List<Vector3> constructionPointPositions) {
            foreach (Vector3 constructionPointPosition in constructionPointPositions) {
                CreateWeaponConstructionPanel(constructionPointPosition);
            }
        }
    }
}