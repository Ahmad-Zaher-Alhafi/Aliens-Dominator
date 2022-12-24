using Arrows;
using UnityEngine;
public class TNT : InteractableObjectBase {
    public override void HandleArrowHit(Arrow arrow) {
        

        Destroy(gameObject);
    }
}