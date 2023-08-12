using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiniteStateMachine {
    public abstract class State<TAutomatable, TType> where TAutomatable : IAutomatable where TType : Enum {
        public abstract TType Type { get; }

        public bool IsActive { get; private set; }
        public bool IsActiveAsSecondaryState { get; set; }

        protected readonly TAutomatable AutomatedObject;

        private List<State<TAutomatable, TType>> statesSyncedWith = new();
        private List<State<TAutomatable, TType>> interruptStates = new();

        protected State(TAutomatable automatedObject) {
            AutomatedObject = automatedObject;
        }

        public virtual void Activate(bool isSecondaryState = false) {
            string stateRank = isSecondaryState ? "Secondary state" : "PrimaryState";
            Debug.Log($"State {Type} of {AutomatedObject} was activated as {stateRank} with instance id {AutomatedObject.GameObject.GetInstanceID()}", AutomatedObject.GameObject);
            IsActive = true;
            IsActiveAsSecondaryState = isSecondaryState;
        }

        public virtual void Fulfil() {
            Debug.Log($"State {Type} of {AutomatedObject} was deactivated with instance id {AutomatedObject.GameObject.GetInstanceID()}", AutomatedObject.GameObject);
            IsActive = false;
        }

        /// <summary>
        /// Deactivate the state even if it's not finished
        /// </summary>
        public virtual void Interrupt() {
            Debug.Log($"State {Type} of {AutomatedObject} was Interrupted with instance id {AutomatedObject.GameObject.GetInstanceID()}", AutomatedObject.GameObject);
            IsActive = false;
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