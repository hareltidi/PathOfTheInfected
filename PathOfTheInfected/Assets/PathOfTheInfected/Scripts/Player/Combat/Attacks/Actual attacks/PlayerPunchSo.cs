using UnityEngine;

namespace PathOfTheInfected.Player.Combat.Attacks
{
    [CreateAssetMenu(fileName = "PlayerPunchSo", menuName = "Attacks/Player/PlayerPunchSo", order = 0)]
    public class PlayerPunchSo : PlayerAttackSoBase
    {
        /// <summary>
        /// The start of the attack
        /// </summary>
        public override void StartAttack()
        {
            PlayerCombat.PlayerPunchHitBox.BeginAttack();
            base.StartAttack();
        }


        public override void PerformAttack()
        {
            PlayerCombat.PlayerPunchHitBox.PerformHitCheck();
        }

        public override void EndAttack()
        {
            PlayerCombat.PlayerPunchHitBox.EndAttack();
            base.EndAttack();
        }
    }
}