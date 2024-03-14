using System.Collections;
using Cinemachine;
using Context;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace ManagersAndControllers {
    public class CameraController : NetworkBehaviour {
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private CinemachineVirtualCamera generalCamera;
        [SerializeField] private CinemachineVirtualCamera playerFollowCamera;

        public bool IsBlending => cinemachineBrain.IsBlending;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            SwitchToPlayerCamera();
        }

        public void SwitchToGeneralCamera() {
            // We need to switch to the playerFollowCamera first to prevent the blend interrupt when the player's camera gets destroyed
            playerFollowCamera.enabled = true;
            StartCoroutine(SwitchToGeneralCameraDelayed());
        }

        private IEnumerator SwitchToGeneralCameraDelayed() {
            yield return new WaitForEndOfFrame();
            generalCamera.enabled = true;
            playerFollowCamera.enabled = false;
        }

        private void SwitchToPlayerCamera() {
            generalCamera.enabled = false;
            playerFollowCamera.enabled = false;
        }

        private void Update() {
            if (!IsSpawned) return;
            if (Ctx.Deps.GameController.Player == null) return;

            playerFollowCamera.transform.position = Ctx.Deps.GameController.Player.PlayerCamera.transform.position;
            playerFollowCamera.transform.rotation = Ctx.Deps.GameController.Player.PlayerCamera.transform.rotation;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(CameraController))]
        public class GameControllerEditor : Editor {
            public override void OnInspectorGUI() {
                base.OnInspectorGUI();

                CameraController cameraController = (CameraController) target;

                if (!Application.isPlaying) {
                    EditorGUILayout.HelpBox("Editor content is shown only in play mode", MessageType.Info);
                    return;
                }

                if (GUILayout.Button("Switch to general camera")) {
                    cameraController.SwitchToGeneralCamera();
                }
            }
        }
#endif
    }
}