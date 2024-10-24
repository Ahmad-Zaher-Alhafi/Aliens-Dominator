using System;
using System.Collections;
using Cinemachine;
using Context;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace ManagersAndControllers {
    public class CameraController : NetworkBehaviour {
        public enum CameraType {
            General,
            Player,
            TopDown
        }

        [Header("Cameras")]
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private CinemachineVirtualCamera generalCamera;
        [SerializeField] private CinemachineVirtualCamera playerFollowCamera;
        [SerializeField] private CinemachineVirtualCamera topDownCamera;

        [Header("Blend times")]
        [SerializeField] private float betweenPlayerAndGeneralCamerasBlendTime = 1f;
        [SerializeField] private float betweenPlayerAndTopDownCamerasBlendTime = .5f;

        public bool IsBlending => cinemachineBrain.IsBlending;
        public Camera LocalActiveCamera => cinemachineBrain.OutputCamera;

        private CameraType currentCameraType;

        public void SwitchToGeneralCamera() {
            cinemachineBrain.m_DefaultBlend.m_Time = betweenPlayerAndGeneralCamerasBlendTime;
            DisableAllCameras();
            StartCoroutine(SwitchToGeneralCameraDelayed());
        }

        private IEnumerator SwitchToGeneralCameraDelayed() {
            yield return new WaitForEndOfFrame();
            DisableAllCameras();
            generalCamera.enabled = true;
            currentCameraType = CameraType.General;
        }

        public void SwitchToPlayerCamera() {
            cinemachineBrain.m_DefaultBlend.m_Time = currentCameraType switch {
                CameraType.General => betweenPlayerAndGeneralCamerasBlendTime,
                CameraType.Player => betweenPlayerAndGeneralCamerasBlendTime,
                CameraType.TopDown => betweenPlayerAndTopDownCamerasBlendTime,
                _ => throw new ArgumentOutOfRangeException(nameof(currentCameraType), currentCameraType, "Unknown camera switching from.")
            };

            DisableAllCameras();
            playerFollowCamera.enabled = true;
            currentCameraType = CameraType.Player;
        }

        public void SwitchToTopDownCamera() {
            cinemachineBrain.m_DefaultBlend.m_Time = betweenPlayerAndTopDownCamerasBlendTime;
            DisableAllCameras();
            topDownCamera.enabled = true;
            currentCameraType = CameraType.TopDown;
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