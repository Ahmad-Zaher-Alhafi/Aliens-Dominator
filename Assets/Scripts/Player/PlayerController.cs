using Arrows;
using FMODUnity;
using UnityEngine;

namespace Player {
    public class PlayerController : MonoBehaviour {
        [SerializeField] private float lookSpeed = 3;
        [SerializeField] private float verticalClamp = 45;

        [SerializeField] private Transform mainCamera;
        
        [SerializeField] private Arrow defaultArrow;
        [SerializeField] private Transform arrowSpawnPoint;
        
        [SerializeField] private Transform bow;
        [SerializeField] private float maxBowMovement = 0.4f;

        [SerializeField] private StudioEventEmitter releaseSound;
        [SerializeField] private StudioEventEmitter drawSound;
        
        [SerializeField] private PlayerTeleportObject currentPlayerTeleportObject;

        private Arrow arrow;
        private float draw;
        private Vector3 initialDrawPosition;
        private float initialYRotation;
        private Vector2 rotation = Vector2.zero;

        public PlayerTeleportObject CurrentPlayerTeleportObject {
            get => currentPlayerTeleportObject;
            set => currentPlayerTeleportObject = value;
        }

        private void Start() {
            initialYRotation = transform.eulerAngles.y;
        }

        public void Update() {
            LookUpdate();
            DrawUpdate();
        }

        private void LookUpdate() {
            float yChange = Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.x -= yChange;
            rotation.x = Mathf.Clamp(rotation.x, -verticalClamp, verticalClamp);

            float xChange = Input.GetAxis("Mouse X") * lookSpeed;
            rotation.y += xChange;

            transform.eulerAngles = new Vector2(0, rotation.y + initialYRotation);
            mainCamera.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        }
        
        private void DrawUpdate() {
            if (Input.GetButton("Fire1")) {
                if (arrow == null) {
                    arrow = defaultArrow.GetObject<DefaultArrow>(arrowSpawnPoint);
                    arrow.Init(arrowSpawnPoint);
                    drawSound.Play();
                }

                draw = Mathf.Clamp(draw + Time.deltaTime, 0, 1);
                arrow.transform.position = arrowSpawnPoint.position;

                // Whenever we draw, we want the bow to move as well
                var bowLocalPosition = bow.localPosition;
                float z = Mathf.Clamp(bowLocalPosition.z + draw, -maxBowMovement, maxBowMovement);
                bowLocalPosition = Vector3.Lerp(bowLocalPosition, new Vector3(bowLocalPosition.x, bowLocalPosition.y, z), Time.deltaTime * 4f);
                bow.localPosition = bowLocalPosition;
            } else if (Input.GetButtonUp("Fire1")) {
                if (arrow == null) return;

                arrow.Fire(draw);
                arrow = null;
                draw = 0;
                releaseSound.Play();
            }

            // Reset the bow position if its not already reset
            if (bow.localPosition.z != 0f) {
                bow.localPosition = Vector3.Lerp(bow.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }
        }
    }
}