using System;
using PathOfTheInfected.Combat;
using TidiGameplayMessaging.Core;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Enemy.Health
{
    public class EnemyHealth : MonoBehaviour, IHitResponder, IHitStoppable
    {
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");

        public bool IsDead { get; set; }
        [Header("Health and damage")]
        [field: SerializeField] public float CurrentHealth { get; private set; }
        [field: SerializeField] public float MaxHealth { get; private set; }
        [ColorUsage(true, true)]
        public Color flashColor = Color.red;
        [SerializeField] public float flashTime = 0.1f;
        [SerializeField] public EaseType damageFlashEaseType = EaseType.Linear;
        protected Material[] Materials;
        private Color _originalColor;
        private TidiTween<float>[] _flashTweens;
        [SerializeField] protected SpriteRenderer[] spriteRenderers;
        public Action EnemyDamaged;

        private void Awake()
        {
            InitMaterials();
            CurrentHealth = MaxHealth;
        }


        #region Damage

        public void Die()
        {
            IsDead = true;
            Destroy(gameObject, flashTime + 0.1f);
        }

        public void TakeDamage(float finalDamage, float hitStopTime)
        {
            if (IsDead) return;
            CurrentHealth -= finalDamage;
            TidiGameplayMessagingSubsystem.Instance.Broadcast<OnEnemyDamaged>();
            EnemyDamaged?.Invoke();
            FlashDamage();
            HitStop(hitStopTime);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void FlashDamage()
        {
            SetFlashColor(in flashColor);
            for (int i = 0; i < Materials.Length; i++)
            {
                if (_flashTweens[i] != null)
                {
                    _flashTweens[i].FullKill();
                }

                Material localMat = Materials[i];
                float currentAmount = localMat.GetFloat(FlashAmountId);
                _flashTweens[i] = TidiTweenManager
                    .TweenFloat(localMat, currentAmount, 1, flashTime,
                        (value) => { localMat.SetFloat(FlashAmountId, value); }).SetPingPong(2)
                    .SetEase(damageFlashEaseType);
            }
            SetFlashColor(in _originalColor);

        }

        ///<summary>
        /// Set the flash color when we need to flash
        ///</summary>
        /// <param name="color">The color the flash should be in</param>
        private void SetFlashColor(in Color color)
        {
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i].SetColor(FlashColorId, color);
            }
        }

        ///<summary>
        /// Apply hit stop
        ///</summary>
        ///<param name="duration">How long should we freeze time</param>
        public void HitStop(float duration)
        {
            if (!HitStopManager.Instance)
            {
                HitStopManager.Initialize();
            }

            HitStopManager.Instance?.HitStop(duration);
        }

        /// <summary>
        /// Create a runtime instance of our materials
        /// </summary>
        protected virtual void InitMaterials()
        {
            Materials = new Material[spriteRenderers.Length];
            _flashTweens = new TidiTween<float>[spriteRenderers.Length];
            _originalColor = spriteRenderers[0].color;
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                Material instance = Instantiate(spriteRenderers[i].sharedMaterial);
                instance.name = $"{spriteRenderers[i].sharedMaterial.name}_Runtime_{i}";
                spriteRenderers[i].material = instance;
                Materials[i] = instance;
            }
        }

        #endregion

        public HitResponse OnHit(ref HitData damageData)
        {
            float finalDamage = DamageCalculator.CalculateDamage(in damageData);

            TakeDamage(finalDamage, damageData.attackDefinition.hitStopTime);

            return new HitResponse(
                response: Response.DamageEnemy,
                consumeCharges: true,
                finalDamage: finalDamage
            );
        }
    }
}