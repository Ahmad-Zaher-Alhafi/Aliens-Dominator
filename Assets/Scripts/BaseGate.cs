using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGate : MonoBehaviour
{
    private Rigidbody rig;
 
    void Start()
    {
        rig = GetComponent<Rigidbody>();
    }

    public void ApplyHitMotion(float force)
    {
        rig.AddForce(Vector3.forward * force, ForceMode.Acceleration);
    }
}