using FiniteStateMachine;
using Multiplayer;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace SecurityWeapons {
    public class AutomationCommander : NetworkBehaviour {
        [SerializeField] private TextMeshProUGUI activationStateText;

        private IAutomatable automatableToCommand;
        private GameObject lastArrowHitWith;
        private readonly NetworkVariable<SerializedNetworkString> status = new(new SerializedNetworkString(""));

        public void Init(IAutomatable automatableToCommand, bool activateOnStart) {
            this.automatableToCommand = automatableToCommand;
            automatableToCommand.IsAutomatingEnabled = activateOnStart;
        }

        private void Update() {
            SetActivationStateText();
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.gameObject.CompareTag(Constants.Arrow) || other.gameObject == lastArrowHitWith) return;
            lastArrowHitWith = other.gameObject;

            if (IsServer) {
                automatableToCommand.IsAutomatingEnabled = !automatableToCommand.IsAutomatingEnabled;
            } else {
                ChangeAutomationStatusServerRPC(!automatableToCommand.IsAutomatingEnabled);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeAutomationStatusServerRPC(bool hasToActivate) {
            automatableToCommand.IsAutomatingEnabled = hasToActivate;
        }

        private void SetActivationStateText() {
            if (IsServer) {
                activationStateText.text = automatableToCommand.IsAutomatingEnabled ? "Enabled" : "Disabled";
                status.Value = new SerializedNetworkString(activationStateText.text);
            } else {
                activationStateText.text = status.Value.Value;
            }
        }
    }
}