using UnityEngine;

public abstract class Hitable : MonoBehaviour
{
    public abstract void HandleArrowHit(ArrowBase arrow);
}