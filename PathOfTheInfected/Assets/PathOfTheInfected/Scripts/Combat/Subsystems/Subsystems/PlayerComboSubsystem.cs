using PathOfTheInfected.Player.Combat;

namespace PathOfTheInfected.Combat
{
    public class PlayerComboSubsystem : CombatSubsystem
    {
        public override void Initialize(PlayerCombat owner, bool isInDebugMode)
        {
            base.Initialize(owner, isInDebugMode);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            base.FixedUpdate(fixedDeltaTime);
        }

        protected override void OnRegisterHit(CombatHitContext context)
        {
            base.OnRegisterHit(context);
        }

        public override void ClearStates()
        {
            base.ClearStates();
        }
    }
}