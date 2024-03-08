using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Pool;
using UnityEngine;
using UnityEngine.AI;

public class PathDrawer : PooledObject {
    [SerializeField] private Material materialPrefab;
    [SerializeField] private LineRenderer linerRenderer;
    [SerializeField] private float animationDuration = .5f;
    [SerializeField] private float fragmentationDistance = 3;
    [SerializeField] private float pathCreationSpeed = .03f;
    [SerializeField] private float timeToClearPath = 5;

    private void Awake() {
        Material material = Instantiate(materialPrefab);
        linerRenderer.material = material;
        material.DOOffset(Vector2.left, animationDuration).SetEase(Ease.Linear)
            .SetLoops(-1)
            .OnStepComplete(() => material.mainTextureOffset = Vector2.zero)
            .SetLink(gameObject)
            .Play();
    }

    public void Draw(List<Vector3> pathPoints) {
        linerRenderer.positionCount = 0;

        List<Vector3> pointsToDraw = new List<Vector3>();

        for (int i = 1; i < pathPoints.Count; i++) {
            Vector3 previousPoint = pathPoints[i - 1];
            Vector3 currentPoint = pathPoints[i];
            Vector3 direction = (currentPoint - previousPoint).normalized;

            NavMeshPath navMeshPath = new NavMeshPath();
            NavMesh.CalculatePath(previousPoint, currentPoint, NavMesh.AllAreas, navMeshPath);

            for (int j = 1; j < navMeshPath.corners.Length; j++) {
                previousPoint = navMeshPath.corners[j - 1];
                currentPoint = navMeshPath.corners[j];

                float distance = Vector3.Distance(previousPoint, currentPoint);
                int numOfFragments = Mathf.CeilToInt(distance / fragmentationDistance);

                float highestY = Mathf.Max(previousPoint.y, currentPoint.y);
                float lowestY = Mathf.Min(previousPoint.y, currentPoint.y);

                for (int k = 0; k <= numOfFragments; k++) {
                    Vector3 fragmentPoint = Vector3.MoveTowards(previousPoint, currentPoint, k * fragmentationDistance);

                    if (Physics.Raycast(new Vector3(fragmentPoint.x, 1000f, fragmentPoint.z), Vector3.down, out RaycastHit hit, Mathf.Infinity, 1 << 3)) {

                        if (hit.point.y > highestY) {
                            // If the fragment point y position is higher than the two original points, then make sure that the straight path between those two points is blocked
                            if (!Physics.Raycast(new Vector3(fragmentPoint.x, fragmentPoint.y + fragmentationDistance, fragmentPoint.z), direction, out RaycastHit _, fragmentationDistance, 1 << 3)) continue;
                        }

                        if (hit.point.y < lowestY) {
                            // If the fragment point y position is lower than the two original points, then make sure that the straight path between those two points is blocked
                            if (Physics.Raycast(new Vector3(fragmentPoint.x, fragmentPoint.y + fragmentationDistance, fragmentPoint.z), direction, out RaycastHit _, fragmentationDistance, 1 << 3)) continue;
                        }

                        fragmentPoint.y = hit.point.y + 1;
                    }

                    pointsToDraw.Add(fragmentPoint);
                }
            }
        }

        StartCoroutine(DrawPathSlowly(pointsToDraw));
    }

    /// <summary>
    /// Draw it slowly so the player can watch it while being drawn
    /// </summary>
    /// <param name="pointsToDraw"></param>
    /// <returns></returns>
    private IEnumerator DrawPathSlowly(List<Vector3> pointsToDraw) {
        for (int i = 0; i < pointsToDraw.Count; i++) {
            yield return new WaitForSeconds(pathCreationSpeed);
            linerRenderer.positionCount = i + 1;
            linerRenderer.SetPosition(i, pointsToDraw[i]);
        }

        StartCoroutine(ClearPathDelayed());
    }

    private IEnumerator ClearPathDelayed() {
        yield return new WaitForSeconds(timeToClearPath);
        Clear();
    }

    private void Clear() {
        linerRenderer.positionCount = 0;
        ReturnToPool();
    }
}