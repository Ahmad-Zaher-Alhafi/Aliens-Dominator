using System;
using System.Collections.Generic;
using UnityEngine;

namespace Creatures {
    public class RagdollRewind : MonoBehaviour {
        public bool DoneRecodring;
        public List<Transform> bones;
        public List<KeyFrame> frames;
        [SerializeField]
        private float BodyPartsInterpDuration = .5f;
        [SerializeField]
        private float RootInterpDuration = .5f;
        private int bytes; // count bytes
        private Creature creature;
        private bool doRewind;
        private float duration;
        private bool enableRecording;
        private List<AnimStruct> interpList;
        private bool interpolating;
        private float interpStartTime;
        private float recordDurationDelta; // delta time for the ticking of the recording
        private float recordDurationTime; // target time for the duration of the recording
        private float recordFrameSkipDelta;
        private float recordFrameSkipTime;
        private Rigidbody rig;
        private bool rootInterpolating;
        private Rigidbody rootRig;
        private float rootStartTime;
        private Quaternion rootTargetLocalRotaion;
        private Vector3 rootTargetPosition;

        private void Start() {
            rootRig = transform.root.GetComponent<Rigidbody>();
            rig = transform.GetComponent<Rigidbody>();
            BodyPartsInterpDuration = .5f;
            RootInterpDuration = .005f;
            creature = transform.root.GetComponent<Creature>();
            bones = new List<Transform>();
            GetChildrenRecursive(transform);
        }

        private void LateUpdate() {
            if (enableRecording) {
                if (recordDurationDelta < recordDurationTime) {
                    if (recordFrameSkipDelta > recordFrameSkipTime) {
                        var kf = new KeyFrame();
                        foreach (Transform t in bones) {
                            kf.Add(t.localPosition, t.localRotation);
                            bytes += 28;
                        }

                        frames.Add(kf);
                        recordFrameSkipDelta = 0;
                    }

                    recordFrameSkipDelta += Time.deltaTime;
                    recordDurationDelta += Time.deltaTime;
                    return;
                }

                //Debug.Log("RECORDED " + frames.Count + " " + bytes + " bytes");
                enableRecording = false;
                DoneRecodring = true;
            }

            if (doRewind && !interpolating && DoneRecodring) {
                KeyFrame kf = frames[frames.Count - 1];
                frames.Remove(kf);
                interpStartTime = Time.time;
                interpList = new List<AnimStruct>();

                foreach (Transform t in bones) {
                    var interp = new AnimStruct(t.localPosition, kf.positions[bones.IndexOf(t)], t.localRotation, kf.rotations[bones.IndexOf(t)]);
                    interpList.Add(interp);
                }

                interpolating = true;

                if (frames.Count < 1) {
                    doRewind = false;
                    recordFrameSkipDelta = 0;
                    recordDurationDelta = 0;
                }
            } else if (interpolating) {
                float t = (Time.time - interpStartTime) / BodyPartsInterpDuration;
                if (t > 1) {
                    interpolating = false;

                    foreach (Transform tr in bones) {
                        tr.localPosition = interpList[bones.IndexOf(tr)].targetPosition;
                        tr.localRotation = interpList[bones.IndexOf(tr)].targetRotation;
                    }
                } else {
                    foreach (Transform tr in bones) {
                        tr.localPosition = Vector3.Slerp(interpList[bones.IndexOf(tr)].startPosition, interpList[bones.IndexOf(tr)].targetPosition, t);
                        tr.localRotation = Quaternion.Slerp(interpList[bones.IndexOf(tr)].startRotation, interpList[bones.IndexOf(tr)].targetRotation, t);
                    }
                }
            }

            if (rootInterpolating) {
                //float t = (Time.time - rootStartTime) / RootInterpDuration;

                if (Vector3.Distance(transform.position, rootTargetPosition) < .2f && Quaternion.Angle(transform.localRotation, rootTargetLocalRotaion) < 1f) {
                    //print("Finished rotating and positioning");
                    rootInterpolating = false;
                    transform.position = rootTargetPosition;
                    transform.localRotation = rootTargetLocalRotaion;
                } else {
                    if (Quaternion.Angle(transform.localRotation, rootTargetLocalRotaion) > 1f) transform.localRotation = Quaternion.Slerp(transform.localRotation, rootTargetLocalRotaion, RootInterpDuration);
                }
            }
        }


        // seconds is the time window in which frames will be stored for animation
        // delta is how many milliseconds will pass beetween one recording and the other
        public void StartRecording(float seconds, float delta, Animator animator) {
            rootStartTime = Time.time;
            rootTargetLocalRotaion = transform.localRotation;
            rootTargetPosition = transform.position;
            animator.enabled = false;
            DoneRecodring = false;
            frames = new List<KeyFrame>();
            enableRecording = true;
            duration = seconds;
            recordDurationTime = seconds;
            recordFrameSkipTime = delta;
            recordFrameSkipDelta = recordFrameSkipTime + 1;
        }

        public void DoRewind() {
            rootInterpolating = true;
            rootRig.isKinematic = true;
            rig.isKinematic = true;
            rootRig.useGravity = false;
            rig.useGravity = false;

            foreach (Transform t in bones) {
                var rb = t.GetComponent<Rigidbody>();
                if (rb != null) {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }

            doRewind = true;
        }

        private void GetChildrenRecursive(Transform t) {
            if (t == null) return;

            foreach (Transform child in t) {
                if (child == null) continue;

                bones.Add(child);
                GetChildrenRecursive(child);
            }
        }
    }

// FOR ALL THE CHILDREN
    [Serializable]
    public class KeyFrame {
        public List<Vector3> positions = new();
        public List<Quaternion> rotations = new();

        public void Add(Vector3 position, Quaternion rotation) {
            positions.Add(position);
            rotations.Add(rotation);
        }
    }

    public class AnimStruct {
        public Vector3 startPosition;
        public Quaternion startRotation;
        public Vector3 targetPosition;
        public Quaternion targetRotation;

        public AnimStruct(Vector3 startPosition, Vector3 targetPosition, Quaternion startRotation, Quaternion targetRotation) {
            this.startPosition = startPosition;
            this.targetPosition = targetPosition;
            this.startRotation = startRotation;
            this.targetRotation = targetRotation;
        }
    }
}