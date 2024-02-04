using FiniteStateMachine;
using TMPro;
using UnityEngine;

namespace SecurityWeapons {
    public class AutomationCommander : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI activationStateText;

        private IAutomatable automatableToCommand;
        private GameObject lastArrowHitWith;


        public void Init(IAutomatable automatableToCommand) {
            this.automatableToCommand = automatableToCommand;
            SetActivationStateText();
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.gameObject.CompareTag(Constants.Arrow) || other.gameObject == lastArrowHitWith) return;
            lastArrowHitWith = other.gameObject;
            automatableToCommand.IsAutomatingEnabled = !automatableToCommand.IsAutomatingEnabled;
            SetActivationStateText();
        }

        private void SetActivationStateText() {
            activationStateText.text = automatableToCommand.IsAutomatingEnabled ? "Enabled" : "Disabled";
        }
    }
}