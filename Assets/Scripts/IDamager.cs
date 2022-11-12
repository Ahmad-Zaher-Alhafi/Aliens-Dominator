using UnityEngine;
public interface IDamager {
    public int Damage { get; }
    public Transform Transform { get; }
    public GameObject GameObject { get; }
}