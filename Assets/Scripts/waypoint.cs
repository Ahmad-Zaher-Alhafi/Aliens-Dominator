using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waypoint : MonoBehaviour
{
   [SerializeField]
   protected float debugDrawRadius = 1.0F;
   public Color Color;
   public GameObject LookAtGO; 

   public virtual void OnDrawGizmos()
   {
       Gizmos.color = Color;
       Gizmos.DrawWireSphere(transform.position, debugDrawRadius);
   }
}
