using System.Collections.Generic;
using PathOfTheInfected.Enemy;
using PathOfTheInfected.Enemy.Projectiles;
using PathOfTheInfected.Gameplay.Scripts.Bosses.LaserCentipede;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Bosses.LaserCentipede
{
    [CreateAssetMenu(fileName = "LaserShotAttack", menuName = "Boss/Attacks/LaserCentipede/LaserShotAttack")]
    public class LaserShotAttack : AttackSOBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float spawnMarginX = 2f;
        [SerializeField] private float spawnMarginY = 0f;
        [SerializeField] private bool requiresGrounded = true;
        [SerializeField] private AnimationClip attackAnim;
        [Header("Warning Box Config")]
        [SerializeField] private float warningBoxWidth = 1f;
        [SerializeField] private float warningBoxLength = 20f;
        [SerializeField] private Color warningColor = new(1f, 0f, 0f, 0.4f);
        [SerializeField] private float warningBoxTimePercentage = 60;

        private Vector2 _targetDir;
        private GameObject _warningInstance;
        private LineRenderer _warningLine;
        private LaserCentipedeBrain _bossBrain;
        private int _attackAnimHash;

        public override void InitAttack(AttackContext ctx, IAttackOwnerable owner, Transform target)
        {
            base.InitAttack(ctx, owner, target);
            _bossBrain = (LaserCentipedeBrain)ctx.Owner;
            _attackAnimHash = Animator.StringToHash(attackAnim.name);
        }

        public override void AttackLogic(AttackContext ctx)
        {
            if (requiresGrounded && !ctx.Owner.IsGrounded) return;

            ctx.Timer += Time.fixedDeltaTime;
            switch (ctx.Phase)
            {
                case AttackPhase.WindUp:
                    if (ctx.Target && _bossBrain)
                    {
                        CleanupWarning(); //Clean up warnings that didn't get cleaned up for some reason
                        _bossBrain.AnimInstance.PlayAnimationIfNotCurrent(_attackAnimHash, 0, 0,
                            true, true);
                        // Track player direction during windup but only in the percentage of the windup time we specify,
                        // to give the player some time to react to the attack direction
                        if (ctx.Timer <= windupDuration * (warningBoxTimePercentage / 100f))
                        {
                            _targetDir = (ctx.Target.position - ctx.Owner.Transform.position).normalized;
                        }

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

                        if (shouldFaceRight && !ctx.Owner.IsFacingRight || !shouldFaceRight && ctx.Owner.IsFacingRight && _bossBrain)
                        {
                            _bossBrain.FlipFacing();
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
                    if (ctx.Timer >= AttackDef.recoveryTime)
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
            projectileLogic?.InitProjectileValuesFromAttack(ctx, AttackDef, _targetDir, spawnPos, ctx.Owner.GameObject);
        }
    }
}
