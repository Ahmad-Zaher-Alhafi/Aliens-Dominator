using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiniteStateMachine {
    public interface IState {
        bool IsActive { get; }
        event Action<bool> StateActivationChanged;
        void Fulfil();
    }

    public abstract class State<TAutomatable, TType> : IState where TAutomatable : IAutomatable where TType : Enum {
        public abstract TType Type { get; }
        public bool IsActive {
            get => isActive;
            private set {
                isActive = value;
                stateActivationChanged?.Invoke(isActive);
            }
        }
        private bool isActive;
        public bool IsActiveAsSecondaryState => IsActive && isSecondaryState;
        public bool CheckWhenAutomatingDisabled { get; }
        public event Action<bool> StateActivationChanged {
            add => stateActivationChanged += value;
            remove => stateActivationChanged -= value;
        }
        private event Action<bool> stateActivationChanged;

        public TAutomatable AutomatedObject { get; }

        private List<State<TAutomatable, TType>> statesSyncedWith = new();
        private List<State<TAutomatable, TType>> interruptStates = new();
        private bool isSecondaryState;

        /// <summary>
        ///
        /// </summary>
        /// <param name="automatedObject"></param>
        /// <param name="checkWhenAutomatingDisabled">Set to ture to be checked in the state machine even if the automation was disabled</param>
        protected State(TAutomatable automatedObject, bool checkWhenAutomatingDisabled = false) {
            AutomatedObject = automatedObject;
            CheckWhenAutomatingDisabled = checkWhenAutomatingDisabled;
        }

        public virtual void Activate(bool isSecondaryState = false) {
            string stateRank = isSecondaryState ? "Secondary state" : "PrimaryState";
            Debug.Log($"State {Type} of {AutomatedObject.GameObject.name} was activated as {stateRank} with instance id {AutomatedObject.GameObject.GetInstanceID()}", AutomatedObject.GameObject);
            IsActive = true;
            this.isSecondaryState = isSecondaryState;
        }

        public virtual void Fulfil() {
            Debug.Log($"State {Type} of {AutomatedObject.GameObject.name} was deactivated with instance id {AutomatedObject.GameObject.GetInstanceID()}", AutomatedObject.GameObject);
            IsActive = false;
            isSecondaryState = false;
        }

        /// <summary>
        /// Deactivate the state even if it's not finished
        /// </summary>
        public virtual void Interrupt() {
            Debug.Log($"State {Type} of {AutomatedObject.GameObject.name} was Interrupted with instance id {AutomatedObject.GameObject.GetInstanceID()}", AutomatedObject.GameObject);
            IsActive = false;
            isSecondaryState = false;
        }

        public void MarkAsPrimaryState() {
            Debug.Log($"State {Type} of {AutomatedObject.GameObject.name} became a Primary state with instance id {AutomatedObject.GameObject.GetInstanceID()}");
            isSecondaryState = false;
        }

        public virtual void Tick() { }

        public abstract bool CanBeActivated();

        public void SetStatesSyncedWith(List<State<TAutomatable, TType>> statesSyncedWith) {
            this.statesSyncedWith = statesSyncedWith;
        }

        public void SetInterruptStates(List<State<TAutomatable, TType>> interruptStates) {
            this.interruptStates = interruptStates;
        }

        public bool CanBeSyncedWith(State<TAutomatable, TType> state) {
            return statesSyncedWith.Contains(state);
        }

        public bool CanInterruptState(State<TAutomatable, TType> state) {
            return interruptStates.Contains(state);
        }

        public virtual void Clear() { }
    }
}