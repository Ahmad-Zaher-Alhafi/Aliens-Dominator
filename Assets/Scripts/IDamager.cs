using UnityEngine;
public interface IDamager {
    public float Damage { get; }
    public Transform Transform { get; }
    public GameObject GameObject { get; }
}