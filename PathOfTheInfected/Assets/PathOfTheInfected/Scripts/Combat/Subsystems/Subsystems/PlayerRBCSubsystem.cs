using PathOfTheInfected.Player.Combat;
using UnityEngine;

namespace PathOfTheInfected.Combat
{
    public class PlayerRBCSubsystem : CombatSubsystem
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
            if (context.AttackerIsAirborne)
            {
                PlayerOwner.HalfResetDashes();
                PlayerOwner.ReplenishJumps();
                Debug.Log("Airborne hit, Resets granted!");
            }
        }

        public override void ClearStates()
        {
            base.ClearStates();
        }
    }
}