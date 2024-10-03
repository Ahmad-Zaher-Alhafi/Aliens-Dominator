using System.Collections;
using Cinemachine;
using Context;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace ManagersAndControllers {
    public class CameraController : NetworkBehaviour {
        [Header("Cameras")]
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private CinemachineVirtualCamera generalCamera;
        [SerializeField] private CinemachineVirtualCamera playerFollowCamera;
        [SerializeField] private CinemachineVirtualCamera topDownCamera;

        [Header("Blend times")]
        [SerializeField] private float betweenPlayerAndGeneralCamerasBlendTime = 1f;
        [SerializeField] private float betweenPlayerAndTopDownCamerasBlendTime = .5f;

        public bool IsBlending => cinemachineBrain.IsBlending;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            SwitchToPlayerCamera(betweenPlayerAndGeneralCamerasBlendTime);
        }

        public void SwitchToGeneralCamera() {
            cinemachineBrain.m_DefaultBlend.m_Time = betweenPlayerAndGeneralCamerasBlendTime;
            DisableAllCameras();
            // We need to switch to the playerFollowCamera first to prevent the blend interrupt when the player's camera gets destroyed
            playerFollowCamera.enabled = true;
            StartCoroutine(SwitchToGeneralCameraDelayed());
        }

        private IEnumerator SwitchToGeneralCameraDelayed() {
            yield return new WaitForEndOfFrame();
            DisableAllCameras();
            generalCamera.enabled = true;
        }

        private void SwitchToPlayerCamera(float blendTime) {
            cinemachineBrain.m_DefaultBlend.m_Time = blendTime;
            DisableAllCameras();
            playerFollowCamera.enabled = true;
        }

        private void SwitchToTopDownCamera() {
            cinemachineBrain.m_DefaultBlend.m_Time = betweenPlayerAndTopDownCamerasBlendTime;
            DisableAllCameras();
            topDownCamera.enabled = true;
        }

        private void DisableAllCameras() {
            generalCamera.enabled = false;
            playerFollowCamera.enabled = false;
            topDownCamera.enabled = false;
        }

        private void Update() {
            if (!IsSpawned) return;
            if (Ctx.Deps.GameController.Player == null) return;

            playerFollowCamera.transform.position = Ctx.Deps.GameController.Player.PlayerCamera.transform.position;
            playerFollowCamera.transform.rotation = Ctx.Deps.GameController.Player.PlayerCamera.transform.rotation;

            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                SwitchToTopDownCamera();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                SwitchToPlayerCamera(betweenPlayerAndTopDownCamerasBlendTime);
            }
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