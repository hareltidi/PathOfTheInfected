using System;
using PathOfTheInfected.Combat;
using UnityEngine;

namespace PathOfTheInfected___Gameplay.Scripts.Killzone
{
    public class killzone : MonoBehaviour
    {
        public AttackDefinition killzoneAttackDef;
        public Vector2 killzoneSize = new Vector2(10f, 5f);
        public LayerMask spottableMask;

        private void Update()
        {
            Collider2D hit = Physics2D.OverlapBox(transform.position, killzoneSize, 0f,spottableMask);

            IHitResponder hitResponder = hit?.gameObject.GetComponent<IHitResponder>();

            if (hitResponder != null)
            {
                HitData data = new HitData()
                {
                    attackDefinition = killzoneAttackDef,
                    isFirstHit = false,
                    isPlayerDamage = false,
                    isAttackerInAir = false,
                    source = gameObject,
                    timeStamp = Time.timeSinceLevelLoad,
                    target = hit.gameObject,
                    firstHitDamageBoost = 0,
                    comboDamageScalingLevel = 1,
                };
                var result = HitDispatcher.ProcessHit(ref data);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, killzoneSize);
        }
    }
}