using System.Collections.Generic;
using PathOfTheInfected.Combat;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace PathOfTheInfected.Player.Combat
{
    public class PlayerPunchHitBox : MonoBehaviour
    {
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float hitboxHeight;
        [SerializeField] private Vector2 hitboxOffset;
        private Vector2 _lastAttackDirection = Vector2.right;
        private readonly HashSet<Collider2D> _alreadyHit = new();

        public void BeginAttack()
        {
            _alreadyHit.Clear();
        }

        public void PerformHitCheck()
        {
            Vector2 dir = GetAttackDirection().normalized;

            float range = playerCombat.punchAttack.attackDef.attackRange;

            Vector2 size = new Vector2(range, hitboxHeight);

            Vector2 center =
                (Vector2)transform.position +
                dir * (range * 0.5f);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            var hits = Physics2D.OverlapBoxAll(center, size, angle, enemyLayer);

            foreach (var hit in hits)
            {
                if (!_alreadyHit.Add(hit)) continue;

                if (hit.TryGetComponent<IHitResponder>(out var responder))
                {
                    HitData data = BuildHitData(hit);

                    HitResult result = HitDispatcher.ProcessHit(ref data);

                    OnPunchFinished(result);
                }
            }
        }

        public Vector2 GetAttackDirection()
        {
            Vector2 input = InputManager.Movement;

            if (input.sqrMagnitude > 0.01f)
            {
                _lastAttackDirection = input.normalized;
            }

            return _lastAttackDirection;
        }

        public void EndAttack()
        {
            _alreadyHit.Clear();
        }

        private HitData BuildHitData(Collider2D target)
        {
            var data = new HitData
            {
                source = gameObject,
                target = target.gameObject,
                attackDefinition = playerCombat.punchAttack.attackDef,
                comboDamageScalingLevel = playerCombat.PerkSubsystem.HitMultiplier,
                isFirstHit = false,
                firstHitDamageBoost = playerCombat.punchAttack.attackDef.firstHitDamageBoost,
                isPlayerDamage = true,
                isAttackerInAir = playerCombat.PlayerOwner.IsGrounded,
                timeStamp = Time.timeSinceLevelLoad
            };
            return data;
        }


        private void OnPunchFinished(HitResult hitResult)
        {
            playerCombat.CurrentAttack.ReactToHitResult(hitResult);
        }

        private void OnDrawGizmosSelected()
        {
            if (!playerCombat || !playerCombat.punchAttack || !playerCombat.punchAttack.attackDef) return;

            Vector2 dir = _lastAttackDirection.normalized;
            float range = playerCombat.punchAttack.attackDef.attackRange;

            Vector2 center = (Vector2)transform.position + dir * (range * 0.5f);
            Vector2 size = new Vector2(range, hitboxHeight);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // semi-transparent red
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawCube(Vector3.zero, new Vector3(size.x, size.y, 1f));
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}