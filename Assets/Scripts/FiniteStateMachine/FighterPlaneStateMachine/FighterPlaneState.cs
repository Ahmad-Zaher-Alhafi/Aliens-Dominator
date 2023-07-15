using SecurityWeapons;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public enum FighterPlaneStateType {
        Deactivated,
        TakingOff,
        Patrolling,
        Aiming,
        Shooting,
        GoingBackToBase,
        Destroyed,
    }

    public abstract class FighterPlaneState : State<FighterPlane, FighterPlaneStateType> {
        protected FighterPlaneState(FighterPlane fighterPlane) : base(fighterPlane) { }
    }
}