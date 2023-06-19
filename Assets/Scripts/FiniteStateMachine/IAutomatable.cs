using System;

namespace FiniteStateMachine {
    public interface IAutomatable {
        public Enum CurrentStateType { get; }
    }
}