using Arrows;
using Context;
using ManagersAndControllers;
using Player;
using Pool;
using UnityEngine;

namespace Collectables {
    public class SupplyBalloon : PooledObject {
        public float HeightLimit;
        public float TimeLimit;
        public float Speed = 10f;

        [Range(1, 100)]
        public int ChanceOfSpawning = 50;

        public Arrow Arrow;
        [SerializeField] private Constants.SuppliesTypes suppliesType;

        public void Init(Vector3 position) {
            transform.position = position;
        }

        // Update is called once per frame
        private void Update() {
            if (transform.position.y >= HeightLimit) Destroy(gameObject);
            else transform.position += Vector3.up * Speed * Time.deltaTime;
        }

        private void OnCollisionEnter(Collision collision) {
            if (collision.collider.tag != "Arrow") return;

            if (!CompareTag(Constants.SuppliesCallerTag)) {
                var rig = FindObjectOfType<Player.Player>();
            } else {
                Ctx.Deps.EventsManager.OnCallingSupplies(suppliesType); //call the airplane to get a supplies drop
            }

            Destroy(gameObject);
        }
    }
}