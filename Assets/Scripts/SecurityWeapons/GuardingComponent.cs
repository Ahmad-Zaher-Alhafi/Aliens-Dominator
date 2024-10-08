using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SecurityWeapons {
    public class GuardingComponent : MonoBehaviour {
        [SerializeField] private float guardingSpeed = 10;

        private Quaternion initialRotation;
        private Vector2 rotateOnXAxisRange;
        private Vector2 rotateOnYAxisRange;
        private Action onFinishCallBack;
        private Quaternion? targetRotation;

        public void Init(Quaternion initialRotation, Vector2 rotateOnXAxisRange, Vector2 rotateOnYAxisRange, Action onFinishCallBack = null) {
            this.initialRotation = initialRotation;
            this.rotateOnXAxisRange = rotateOnXAxisRange;
            this.rotateOnXAxisRange = rotateOnYAxisRange;
            this.onFinishCallBack = onFinishCallBack;
        }


        // To let the weapon rotates randomly (escorting area)
        public void Guard() {
            if (targetRotation == null) {
                GenerateRandomRotation();
            }

            if (targetRotation == null) return;

            if (transform.rotation != targetRotation) {
                // Smoothly rotate towards the target rotation
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation.Value, guardingSpeed * Time.deltaTime);
            } else {
                onFinishCallBack?.Invoke();
                targetRotation = null;
            }
        }

        private void GenerateRandomRotation() {
            Vector3 initialEulerAngels = initialRotation.eulerAngles;

            // Get random angel on X axis
            Vector3 targetEulerAngels = Vector3.right * (initialEulerAngels.x + Random.Range(rotateOnYAxisRange.x, rotateOnYAxisRange.y));
            // Get random angel on Y axis
            targetEulerAngels += Vector3.up * (initialEulerAngels.y + Random.Range(rotateOnXAxisRange.x, rotateOnXAxisRange.y));
            // Give the same angel on Z axis
            targetEulerAngels += Vector3.forward * initialRotation.eulerAngles.z;

            // Return it as quaternion to avoid the gimbal lock of the euler angels
            targetRotation = Quaternion.Euler(targetEulerAngels);
        }
    }
}