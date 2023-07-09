using SecurityWeapons;

namespace FiniteStateMachine.FighterPlaneStateMachine {
    public enum FighterPlaneStateType {
        Deactivated,
        TakingOff,
        Patrolling,
        Aiming,
        Shooting,
        Landing,
        Destroyed,
    }

    public abstract class FighterPlaneState : State<FighterPlane, FighterPlaneStateType> {
        protected FighterPlaneState(FighterPlane fighterPlane) : base(fighterPlane) { }
    }
}