using System.Collections;
using Creature;
using UnityEngine;
using UnityEngine.AI;

namespace Arrows {
    public class SlowdownArrow : ArrowBase {
        public float slowdownTimer = 6f;
        public float speedDivider = 2f;

        protected override void OnCollisionEnter(Collision collision) {
            if (hasCollided) return;

            hasCollided = true;
            transform.SetParent(collision.transform);
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;

            if (GetComponent<Rigidbody>())
                GetComponent<Rigidbody>().isKinematic = true;

            trail.enabled = false;
            DisableColliders();

            var target = collision.gameObject.GetComponent<Hitable>();
            if (target != null) target.HandleArrowHit(this);
            else audio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Count)]);

            StartCoroutine(SlowDown());
        }

        private IEnumerator SlowDown() {
            GameObject[] enemies = GameHandler.AllEnemies.ToArray();

            foreach (GameObject e in enemies)
                if (e) {
                    var agent = e.GetComponent<NavMeshAgent>();
                    var creature = e.GetComponent<Creature.Creature>();

                    if (!creature.IsSlowedDown) //if the creature is not slowed down 
                    {
                        if (agent != null) //if it was a greound creature so it has a nave mesh agaent
                        {
                            agent.speed /= speedDivider;
                        } else //if it was air creature
                        {
                            creature.EnemySpeed /= speedDivider;
                            creature.GetComponent<FlyingSystem>().SetPatrollingSpeed(speedDivider, true);
                            creature.Animator.speed = creature.EnemySpeed / creature.MaxSpeed;
                        }

                        creature.IsSlowedDown = true;
                    }
                }

            yield return new WaitForSeconds(slowdownTimer);

            foreach (GameObject e in enemies)
                if (e) {
                    var agent = e.GetComponent<NavMeshAgent>();
                    var creature = e.GetComponent<Creature.Creature>();

                    if (creature.IsSlowedDown) //if the creature is slowed down 
                    {
                        if (agent != null) //if it was a greound creature so it has a nave mesh agaent
                        {
                            agent.speed *= speedDivider;
                        } else //if it was air creature
                        {
                            creature.EnemySpeed *= speedDivider;
                            creature.GetComponent<FlyingSystem>().SetPatrollingSpeed(speedDivider, false);
                            creature.Animator.speed = creature.EnemySpeed / creature.MaxSpeed;
                        }


                        creature.IsSlowedDown = false;
                    }
                }

            StartCoroutine(DestroyArrow());
        }
    }
}