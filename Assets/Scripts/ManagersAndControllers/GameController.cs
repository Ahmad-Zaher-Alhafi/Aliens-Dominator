using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace ManagersAndControllers {
    public class GameController : MonoBehaviour {
        
        
        private List<NavMeshSurface> navMeshSurfaces;

        private void Awake() {
            navMeshSurfaces = FindObjectsOfType<NavMeshSurface>().ToList();
            foreach (NavMeshSurface navMeshSurface in navMeshSurfaces) {
                navMeshSurface.BuildNavMesh();
            }
        }
        
        public new Coroutine StartCoroutine(IEnumerator routine) {
            return base.StartCoroutine(routine);
        }
    }
}