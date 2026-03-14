using System;
using PathOfTheInfected.Combat;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Enemy.Health
{
    public class EnemyHealth : MonoBehaviour, IHitResponder, IHitStoppable
    {
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
        private  TidiTween<float> _flashTween;

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
            FlashDamage();
            HitStop(hitStopTime);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void FlashDamage()
        {
            SetFlashColor(flashColor);
            int i = 0;
            foreach (var t in Materials)
            {
                if (_flashTween != null)
                {
                    _flashTween.FullKill();
                }

                Material localMat = t;
                localMat.name += $"Hit Flash Material_{i}";
                float currentAmount = localMat.GetFloat("_FlashAmount");
                _flashTween = TidiTweenManager
                    .TweenFloat(localMat, currentAmount, 1, flashTime,
                        (value) => { localMat.SetFloat("_FlashAmount", value); }).SetPingPong(2)
                    .SetEase(damageFlashEaseType);
                i++;
            }
            SetFlashColor(_originalColor);

        }

        ///<summary>
        /// Set the flash color when we need to flash
        ///</summary>
        /// <param name="color">The color the flash should be in</param>
        private void SetFlashColor(Color color)
        {
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i].SetColor("_FlashColor", color);
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
            SpriteRenderer[] spriteRenderers = GetComponents<SpriteRenderer>();
            Materials = new Material[spriteRenderers.Length];
            _originalColor = spriteRenderers[0].color;
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                Material instance = Instantiate(spriteRenderers[i].sharedMaterial);
                spriteRenderers[i].material = instance;
                Materials[i] = instance;
            }
        }

        #endregion

        public HitResponse OnHit(HitData damageData)
        {
            float finalDamage = DamageCalculator.CalculateDamage(damageData);

            TakeDamage(finalDamage, damageData.attackDefinition.hitStopTime);

            return new HitResponse(
                response: Response.DamageEnemy,
                consumeCharges: true,
                finalDamage: finalDamage
            );
        }
    }
}