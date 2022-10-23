using System.Linq;
using UnityEngine;
public class PathMovement : MonoBehaviour {
    [SerializeField] private Vector3[] points = { Vector3.zero, Vector3.up };

    public float speed = 1;
    [SerializeField] private Mode movementMode = Mode.Loop;

    [SerializeField] private bool faceTarget;
    [SerializeField] private bool randomStart;
    private int pingPongStep = 1;

    private int targetIndex = 1;
    private Vector3[] worldPoints;

    private void Start() {
        CalculateWorldPoints();
        SetStartPosition();
        UpdateFacing();
    }

    private void Update() {
        MoveTowardsTarget();
    }

    private void OnDrawGizmosSelected() {
        if (points.Length == 0) return;

        if (Application.isPlaying == false) CalculateWorldPoints();

        Gizmos.color = Color.red;
        Vector3 previousPoint = worldPoints[points.Length - 1];

        for (int i = 0; i < points.Length; i++) {
            Vector3 point = worldPoints[i];
            Gizmos.DrawSphere(point, 0.1f);
            if (i != 0 || movementMode != Mode.PingPong) Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }

    private void SetStartPosition() {
        targetIndex = randomStart ? Random.Range(0, points.Length) : 0;
        transform.position = worldPoints[targetIndex];
        SelectNextTarget();
    }

    private void CalculateWorldPoints() {
        worldPoints = points.Select(ToWorldPosition).ToArray();
    }

    private Vector3 ToWorldPosition(Vector3 point) {
        return transform.position + (Vector3) (transform.localToWorldMatrix * point);
    }

    private void UpdateFacing() {
        if (faceTarget) transform.LookAt(worldPoints[targetIndex]);
    }

    private void MoveTowardsTarget() {
        if (points.Length == 0) return;
        Vector3 targetPos = worldPoints[targetIndex];

        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * speed);

        if (Vector3.Distance(targetPos, transform.position) < 0.001f) SelectNextTarget();
    }

    private void SelectNextTarget() {
        switch (movementMode) {
            case Mode.PingPong:
                NextPingPongTarget();
                break;

            case Mode.Loop:
                targetIndex = (targetIndex + 1) % points.Length;
                break;

            default:
                Debug.LogError("Undefined movement mode selected!");
                break;
        }

        UpdateFacing();
    }

    private void NextPingPongTarget() {
        if (pingPongStep > 0 && targetIndex == points.Length - 1) pingPongStep = -1;
        else if (pingPongStep < 0 && targetIndex == 0) pingPongStep = 1;

        targetIndex = targetIndex + pingPongStep;
    }

    private enum Mode {
        PingPong,
        Loop
    }
}