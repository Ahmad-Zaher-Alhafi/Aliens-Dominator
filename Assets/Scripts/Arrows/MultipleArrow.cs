using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Arrows {
    /// <summary>
    ///     This classed is used for multiple arrow type, so one can specify the rotation offset and the arrows it should shoot
    /// </summary>
    [Serializable]
    public class ArrowSetting {
        public Vector3 RotationOffset;
        public Arrow Arrow;
    }

    public class MultipleArrow : Arrow {
        public List<ArrowSetting> ArrowSettings = new();

        protected override void OnCollisionEnter(Collision collision) {
            base.OnCollisionEnter(collision);
            if (hasCollided) return;

            hasCollided = true;
            transform.SetParent(collision.transform);
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            if (GetComponent<Rigidbody>())
                GetComponent<Rigidbody>().isKinematic = true;

            trail.enabled = false;

            var target = collision.gameObject.GetComponent<Hitable>();
            if (target != null) target.HandleArrowHit(this);

            StartCoroutine(DestroyArrow());
        }
    }
}