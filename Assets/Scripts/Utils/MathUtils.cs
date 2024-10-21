using System.Collections.Generic;
using UnityEngine;

namespace Utils {
    public static class MathUtils {
        public static T GetRandomObjectFromList<T>(IReadOnlyList<T> list) {
            if (list == null || list.Count == 0) return default;

            int randomNumber = Random.Range(0, list.Count + 1);
            randomNumber = Mathf.Clamp(randomNumber, 0, list.Count - 1);
            T randomPoint = list[randomNumber];
            return randomPoint;
        }

        public static T GetNextObjectInList<T>(IReadOnlyList<T> pathPointsList, int lastObjectIndex) where T : Object {
            return lastObjectIndex + 1 >= pathPointsList.Count ? null : pathPointsList[lastObjectIndex + 1];
        }

        // Function to calculate the object’s parabolic path.
        public static Vector3 CalculateParabolicPath(Vector3 start, Vector3 end, float arcHeight, float t) {
            // Lerp the horizontal movement (between start and end).
            Vector3 horizontal = Vector3.Lerp(start, end, t);

            // Calculate the height at this point in the journey (parabola formula).
            float arc = arcHeight * Mathf.Sin(Mathf.PI * t);

            // Add the arc height to the current horizontal position.
            return new Vector3(horizontal.x, horizontal.y + arc, horizontal.z);
        }

        public static float MapRange(float value, float oldMin, float oldMax, float newMin, float newMax) {
            // Avoid division by zero if the input range is zero
            if (Mathf.Abs(oldMax - oldMin) < Mathf.Epsilon) {
                Debug.LogWarning("Old range cannot be zero.");
                return newMin; // or return newMax, depending on desired behavior.
            }

            // Apply the mapping formula
            float mappedValue = newMin + (value - oldMin) * (newMax - newMin) / (oldMax - oldMin);
            return mappedValue;
        }
    }
}