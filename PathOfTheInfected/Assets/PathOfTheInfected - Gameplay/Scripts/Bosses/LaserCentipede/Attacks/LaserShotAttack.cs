using PathOfTheInfected.Core.Scripts.Boss;
using PathOfTheInfected.Enemy;
using PathOfTheInfected.Enemy.Projectiles;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Bosses.LaserCentipede
{
    [CreateAssetMenu(fileName = "LaserShotAttack", menuName = "Boss/Attacks/LaserCentipede/LaserShotAttack")]
    public class LaserShotAttack : AttackSOBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float spawnMarginX = 2f;
        [SerializeField] private float spawnMarginY = 0f;
        [Header("Warning Box Config")]
        [SerializeField] private float warningBoxWidth = 1f;
        [SerializeField] private float warningBoxLength = 20f;
        [SerializeField] private Color warningColor = new(1f, 0f, 0f, 0.4f);

        private Vector2 _targetDir;
        private GameObject _warningInstance;
        private LineRenderer _warningLine;
        private BossBrain _bossBrain;

        public override void InitAttack(AttackContext ctx, IAttackOwnerable owner, Transform target)
        {
            base.InitAttack(ctx, owner, target);
            _bossBrain = (BossBrain)ctx.Owner;
        }

        public override void AttackLogic(AttackContext ctx)
        {
            if (!ctx.Owner.IsGrounded) return;

            ctx.Timer += Time.fixedDeltaTime;
            switch (ctx.Phase)
            {
                case AttackPhase.WindUp:
                    if (ctx.Target)
                    {
                        // Track player direction during windup
                        _targetDir = (ctx.Target.position - ctx.Owner.Transform.position).normalized;

                        if (!_warningInstance)
                        {
                            _warningInstance = new GameObject("LaserWarningBox");
                            _warningLine = _warningInstance.AddComponent<LineRenderer>();
                            // Use built-in sprite shader for transparency support
                            _warningLine.material = new Material(Shader.Find("Sprites/Default"));
                            _warningLine.startColor = warningColor;
                            _warningLine.endColor = warningColor;
                            _warningLine.startWidth = warningBoxWidth;
                            _warningLine.endWidth = warningBoxWidth;
                            _warningLine.positionCount = 2;
                            _warningLine.sortingOrder = 100; // Render on top of most things
                        }

                        bool shouldFaceRight = _targetDir.x >= 0;

                        if (shouldFaceRight && !ctx.Owner.IsFacingRight || !shouldFaceRight && ctx.Owner.IsFacingRight)
                        {
                            _bossBrain?.FlipFacing();
                        }

                        float signedMarginX = ctx.Owner.IsFacingRight ? spawnMarginX : -spawnMarginX;
                        Vector2 spawnPos = new Vector2(
                            ctx.Owner.Transform.position.x + signedMarginX,
                            ctx.Owner.Transform.position.y + spawnMarginY
                        );

                        _warningLine.SetPosition(0, spawnPos);
                        _warningLine.SetPosition(1, spawnPos + _targetDir * warningBoxLength);
                    }

                    if (ctx.Timer >= windupDuration)
                    {
                        ctx.Timer = 0f;
                        ctx.Phase = AttackPhase.Active;
                    }
                    break;
                case AttackPhase.Active:
                    CleanupWarning();
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
                    CleanupWarning();
                    if (ctx.Timer >= attackDef.recoveryTime)
                    {
                        ctx.IsFinished = true;
                    }
                    break;
                case AttackPhase.PoiseRecovery:
                    CleanupWarning();
                    RecoverPoise(ctx);
                    break;
            }
        }

        private void CleanupWarning()
        {
            if (_warningInstance)
            {
                Destroy(_warningInstance);
                _warningInstance = null;
                _warningLine = null;
            }
        }

        private void OnDisable()
        {
            CleanupWarning();
        }

        public override void PerformAttack(AttackContext ctx)
        {
            base.PerformAttack(ctx);

            if (!projectilePrefab) return;

            float signedMarginX = ctx.Owner.IsFacingRight ? spawnMarginX : -spawnMarginX;
            Vector2 spawnPos = new Vector2(
                ctx.Owner.Transform.position.x + signedMarginX,
                ctx.Owner.Transform.position.y + spawnMarginY
            );

            if (_targetDir == Vector2.zero)
            {
                _targetDir = ctx.Owner.IsFacingRight ? Vector2.right : Vector2.left;
            }

            GameObject projectileToSpawn = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            ProjectileBase projectileLogic = projectileToSpawn.GetComponent<ProjectileBase>();
            projectileLogic?.InitProjectileValuesFromAttack(ctx, attackDef, _targetDir, spawnPos, ctx.Owner.GameObject);
        }
    }
}
