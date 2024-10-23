using System;
using Context;
using ManagersAndControllers;
using UnityEngine;
using Utils.Extensions;

namespace UI {
    public class SuppliesUI : MonoBehaviour {
        [SerializeField] private GameObject panelsHolder;

        [Header("Texts")]
        [SerializeField] private SuppliesPanel constructionSuppliesPaenl;
        [SerializeField] private SuppliesPanel bulletsSuppliesPanel;
        [SerializeField] private SuppliesPanel rocketsSuppliesPanel;

        private void Update() {
            panelsHolder.SetActiveWithCheck(Ctx.Deps.GameController.CurrentViewMode != GameController.ViewMode.General);
            constructionSuppliesPaenl.SetText(Ctx.Deps.SuppliesController.CheckSuppliesAmount(SuppliesController.SuppliesTypes.Construction).ToString());
            bulletsSuppliesPanel.SetText(Ctx.Deps.SuppliesController.CheckSuppliesAmount(SuppliesController.SuppliesTypes.BulletsAmmo).ToString());
            rocketsSuppliesPanel.SetText(Ctx.Deps.SuppliesController.CheckSuppliesAmount(SuppliesController.SuppliesTypes.RocketsAmmo).ToString());
        }

        public void PlayInsufficientSuppliesAnimation(SuppliesController.SuppliesTypes suppliesTypes) {
            switch (suppliesTypes) {
                case SuppliesController.SuppliesTypes.Construction:
                    constructionSuppliesPaenl.PlayErrorAnimation();
                    break;
                case SuppliesController.SuppliesTypes.RocketsAmmo:
                    rocketsSuppliesPanel.PlayErrorAnimation();
                    break;
                case SuppliesController.SuppliesTypes.BulletsAmmo:
                    bulletsSuppliesPanel.PlayErrorAnimation();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(suppliesTypes), suppliesTypes, "Unknown supplies type");
            }
        }
    }
}