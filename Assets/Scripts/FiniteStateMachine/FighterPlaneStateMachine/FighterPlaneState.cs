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
        GettingHit,
    }

    public abstract class FighterPlaneState : State<FighterPlane, FighterPlaneStateType> {
        protected FighterPlaneState(FighterPlane fighterPlane, bool checkWhenAutomatingDisabled) : base(fighterPlane, checkWhenAutomatingDisabled) { }
    }
}