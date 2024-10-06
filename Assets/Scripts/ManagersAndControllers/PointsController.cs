using System.Collections.Generic;
using UnityEngine;

namespace ManagersAndControllers {
    public class PointsController : MonoBehaviour {
        [Header("Fighter plane points")]
        [SerializeField] private Transform fighterPlaneTakeOffPoint;
        /// <summary>
        /// Point where the plane land and take off from
        /// </summary>
        public Transform FighterPlaneTakeOffPoint => fighterPlaneTakeOffPoint;

        [SerializeField] private Transform fighterPlaneLandingPoint;
        /// <summary>
        /// The point above the plane base where the plane should go to before stars landing
        /// </summary>
        public Transform FighterPlaneLandingPoint => fighterPlaneLandingPoint;

        [SerializeField] private List<Transform> fighterPlanePatrollingPoints = new();
        public IReadOnlyList<Transform> FighterPlanePatrollingPoints => fighterPlanePatrollingPoints;
    }
}