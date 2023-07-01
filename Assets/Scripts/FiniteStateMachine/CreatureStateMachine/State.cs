using System.Collections.Generic;
using UnityEngine;

namespace FiniteStateMachine.CreatureStateMachine {
    public abstract class State<T> where T : IAutomatable {
        public bool IsActive { get; private set; }

        protected readonly T StateObject;

        private List<State<T>> statesSyncedWith = new();
        
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

        public void SetStatesSyncedWith(List<State<T>> statesSyncedWith) {
            this.statesSyncedWith = statesSyncedWith;
        }
        
        public bool IsSyncedWith(State<T> state) {
            return statesSyncedWith.Contains(state);
        }

        protected virtual void Clear() {
            IsActive = false;
        }
    }
}