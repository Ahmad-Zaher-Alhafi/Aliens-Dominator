using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public abstract class State<T> where T : IAutomatable {
        public bool IsActive { get; private set; }

        protected readonly T StateObject;

        protected State(T stateObject) {
            StateObject = stateObject;
        }

        public virtual void Activate() {
            Debug.Log($"State {StateObject.CurrentStateType} of {StateObject} was activated");
            IsActive = true;
        }

        public virtual void Fulfil() {
            Debug.Log($"State {StateObject.CurrentStateType} of {StateObject} was deactivated");
            IsActive = false;
        }

        /// <summary>
        /// Deactivate the state even if it's not finished
        /// </summary>
        public virtual void Interrupt() {
            Debug.Log($"State {StateObject.CurrentStateType} of {StateObject} was Interrupted");
            IsActive = false;
            Clear();
        }

        public virtual void Tick() { }

        public abstract bool CanBeActivated();

        protected virtual void Clear() {
            IsActive = false;
        }
    }
}