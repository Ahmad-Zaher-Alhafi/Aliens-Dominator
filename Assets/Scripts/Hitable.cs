using Arrows;
using UnityEngine;
public abstract class Hitable : MonoBehaviour {
    public abstract void HandleArrowHit(Arrow arrow);
}