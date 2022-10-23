using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This classed is used for multiple arrow type, so one can specify the rotation offset and the arrows it should shoot
/// </summary>
[System.Serializable]
public class ArrowSetting
{
    public Vector3 RotationOffset;
    public ArrowBase Arrow;
}

public class MultipleArrow : ArrowBase
{
    public List<ArrowSetting> ArrowSettings = new List<ArrowSetting>();

    protected override void OnCollisionEnter(Collision collision)
    {
        if (hasCollided)
        {
            return;
        }

        hasCollided = true;
        transform.SetParent(collision.transform);
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        if(GetComponent<Rigidbody>()) 
            GetComponent<Rigidbody>().isKinematic = true;

        trail.enabled = false;
        DisableColliders();

        var target = collision.gameObject.GetComponent<Hitable>();
        if (target != null)
        {
            target.HandleArrowHit(this);
        }
        else
        {
            audio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Count)]);
        }

        StartCoroutine(DestroyArrow());
    }
}
