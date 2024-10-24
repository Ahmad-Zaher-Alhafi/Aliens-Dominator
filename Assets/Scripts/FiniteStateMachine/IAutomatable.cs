﻿using UnityEngine;

namespace FiniteStateMachine {
    public interface IAutomatable {
        public GameObject GameObject { get; }
        public bool IsDestroyed { get; }
    }
}