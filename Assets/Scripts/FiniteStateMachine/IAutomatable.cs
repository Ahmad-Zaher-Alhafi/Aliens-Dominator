using UnityEngine;

namespace FiniteStateMachine {
    public interface IAutomatable {
        GameObject GameObject { get; }
        bool IsDestroyed { get; }
        bool IsAutomatingEnabled { get; set; }
    }
}