using Creatures;
using UnityEngine;

namespace FiniteStateMachine.States {
    public enum StateType {
        None,
        Idle,
        Patrolling,
        FollowingPath,
        ChasingTarget,
        GettingHit,
        Attacking,
        RunningAway,
        Dead,
    }

    public abstract class State {
        public abstract StateType Type { get; }

        public bool IsFinal { get; private set; }
        public bool IsActive { get; private set; }
        public virtual bool IsCinematic => false;

        public bool IsNextCinematicState;

        protected readonly Creature Creature;
        protected abstract bool WaitForMoverToFulfill { get; }
        protected abstract bool WaitForAnimatorToFulfill { get; }

        private bool hasMoverOrderFinished = true;
        private bool hasAnimationFinished = true;

        protected State(bool isFinal, Creature creature) {
            IsFinal = isFinal;
            Creature = creature;
        }

        public virtual void Activate() {
            IsActive = true;
            hasMoverOrderFinished = !WaitForMoverToFulfill;
            hasAnimationFinished = !WaitForAnimatorToFulfill;

            Debug.Log($"{Type} was activated for creature {Creature}");
        }

        public virtual void Fulfil() {
            IsActive = false;
            IsNextCinematicState = false;
            Debug.Log($"{Type} was deactivated for creature {Creature}");
        }

        /// <summary>
        /// Deactivate the state even if it's not finished
        /// </summary>
        public virtual void Interrupt() {
            IsActive = false;
            Clear();
            Debug.Log($"{Type} was Interrupted for creature {Creature}");
        }

        public virtual void Tick() { }

        public abstract bool CanBeActivated();

        protected virtual void OnMoverOrderFulfilled() {
            if (!WaitForMoverToFulfill) return;
            hasMoverOrderFinished = true;

            if (WaitForAnimatorToFulfill && !hasAnimationFinished) return;
            Fulfil();
        }

        protected virtual void OnAnimationFinished() {
            if (!WaitForAnimatorToFulfill) return;
            hasAnimationFinished = true;

            if (WaitForMoverToFulfill && !hasMoverOrderFinished) return;
            Fulfil();
        }

        protected virtual void Clear() {
            IsActive = false;
            hasMoverOrderFinished = true;
            hasAnimationFinished = true;
        }
    }
}