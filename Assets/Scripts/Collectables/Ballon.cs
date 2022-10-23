using Arrows;
using ManagersAndControllers;
using Player;
using UnityEngine;

namespace Collectables {
    public class Ballon : MonoBehaviour {
        public float HeightLimit;
        public float TimeLimit;
        public float Speed = 10f;

        [Range(1, 100)]
        public int ChanceOfSpawning = 50;

        public ArrowBase Arrow;
        [SerializeField] private Constants.SuppliesTypes suppliesType;
        private GameHandler GameHandler;

        // Start is called before the first frame update
        private void Start() {
            GameHandler = FindObjectOfType<GameHandler>();

            Destroy(gameObject, TimeLimit);
        }

        // Update is called once per frame
        private void Update() {
            if (transform.position.y >= HeightLimit) Destroy(gameObject);
            else transform.position += Vector3.up * Speed * Time.deltaTime;
        }

        private void OnCollisionEnter(Collision collision) {
            if (collision.collider.tag != "Arrow") return;

            if (!CompareTag(Constants.SuppliesCallerTag)) {
                var rig = FindObjectOfType<ArcheryRig>();
                rig.AddArrow(Arrow);
            } else {
                EventsManager.OnCallingSupplies(suppliesType); //call the airplane to get a supplies drop
            }

            Destroy(gameObject);
        }
    }
}