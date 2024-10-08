using UnityEngine;

namespace FiniteStateMachine.SecurityWeaponMachine {
    public abstract class StateComponent : MonoBehaviour {
        protected IState State;

        protected virtual void Awake() { }

        public void Init(IState state) {
            State = state;
            State.StateActivationChanged += OnStateActivationChanged;
        }

        protected virtual void OnStateActivationChanged(bool isActive) { }

        private void OnDestroy() {
            State.StateActivationChanged -= OnStateActivationChanged;
        }
    }
}