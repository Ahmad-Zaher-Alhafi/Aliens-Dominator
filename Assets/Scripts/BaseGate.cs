using UnityEngine;
public class BaseGate : MonoBehaviour {
    private Rigidbody rig;

    private void Start() {
        rig = GetComponent<Rigidbody>();
    }

    public void ApplyHitMotion(float force) {
        rig.AddForce(Vector3.forward * force, ForceMode.Acceleration);
    }
}