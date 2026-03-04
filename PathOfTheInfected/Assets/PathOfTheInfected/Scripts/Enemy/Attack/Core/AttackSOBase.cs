using PathOfTheInfected.Combat;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "AttackSOBase", menuName = "Enemy/Attack/Core/AttackSOBase", order = 0)]
    public class AttackSOBase : ScriptableObject
    {
        private AttackContext _context;

        [Header("Attack stats")] [SerializeField]
        protected AttackDefinition attackDef;

        [Tooltip("Should we check if the distance between the enemy and the spottable are under a certain threshold for us to attack?")]
        [field: SerializeField] public bool RequireDistanceFromEnemyToSpottable { get; protected set; } = true;
        [Tooltip("If we require distance, what is the threshold we should be under?")] [Range(0f, 100f)]
        [field: SerializeField] public float DistanceThreshold { get; protected set; } = 5f;
        [field: SerializeField] public float MaxAttackRange { get; protected set; } = 10f;
        [field: SerializeField] public float PoiseConsumed { get; protected set; } = 0.5f;
        [Header("Timers")]
        public float windupDuration = 0.5f;




        public virtual void InitAttack(AttackContext ctx, EnemyBrainBase owner, Transform target)
        {
            ctx.Phase = AttackPhase.WindUp;
            ctx.HasHit = false;
            ctx.IsFinished = false;
            ctx.Owner = owner;
            ctx.Timer = 0f;
            _context = ctx;
        }



        /// <summary>
        /// Runs every frame to run attack logic
        /// </summary>
        /// <param name="ctx">The attack context we have on this specific attack</param>
        public virtual void AttackLogic(AttackContext ctx)
        {
            ctx.Timer += Time.deltaTime;
            switch (ctx.Phase)
            {
                case AttackPhase.WindUp:
                    if (ctx.Timer >= windupDuration)
                    {
                        ctx.Timer = 0f;
                        ctx.Phase = AttackPhase.Active;
                    }
                    break;
                case AttackPhase.Active:
                    if (!ctx.HasHit)
                    {
                        PerformAttack(ctx);
                        ctx.HasHit = true;
                    }
                    else
                    {
                        ctx.Phase = AttackPhase.Recovery;
                        ctx.Timer = 0f;
                    }
                    break;
                case AttackPhase.Recovery:
                    if (ctx.Timer >= attackDef.recoveryTime)
                    {
                        ctx.IsFinished = true;
                    }
                    break;
                case AttackPhase.PoiseRecovery:
                    RecoverPoise(ctx);
                    break;
            }
        }

        /// <summary>
        /// Performs the actual attack
        /// </summary>
        /// <param name="ctx">The attack context we have on this specific attack</param>
        public virtual void PerformAttack(AttackContext ctx)
        {
            if (ctx.Owner.CurrentPoise > 0)
            {
                ctx.Owner.CurrentPoise = Mathf.Clamp(ctx.Owner.CurrentPoise - PoiseConsumed, 0f, ctx.Owner.maxPoise);
            }
            else
            {
                ctx.Phase = AttackPhase.PoiseRecovery;
                ctx.Timer = 0f;
            }
        }

        public virtual void RecoverPoise(AttackContext ctx)
        {
            ctx.Owner.CurrentPoise = Mathf.MoveTowards(ctx.Owner.CurrentPoise, ctx.Owner.maxPoise, Time.fixedDeltaTime * PoiseConsumed);
            if (ctx.Owner.CurrentPoise >= ctx.Owner.maxPoise)
            {
                ctx.Phase = AttackPhase.WindUp;
            }
        }
    }
}
