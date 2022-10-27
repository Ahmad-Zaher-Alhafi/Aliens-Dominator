using System.Collections.Generic;
using Creatures;
using ManagersAndControllers;
using UnityEngine;

namespace Defence_Weapons {
    public class SecuritySensor : MonoBehaviour {
        public bool IsItAirPlaneSensor;
        [HideInInspector] public List<Creatures.Creature> Targets = new(); //the creatures that entered the sensor zone
        [Header("Use this field if this sensor is for air or ground weapon")]
        public SecurityWeapon SecurityWeapon;
        [Header("Use this field if this sensor is for fighter airplane")]
        public FighterAirPlane FighterAirPlane;
        [SerializeField] private float rayOffset;

        [HideInInspector] public Creatures.Creature Target; //target to shoot at 
        private readonly string[] layersToIgnor = new string[2]; //layers to prevent the raycast from colliding with
        private RaycastHit raycast; //raycast to chick if we can hit that target and it's not hiden behind another object

        private void Start() {
            layersToIgnor[0] = LayerMask.LayerToName(Constants.projectileLayerNumber);
            layersToIgnor[1] = LayerMask.LayerToName(Constants.ignorRaycastLayerNumber);

            EventsManager.onEnemyDiesInsideSecurityArea += RemoveDeadTarget;
        }

        private void FixedUpdate() {
            if (!IsItAirPlaneSensor) {
                if (SecurityWeapon.HasToDefend) {
                    if (!SecurityWeapon.IsShooting && (Target == null || Target.CurrentState == Creature.CreatureState.Dead)) //if the waepon is not busy and the target is not null or dead
                        if (Targets.Count > 0) //if there are Targets inside the security area
                            foreach (Creatures.Creature tar in Targets) //find a one target from the Targets that had entred the security area
                                if (tar != null && Target.CurrentState != Creature.CreatureState.Dead) //if this creature is not null and not dead
                                    if (CheckIfClearToShootAt(tar)) //if we can shoot at it(not hiden behind another object)
                                    {
                                        Target = tar;
                                        SecurityWeapon.OrderToShoot(tar.transform); //order the wapon to shoot at that target
                                        break;
                                    }
                    if (Target != null) //if the target not null then keep checking and see if we still able to shoot at him and he did not hide behind another object(a rock or something)
                        if (!CheckIfClearToShootAt(Target) || Target.CurrentState == Creature.CreatureState.Dead) {
                            SecurityWeapon.StopShooting(); //if we are not able to shoot him any more then order the weapon to stop shooting at him
                            Target = null;
                        }
                }
            } else {
                if (FighterAirPlane.HasToDefend) {
                    if (!FighterAirPlane.IsShooting && (Target == null || Target.CurrentState == Creature.CreatureState.Dead)) //if the waepon is not busy and the target is not null or dead
                        if (Targets.Count > 0) //if there are Targets inside the security area
                            foreach (Creatures.Creature tar in Targets) //find a one target from the Targets that had entred the security area
                                if (tar != null && Target.CurrentState != Creature.CreatureState.Dead) //if this creature is not null and not dead 
                                    if (CheckIfClearToShootAt(tar)) //if we can shoot at it(not hiden behind another object)
                                    {
                                        Target = tar;
                                        FighterAirPlane.OrderToShoot(tar.transform); //order the wapon to shoot at that target
                                        break;
                                    }

                    if (Target != null) //if the target not null then keep checking and see if we still able to shoot at him and he did not hide behind another object(a rock or something)
                        if (!CheckIfClearToShootAt(Target) || Target.CurrentState == Creature.CreatureState.Dead) {
                            FighterAirPlane.StopShooting(); //if we are not able to shoot him any more then order the weapon to stop shooting at him
                            Target = null;
                        }
                }
            }
        }

        private void OnDestroy() {
            EventsManager.onEnemyDiesInsideSecurityArea -= RemoveDeadTarget;
        }

        private void OnDrawGizmos() //drow a ray which the sensor use it(helps to visualies things)
        {
            if (!IsItAirPlaneSensor) {
                if (SecurityWeapon.HasToDefend && Target != null && Target.CurrentState != Creature.CreatureState.Dead) {
                    Gizmos.color = new Color(1, 0, 0, 1);
                    Gizmos.DrawRay(transform.position, transform.forward * Vector3.Distance(transform.position, Target.RigBody.transform.position));
                }
            } else {
                if (FighterAirPlane.HasToDefend && Target != null && Target.CurrentState != Creature.CreatureState.Dead) {
                    Gizmos.color = new Color(1, 0, 0, 1);
                    Gizmos.DrawRay(transform.position, transform.forward * Vector3.Distance(transform.position, Target.RigBody.transform.position));
                }
            }
        }

        private bool CheckIfClearToShootAt(Creatures.Creature target) //shoot a ray towards the target to know if we can shoot him and not hiden behind another object(a rock or something)
        {
            if (target != null) {
                transform.LookAt(target.RigBody.transform.position);

                Physics.Raycast(transform.position, transform.forward, out raycast, Vector3.Distance(transform.position, target.RigBody.transform.position), ~LayerMask.GetMask(layersToIgnor)); //the +1 is needed for the animation problem
                if (raycast.collider != null) {
                    if (raycast.collider.gameObject.layer == Constants.enemyLayerNumber) //if the ray hit a creature
                        return true;
                    return false;
                }
                return false;
            }
            return false;
        }

        public void RemoveDeadTarget(Creatures.Creature creature) //remove the dead creature from Targets list after it dies
        {
            if (creature != null)
                if (Targets.Contains(creature))
                    Targets.Remove(creature);
        }
    }
}