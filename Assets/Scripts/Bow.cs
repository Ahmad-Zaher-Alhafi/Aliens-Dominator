using Context;
using Unity.Netcode;
using UnityEngine;
public class Bow : NetworkBehaviour {
    [SerializeField] private float drawingSpeed;
    [SerializeField] private float releaseSpeed;
    [SerializeField] private float maxZPos;
    [SerializeField] private Transform bowMovingPart;

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
    public float DrawForce { get; private set; }

    private readonly NetworkVariable<Quaternion> topJointNetworkRotation = new();
    private readonly NetworkVariable<Quaternion> bottomJointNetworkRotation = new();
    private readonly NetworkVariable<Vector3> arrowParentNetworkLocalPosition = new();
    private readonly NetworkVariable<Vector3> bowMovingPartNetworkLocalPosition = new();

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();
        Destroy(gameObject);
    }

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

        if (IsOwner) {
            if (shootRoutine) return;

            if (Ctx.Deps.InputActions.SharedActions.PrimaryAction.IsPressed()) drawAngle += drawingSpeed * Time.deltaTime;
            else drawAngle += -releaseSpeed * Time.deltaTime;

            drawAngle = Mathf.Clamp(drawAngle, 0, maxDrawangle);

            Quaternion lookRotation = Quaternion.AngleAxis(-drawAngle, transform.right) * transform.rotation;
            topJoint.rotation = lookRotation;
            lookRotation = Quaternion.AngleAxis(drawAngle, transform.right) * transform.rotation;
            bottomJoint.rotation = lookRotation;
            DrawForce = drawAngle / maxDrawangle;
            arrowParent.localPosition = Vector3.Lerp(arrowPosition, maxArrowPosition, DrawForce);

            if (IsServer) {
                topJointNetworkRotation.Value = topJoint.rotation;
                bottomJointNetworkRotation.Value = bottomJoint.rotation;
                arrowParentNetworkLocalPosition.Value = arrowParent.localPosition;
                bowMovingPartNetworkLocalPosition.Value = bowMovingPart.localPosition;
            } else {
                UpdateMotionServerRPC(arrowParent.localPosition, topJoint.rotation, bottomJoint.rotation, bowMovingPart.localPosition);
            }
        } else {
            if (topJointNetworkRotation.Value != Quaternion.identity) {
                topJoint.rotation = Quaternion.LerpUnclamped(topJoint.rotation, topJointNetworkRotation.Value, .1f);
            }

            if (bottomJointNetworkRotation.Value != Quaternion.identity) {
                bottomJoint.rotation = Quaternion.LerpUnclamped(bottomJoint.rotation, bottomJointNetworkRotation.Value, .1f);
            }

            if (arrowParentNetworkLocalPosition.Value != Vector3.zero) {
                arrowParent.localPosition = Vector3.LerpUnclamped(arrowParent.localPosition, arrowParentNetworkLocalPosition.Value, .1f);
            }

            if (bowMovingPartNetworkLocalPosition.Value != Vector3.zero) {
                bowMovingPart.localPosition = Vector3.LerpUnclamped(bowMovingPart.localPosition, bowMovingPartNetworkLocalPosition.Value, .1f);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateMotionServerRPC(Vector3 arrowParentLocalPosition, Quaternion topJointRotation, Quaternion bottomJointRotation, Vector3 bowMovingPartLocalPosition) {
        topJointNetworkRotation.Value = topJointRotation;
        bottomJointNetworkRotation.Value = bottomJointRotation;
        arrowParentNetworkLocalPosition.Value = arrowParentLocalPosition;
        bowMovingPartNetworkLocalPosition.Value = bowMovingPartLocalPosition;
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