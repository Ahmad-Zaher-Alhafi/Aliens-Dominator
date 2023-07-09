using TMPro;
using UnityEngine;

namespace SecurityWeapons {
    public class WeaponCommander : MonoBehaviour {
        [SerializeField] private SecurityWeapon weaponToComand;
        [Header("Only for fighter airplane")]
        [SerializeField] private FighterPlane airplaneToComand;
        [SerializeField] private bool isItWeaponChanger; //if it was the responsible of changeing the weapon that the airplane use
        [Header("For Weapon Changer Only")]
        [SerializeField] private TextMeshProUGUI weaponStateText;
        [SerializeField] private bool hasToDefend;

        [Space]
        private bool hasToUseRockets;

        private void Start() {
            hasToUseRockets = false;

            //if (weaponToComand != null) weaponToComand.UpdateDefendingState(hasToDefend);

            if (isItWeaponChanger) UpdateWeaponStateText();
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.gameObject.CompareTag(Constants.Arrow)) return;

            if (weaponToComand != null) {
                hasToDefend = !hasToDefend;
                //weaponToComand.UpdateDefendingState(hasToDefend);
            } else if (airplaneToComand != null) {
                if (isItWeaponChanger) {
                    hasToUseRockets = !hasToUseRockets;
                    UpdateWeaponStateText();
                    airplaneToComand.SetWeaponToUse(hasToUseRockets);
                } else {
                    hasToDefend = !hasToDefend;
                    airplaneToComand.UpdateDefendingState(hasToDefend);
                }
            }

            Destroy(other.gameObject);
        }

        private void UpdateWeaponStateText() {
            if (hasToUseRockets) weaponStateText.text = "Rockets";
            else weaponStateText.text = "Bullets";
        }
    }
}