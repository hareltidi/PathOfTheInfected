using System;
using PathOfTheInfected.Combat;
using UnityEngine;

namespace PathOfTheInfected.Player.Combat
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerPunchHitBox : MonoBehaviour
    {
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private BoxCollider2D playerPunchHitBox;
        [SerializeField] private BoxCollider2D playerHitBox;
        private bool _active = true;

        private void OnEnable()
        {
            if (!playerPunchHitBox)
            {
                playerPunchHitBox = GetComponent<BoxCollider2D>();
            }

            if (!playerHitBox || !playerPunchHitBox || !playerCombat || !playerCombat.punchAttack || !playerCombat.punchAttack.attackDef)
            {
                return;
            }

            Bounds baseBounds = playerHitBox.bounds;
            Vector2 baseCenter = baseBounds.center;
            Vector2 baseSize = baseBounds.size;

            float range = Mathf.Max(0f, playerCombat.punchAttack.attackDef.attackRange);
            int facingDirection = transform.lossyScale.x >= 0f ? 1 : -1;
            float forwardOffset = range * 0.5f * facingDirection;

            Vector2 center = baseCenter + (Vector2.right * forwardOffset);
            Vector2 size = new Vector2(baseSize.x + range, baseSize.y);

            Vector2 localCenter = transform.InverseTransformPoint(center);
            playerPunchHitBox.offset = localCenter;
            playerPunchHitBox.size = size;
        }


        public void EnableHitBox()
        {
            _active = true;
        }

        public void DisableHitBox()
        {
            _active = false;
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_active) return;
            // Build the hit data
            IHitResponder responder = other.GetComponent<IHitResponder>();
            if (responder != null)
            {
                HitData hitData = BuildHitData(other);
                // Dispatch
                HitResult result = HitDispatcher.ProcessHit(hitData);
                // react
                OnPunchFinished(result);
            }

        }

        private HitData BuildHitData(Collider2D target)
        {
            HitData data = new HitData()
            {
                source = gameObject,
                target = target.gameObject,
                attackDefinition = playerCombat.punchAttack.attackDef,
                comboDamageScalingLevel = 1,
                isFirstHit = false,
                firstHitDamageBoost = 0
            };
            return data;
        }


        private void OnPunchFinished(HitResult hitResult)
        {
            _active = false;
            playerCombat.CurrentAttack.ReactToHitResult(hitResult);
        }
    }
}
