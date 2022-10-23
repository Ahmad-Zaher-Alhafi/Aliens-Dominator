using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HypnotizeArrow : ArrowBase
{
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
            Creature creature = target.GetComponentInParent<Creature>();

            if(creature.gameObject.tag == "OnStartWaves")
            {
                target.HandleArrowHit(this);
                return;
            }

            creature.Hypnotize(125f);
        }
        else
        {
            audio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Count)]);
        }

        StartCoroutine(DestroyArrow());
    }
}
