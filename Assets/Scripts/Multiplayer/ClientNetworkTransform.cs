﻿using Unity.Netcode.Components;

namespace Multiplayer {
    public class ClientNetworkTransform : NetworkTransform {

        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}