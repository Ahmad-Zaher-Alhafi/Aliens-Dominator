using System.Collections.Generic;
using UnityEngine;

namespace Utils {
    public class MathUtils : MonoBehaviour {
        public static T GetRandomObjectFromList<T>(IReadOnlyList<T> list) {
            if (list == null) return default;

            int randomNumber = Random.Range(0, list.Count + 1);
            randomNumber = Mathf.Clamp(randomNumber, 0, list.Count - 1);
            T randomPoint = list[randomNumber];
            return randomPoint;
        }

        public static T GetNextObjectInList<T>(IReadOnlyList<T> pathPointsList, int lastObjectIndex) where T : Object {
            return lastObjectIndex + 1 >= pathPointsList.Count ? null : pathPointsList[lastObjectIndex + 1];
        }
    }
}