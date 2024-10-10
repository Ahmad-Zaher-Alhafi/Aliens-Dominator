using Context;
using ManagersAndControllers;
using TMPro;
using UnityEngine;
using Utils.Extensions;

namespace UI {
    public class SuppliesUI : MonoBehaviour {
        [SerializeField] private GameObject panelsHolder;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI constructionSuppliesAmountText;
        [SerializeField] private TextMeshProUGUI bulletsSuppliesAmountText;
        [SerializeField] private TextMeshProUGUI rocketsSuppliesAmountText;

        private void Update() {
            panelsHolder.SetActiveWithCheck(Ctx.Deps.GameController.CurrentViewMode != GameController.ViewMode.General);
            constructionSuppliesAmountText.text = Ctx.Deps.SuppliesController.CheckSuppliesAmount(SuppliesController.SuppliesTypes.Construction).ToString();
            bulletsSuppliesAmountText.text = Ctx.Deps.SuppliesController.CheckSuppliesAmount(SuppliesController.SuppliesTypes.BulletsAmmo).ToString();
            rocketsSuppliesAmountText.text = Ctx.Deps.SuppliesController.CheckSuppliesAmount(SuppliesController.SuppliesTypes.RocketsAmmo).ToString();
        }
    }
}