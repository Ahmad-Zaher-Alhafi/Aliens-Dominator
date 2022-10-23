using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(TrailRenderer), typeof(Rigidbody), (typeof(AudioSource)))]
public class ArrowBase : MonoBehaviour
{
    [SerializeField] protected float speed = 5;
    [SerializeField] protected List<AudioClip> hitSounds = new List<AudioClip>();
    [SerializeField] protected AudioClip releaseSound = null;
    [SerializeField] protected AudioClip knockingSound = null;
    protected TrailRenderer trail;
    protected Rigidbody body;
    protected new AudioSource audio;
    protected bool hasCollided;
    public float damage = 50f;
    protected GameHandler GameHandler;

    public float TimeToDestroyArrow = 5f;

    //Set the chance of a certain arrow to be added
    [Range(1, 100)]
    public int ChanceOfReceiving = 50;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        body = GetComponent<Rigidbody>();
        audio = GetComponent<AudioSource>();

        GameHandler = FindObjectOfType<GameHandler>();
    }

    public void Loose(float drawForce)
    {
        hasCollided = false;
        trail.enabled = true;
        body.isKinematic = false;
        body.velocity = transform.forward * speed * drawForce;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        body.detectCollisions = true;
        audio.PlayOneShot(releaseSound);
    }

    public void Knock()
    {
        hasCollided = false;
        trail.enabled = false;
        body.isKinematic = true;
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        body.detectCollisions = false;
        audio.PlayOneShot(knockingSound);

        body.velocity = Vector3.zero;

        EnableColliders();
    }

    protected virtual void OnCollisionEnter(Collision collision) {}

    private void Update()
    {
        if (hasCollided == false)
        {
            //Making the arrows move more realistic
            transform.forward = Vector3.Slerp(transform.forward, body.velocity.normalized, 10f * Time.deltaTime);
        }
    }

    protected IEnumerator DestroyArrow()
    {
        yield return new WaitForSeconds(TimeToDestroyArrow);

        transform.SetParent(null);
        gameObject.SetActive(false);

        GameHandler.PoolManager.AddArrowToPool(gameObject);
    }

    private void EnableColliders()
    {
        foreach (var component in GetComponentsInChildren<Collider>())
        {
            component.enabled = true;
        }
    }

    protected void DisableColliders()
    {
        foreach (var component in GetComponentsInChildren<Collider>())
        {
            component.enabled = false;
        }
    }
}