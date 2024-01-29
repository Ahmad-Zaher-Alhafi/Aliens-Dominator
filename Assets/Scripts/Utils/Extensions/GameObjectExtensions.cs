using UnityEngine;

namespace Utils.Extensions {
    public static class GameObjectExtensions {
        public static void SetActiveWithCheck(this GameObject gameObject, bool activate) {
            if (activate && gameObject.activeSelf || !activate && !gameObject.activeSelf) return;

            gameObject.SetActive(activate);
        }
    }
}