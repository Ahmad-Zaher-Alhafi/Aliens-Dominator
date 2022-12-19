using System.Collections.Generic;
using Arrows;
using ManagersAndControllers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player {
    public class ArcheryRig : MonoBehaviour {
        public bool WasGameStarted;
        [FormerlySerializedAs("GameHandler")]
        public GameController gameController;

        [SerializeField] private float shotForce = 50;
        [SerializeField] private float moveForce = 1;

        public float lookSpeed = 3;
        [SerializeField] private Vector2 rotation = Vector2.zero;
        [SerializeField] private float verticalClamp = 45;
        [SerializeField] private float horizontalClamp = 90;
        [SerializeField] private Transform verticalPivot;
        [SerializeField] private ArrowBase DefaultArrow;
        [SerializeField] private ArrowBase NewArrow;
        [SerializeField] private Vector3 localFullDrawOffset = Vector3.forward;
        [SerializeField] private Transform arrowSpawnPoint;
        [SerializeField] private Transform bowTransform;
        [SerializeField] private float MaxBowMovement = 0.4f;

        public AudioClip releaseSound;
        [SerializeField] private AudioClip drawSound;

        [SerializeField] private PlayerTeleportObject currentPlayerTeleportObjectt;
        private ArrowBase arrow;
        private readonly Queue<ArrowBase> Arrows = new();

        private new AudioSource audio;

        private Vector3 BowStartPos;
        private float draw;
        private Vector3 initialDrawPosition;
        private float initialYRotation;

        public static ArcheryRig Instance { get; private set; }

        public int ArrowsShot { get; private set; }
        public bool AllowInput { get; set; } = true;

        public PlayerTeleportObject CurrentPlayerTeleportObject {
            get => currentPlayerTeleportObjectt;
            set => currentPlayerTeleportObjectt = value;
        }

        private void Awake() {
            Instance = this;

            BowStartPos = bowTransform.position;
            audio = GetComponent<AudioSource>();
        }

        private void Start() {
            initialYRotation = transform.eulerAngles.y;
        }

        public void Update() {
            if (AllowInput) {
                LookUpdate();
                DrawUpdate();
            }
        }

        private float Mod(float x, float m) {
            return (x % m + m) % m;
        }

        private void DrawUpdate() {
            if (!WasGameStarted) return;

            ArrowBase arrowBase = GetArrow();

            if (Input.GetButton("Fire1")) {
                if (arrow == null) {
                    arrow = DefaultArrow.GetObject<DefaultArrow>(arrowSpawnPoint);
                    arrow.transform.localPosition = Vector3.zero;
                    arrow.transform.rotation = arrowSpawnPoint.transform.rotation;
                    arrow.Knock();
                    audio.PlayOneShot(drawSound);
                }

                draw = Mathf.Clamp(draw + Time.deltaTime, 0, 1);
                arrow.transform.position = arrowSpawnPoint.position;

                //Whenever we draw, we want the bow to move as well
                float z = Mathf.Clamp(bowTransform.localPosition.z + draw, -MaxBowMovement, MaxBowMovement);
                bowTransform.localPosition = Vector3.Lerp(bowTransform.localPosition, new Vector3(bowTransform.localPosition.x, bowTransform.localPosition.y, z), Time.deltaTime * 4f);

                return;
            }
            if (Input.GetButtonUp("Fire1") && arrow != null) {
                ArrowsShot += 1;

                HandleMultipleArrows(arrowBase);

                arrow.Loose(draw);

                arrow.transform.SetParent(null);
                arrow = null;

                draw = 0;
                audio.PlayOneShot(releaseSound);
            }

            //Reset the bow position if its not already reset
            if (bowTransform.localPosition.z != 0f) bowTransform.localPosition = Vector3.Lerp(bowTransform.localPosition, Vector3.zero, Time.deltaTime * 10f);
        }

        /// <summary>
        ///     Check if the Arrow is of type "MultipleArrow" and instantiate the arrows specified in the settings
        /// </summary>
        /// <param name="arrowBase"></param>
        private void HandleMultipleArrows(ArrowBase arrowBase) {
            if (!(arrow is MultipleArrow)) return;

            var _arrow = arrowBase as MultipleArrow;

            _arrow.ArrowSettings.ForEach(setting => {
                if (setting.Arrow) {
                    var mArrow = gameController.PoolManager.GetArrow(setting.Arrow, arrowSpawnPoint.position, Quaternion.Euler(arrowSpawnPoint.transform.rotation.eulerAngles + setting.RotationOffset)).GetComponent<ArrowBase>();
                    mArrow.transform.SetParent(arrowSpawnPoint);
                    mArrow.Knock();

                    mArrow.Loose(draw);
                    mArrow.transform.SetParent(null);
                } else {
                    Debug.LogError($"Setting: Arrow is not defined at index {_arrow.ArrowSettings.FindIndex(a => a == setting)}");
                }
            });
        }


        private void LookUpdate() {
            float yChange = Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.x -= yChange;
            rotation.x = Mathf.Clamp(rotation.x, -verticalClamp, verticalClamp);

            float xChange = Input.GetAxis("Mouse X") * lookSpeed;
            rotation.y += xChange;
            rotation.y = Mathf.Clamp(rotation.y, -horizontalClamp, horizontalClamp);

            transform.eulerAngles = new Vector2(0, rotation.y + initialYRotation);
            verticalPivot.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        }
        public void AddArrow(ArrowBase arrow) {
            Arrows.Enqueue(arrow);
        }

        private ArrowBase GetArrow() {
            if (Arrows.Count <= 0)
                return DefaultArrow;

            return Arrows.Dequeue();
        }

        public void UpdateArrow() {
            DefaultArrow = NewArrow;
        }
    }
}