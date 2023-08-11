using System.Collections;
using Context;
using FMODUnity;
using UnityEngine;
public class Airplane : MonoBehaviour {

    [SerializeField] private GameObject suppliesPrefab; //supplies object that are we going to drop
    [SerializeField] private GameObject rocketsAmmoPrefab; //rocketsPack object that are we going to drop
    [SerializeField] private GameObject bulletsAmmoPrefab; //bulletsAmmo object that are we going to drop

    [SerializeField] private float airplaneSpeed;
    [SerializeField] private float secondsToDropSuppiles; //how many seconds should the airplane fly before it drops the supplies(to control the area)
    [SerializeField] private float secondsToResetAirplanePostion; //seconds needed for the airplane to get diaapeared from player view field
    [SerializeField] private Transform dropCreatingPoint; //where should the drop be creating(in this case it should be created in the airplane position but a bit lower to looks like it was dropped out of the airplane)
    [SerializeField] private float dropRotatingSpeed; //the speed of the rotating movement of the drop

    private StudioEventEmitter airplaneSound;
    private Vector3 initialPosition; //the start postion of the aireplane where it comes from
    private Rigidbody rig;

    public bool IsItBusy {
        get;
        private set;
    }

    private void Awake() {
        airplaneSound = GetComponent<StudioEventEmitter>();
        rig = GetComponent<Rigidbody>();
        initialPosition = transform.position;
    }

    private void OnDestroy() {
        Ctx.Deps.EventsManager.onCallingSupplies -= MoveToDropArea;
    }

    /// <summary>
    ///     to let the airplane move to the area that you want to drop the supplies in
    /// </summary>
    public void MoveToDropArea(Constants.SuppliesTypes suppliesType) {
        IsItBusy = true;
        airplaneSound.Play();
        rig.velocity = transform.forward * airplaneSpeed; //move the airplane
        StartCoroutine(DropSupplies(suppliesType));
    }

    /// <summary>
    ///     to let the aireplane drop the supplies after it reaches to the dropping area
    /// </summary>
    public IEnumerator DropSupplies(Constants.SuppliesTypes suppliesType) {
        yield return new WaitForSeconds(secondsToDropSuppiles); //wait for the wanted time
        GameObject supplies = null;

        switch (suppliesType) {
            case Constants.SuppliesTypes.ArrowUpgrade:
                supplies = Instantiate(suppliesPrefab, dropCreatingPoint.position, Quaternion.identity);
                break; //create the drop
            case Constants.SuppliesTypes.RocketsAmmo:
                supplies = Instantiate(rocketsAmmoPrefab, dropCreatingPoint.position, Quaternion.identity);
                break; //create the drop
            case Constants.SuppliesTypes.BulletsAmmo:
                supplies = Instantiate(bulletsAmmoPrefab, dropCreatingPoint.position, Quaternion.identity);
                break; //create the drop

        }

        var supRig = supplies.GetComponent<Rigidbody>();
        supRig.velocity = rig.velocity; //give it velocity same to the airplane veocity
        supRig.AddTorque(Vector3.one * dropRotatingSpeed); //give it rotating movement to be cooler
        StartCoroutine(ResetAirplanePosition());
    }

    /// <summary>
    ///     get the airplane back to it's initial posiotn after it disappears from the player view field to make it ready to
    ///     drop another one later
    /// </summary>
    /// <returns></returns>
    public IEnumerator ResetAirplanePosition() {
        yield return new WaitForSeconds(secondsToResetAirplanePostion); //wait for the airplane until it disappears
        rig.velocity = Vector3.zero; //stop the airplane movement
        yield return new WaitForSeconds(3); //wait until the smoke disappears
        transform.position = initialPosition; //reset the airplane to it's initial position
        airplaneSound.Stop();
        IsItBusy = false;
        gameObject.SetActive(false);
    }
}