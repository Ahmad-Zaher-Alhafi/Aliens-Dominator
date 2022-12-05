using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Creatures {
    public interface ITransportingCreature { }

    public class TransportingCreature : BossCreature, ITransportingCreature {
        [SerializeField] private ParticleSystem portalPaticles;
        [SerializeField] private float secondsForParticlesCreating;
        [SerializeField] private GameObject creatureMesh;
        [SerializeField] private float speedOfAdjestingHeight;
        [SerializeField] private Vector3 transportPositionOffset;
        private float correctCreatureHeight;
        private bool hasToSetHeightPosition; //to make sure that the creature has set up on the ground correctly(because not all the pathPoints have the same height)
        private NavMeshAgent navMeshAgent; //the nav nesh agent changes the creature height to be on the ground so we need it to get the correct height
        private Transform pathPointToTransfareTo;


        private void Start() {
            hasToSetHeightPosition = false;
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Update() {
            if (hasToSetHeightPosition) SetHeightPosition();

            if (Input.GetKeyDown(KeyCode.D)) OrderToTransport(pathPointToTransfareTo);
        }

        public void OrderToTransport(Transform pathPointToTransfareTo) {
            StartCoroutine(Transport(pathPointToTransfareTo));
        }

        private IEnumerator Transport(Transform pathPointToTransfareTo) {
            this.pathPointToTransfareTo = pathPointToTransfareTo;
            portalPaticles.Play();
            yield return new WaitForSeconds(secondsForParticlesCreating);
            if (gameObject != null) {
                creatureMesh.SetActive(false);
                transform.position = pathPointToTransfareTo.position + transportPositionOffset;
                portalPaticles.Play();
            }
            yield return new WaitForSeconds(secondsForParticlesCreating);
            if (gameObject != null) {
                creatureMesh.SetActive(true);
                StartCoroutine(GetCorrectHeight());
            }
        }

        private void SetHeightPosition() //adjest the creature height to the correctCreatureHeight
        {
            if (transform.position.y < correctCreatureHeight) transform.position += Vector3.up * speedOfAdjestingHeight * Time.deltaTime;
            else hasToSetHeightPosition = false;
        }

        private IEnumerator GetCorrectHeight() {
            float oldHeight = transform.position.y;
            navMeshAgent.enabled = true;
            yield return new WaitForSeconds(0.01f); //needs time to let the creature adjest it's y axis after enabling the navAgent
            correctCreatureHeight = transform.position.y;
            navMeshAgent.enabled = false;
            transform.position = new Vector3(transform.position.x, oldHeight, transform.position.z);
            hasToSetHeightPosition = true;
        }
    }
}