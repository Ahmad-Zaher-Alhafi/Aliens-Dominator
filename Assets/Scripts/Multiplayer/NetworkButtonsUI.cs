using Unity.Netcode;
using UnityEngine;

namespace Multiplayer {
    public class NetworkButtonsUI : MonoBehaviour {
        void OnGUI() {
            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
                StartButtons();
            } else {
                StatusLabels();
            }

            GUILayout.EndArea();
        }

        static void StartButtons() {
            if (GUILayout.Button("Host", GUILayout.Height(50))) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client", GUILayout.Height(50))) NetworkManager.Singleton.StartClient();
            if (GUILayout.Button("Server", GUILayout.Height(50))) NetworkManager.Singleton.StartServer();
        }

        static void StatusLabels() {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }
    }
}