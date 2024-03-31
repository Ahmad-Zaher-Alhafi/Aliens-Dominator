using Context;
using DG.Tweening;
using Pool;
using TMPro;
using UnityEngine;

namespace UI {
    public class AnimatedText : PooledObject {
        [SerializeField] private int devider = 2;
        [SerializeField] private float minSize = .3f;
        [SerializeField] private float maxSize = 1;

        private TextMeshPro text;

        private void Awake() {
            text = GetComponent<TextMeshPro>();
            transform.localScale = Vector3.zero;
        }

        public void ShowText(string textToShow, Vector3 createPosition, Color color) {
            text.color = color;
            transform.position = createPosition;
            text.text = textToShow;
            float distance = Vector3.Distance(transform.position, Ctx.Deps.GameController.Player.transform.position);
            float targetScale = Mathf.Clamp(distance / devider, minSize, maxSize); // Adjust min and max scale values according to your needs

            DOTween.Sequence()
                .Join(transform.DOPunchScale(Vector3.one * targetScale, 1, 1))
                .Append(transform.DOScale(Vector3.zero, .3f))
                .SetLink(gameObject)
                .OnKill(ReturnToPool)
                .Play();
        }
    }
}