using PathOfTheInfected.Core.Scripts.Boss;
using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Bosses.LaserCentipede
{
    [CreateAssetMenu(fileName = "JumpAttack", menuName = "Boss/Attacks/LaserCentipede/JumpAttack")]
    public class JumpAttack : AttackSOBase
    {
        [SerializeField] private float jumpForceX = 5f;
        [SerializeField] private float jumpForceY = 10f;

        private Rigidbody2D _rb;
        private BossBrain _bossBrain;

        public override void InitAttack(AttackContext ctx, IAttackOwnerable owner, Transform target)
        {
            base.InitAttack(ctx, owner, target);
            _rb =  ctx.Owner.GameObject.GetComponent<Rigidbody2D>();
            _bossBrain = ctx.Owner.GameObject.GetComponent<BossBrain>();
        }

        public override void PerformAttack(AttackContext ctx)
        {
            base.PerformAttack(ctx);

            if (_rb && _bossBrain.IsGrounded)
            {
                float dirX = Mathf.Sign(ctx.Target.position.x - ctx.Owner.Transform.position.x);

                _rb.linearVelocity = new Vector2(dirX * jumpForceX, jumpForceY);
            }
        }
    }
}
