using Unity.Netcode;
using UnityEngine;
public class Bow : NetworkBehaviour {
    [SerializeField] private float drawingSpeed;
    [SerializeField] private float releaseSpeed;
    [SerializeField] private float maxZPos;
    public Transform arrow;
    public Transform arrowParent;
    public Transform topJoint;
    public Transform pointA;
    public Transform bottomJoint;
    public Transform pointB;

    public Vector3 arrowPosition;
    public float maxDrawangle = 45;

    [Header("Bow following camera speed")]
    public float Speed = 5f;

    private Vector3 CachedLocalPos;
    private Transform CachedParent;
    private Transform CachedTransform;

    private float drawAngle;

    private LineRenderer lr;
    private Vector3 maxArrowPosition;
    private readonly bool shootRoutine = false;

    private void Start() {
        arrowPosition = arrowParent.localPosition;
        maxArrowPosition = arrowParent.localPosition + Vector3.back * maxZPos;
        lr = GetComponent<LineRenderer>();

        CachedTransform = transform; // Cache inital position of the bow
        CachedLocalPos = CachedTransform.localPosition; // Cache initial local position of the bow
        CachedParent = CachedTransform.parent; // Caching so we can follow the parent
        CachedTransform.parent = null; // Removing parent, so it moves without being restricted by it
    }

    // Update is called once per frame
    private void Update() {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if (!IsOwner) return;

        if (shootRoutine) return;

        if (Input.GetMouseButton(0)) drawAngle += drawingSpeed * Time.deltaTime;
        else drawAngle += -releaseSpeed * Time.deltaTime;

        drawAngle = Mathf.Clamp(drawAngle, 0, maxDrawangle);

        Quaternion lookRotation = Quaternion.AngleAxis(-drawAngle, transform.right) * transform.rotation;
        topJoint.rotation = lookRotation;
        lookRotation = Quaternion.AngleAxis(drawAngle, transform.right) * transform.rotation;
        bottomJoint.rotation = lookRotation;

        float t = drawAngle / maxDrawangle;
        arrowParent.localPosition = Vector3.Lerp(arrowPosition, maxArrowPosition, t);
    }


    private void LateUpdate() {
        var linePositions = new Vector3[3];
        linePositions[0] = pointA.position;
        linePositions[1] = arrow.position;
        linePositions[2] = pointB.position;
        lr.SetPositions(linePositions);

        // Follow the unlinked parent position and rotation with a delay
        Vector3 destination = CachedParent.TransformPoint(CachedLocalPos);
        CachedTransform.position = Vector3.Lerp(CachedTransform.position, destination, Speed * Time.deltaTime);
        CachedTransform.rotation = CachedParent.rotation;
    }
}