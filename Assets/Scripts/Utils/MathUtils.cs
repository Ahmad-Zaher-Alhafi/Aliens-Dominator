using System.Collections.Generic;
using UnityEngine;

namespace Utils {
    public class MathUtils : MonoBehaviour {
        public static T GetRandomObjectFromList<T>(IReadOnlyList<T> pathPointsList) {
            int randomNumber = Random.Range(0, pathPointsList.Count + 1);
            randomNumber = Mathf.Clamp(randomNumber, 0, pathPointsList.Count - 1);
            T randomPoint = pathPointsList[randomNumber];
            return randomPoint;
        }
        
        public static T? GetNextObjectInList<T>(IReadOnlyList<T> pathPointsList, int lastObjectIndex) where T : Object {
            return lastObjectIndex + 1 >= pathPointsList.Count ? null : pathPointsList[lastObjectIndex + 1];
        }
    }
}