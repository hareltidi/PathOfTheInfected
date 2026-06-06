using System.Collections.Generic;
using PathOfTheInfected.Combat;
using PathOfTheInfected.Damagable.Messages;
using PathOfTheInfected.Enemy;
using TidiGameplayMessaging.Core;
using TidiMovementComponent2D.Core;
using TidiTweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PathOfTheInfected.Damagable
{
    public class PlayerHealth : MonoBehaviour, IHitResponder, IHitStoppable
    {
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        private PlayerSm _playerOwner;

        private void Awake()
        {
            SyncRuntimeRenderersAndMaterials();
            _playerOwner = PlayerSm.Instance;
        }

        private void Start()
        {
            CurrentHealth = MaxHealth;
        }

        private void Update()
        {
            if (_isHitStopped)
            {
                _hitStopTimer -= Time.unscaledDeltaTime;

                if (_hitStopTimer <= 0f)
                {
                    _isHitStopped = false;
                    Rb.simulated = true;
                }
            }
        }

        /// <summary>
        ///  Apply hit stop
        /// </summary>
        /// <param name="duration">How long should we freeze time?</param>
        public void HitStop(float duration)
        {
            if (!HitStopManager.Instance)
            {
                HitStopManager.Initialize();
            }

            HitStopManager.Instance?.HitStop(duration);
        }

        // Runtime scarf segments can add SpriteRenderers after Awake, so we sync before flashing.
        private void SyncRuntimeRenderersAndMaterials()
        {
            if (!visuals)
            {
                return;
            }

            var allRenderers = visuals.GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < allRenderers.Length; i++)
            {
                var spriteRenderer = allRenderers[i];
                if (!spriteRenderer || !_registeredRenderers.Add(spriteRenderer))
                {
                    continue;
                }

                var sharedMaterial = spriteRenderer.sharedMaterial;
                if (!sharedMaterial)
                {
                    continue;
                }

                var instance = Instantiate(sharedMaterial);
                instance.name = $"{sharedMaterial.name}_RuntimeFlash_{_materials.Count}";
                spriteRenderer.material = instance;

                _materials.Add(instance);
                _flashTweens.Add(null);

                if (!_hasOriginalColor)
                {
                    _originalColor = spriteRenderer.color;
                    _hasOriginalColor = true;
                }
            }
        }


        public void TakeDamage(float finalDamage, float hitStopTime)
        {
            if (IsDead) return;

            CurrentHealth -= finalDamage;
            TidiGameplayMessagingSubsystem.Instance.Broadcast<PlayerHitChannel, PlayerHealthChangedPayload>(new PlayerHealthChangedPayload
            {
                NewHealth = CurrentHealth,
                Type = HealthChangeType.Damage
            });
            FlashDamage();
            HitStop(hitStopTime);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            IsDead = true;
            Destroy(gameObject);
        }

        /// <summary>
        /// Make the player flash.
        /// </summary>
        private void FlashDamage()
        {
            SyncRuntimeRenderersAndMaterials();
            if (_materials.Count == 0)
            {
                return;
            }

            SetFlashColor(flashColor);
            for (var i = 0; i < _materials.Count; i++)
            {
                if (_flashTweens[i] != null)
                {
                    _flashTweens[i].FullKill();
                }

                var localMat = _materials[i];
                var currentAmount = localMat.GetFloat(FlashAmountId);
                _flashTweens[i] = TidiTweenManager
                    .TweenFloat(localMat, currentAmount, 1, flashTime,
                        value => { localMat.SetFloat(FlashAmountId, value); }).SetPingPong(2)
                    .SetEase(damageFlashEaseType);
            }
            SetFlashColor(_originalColor);
        }

        /// <summary>
        /// Set the flash color when we need to flash
        /// </summary>
        /// <param name="color">The color the flash should be in</param>
        private void SetFlashColor(Color color)
        {
            for (var i = 0; i < _materials.Count; i++)
            {
                _materials[i].SetColor(FlashColorId, color);
            }
        }

        private void OnDestroy()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex );
            for (var i = 0; i < _flashTweens.Count; i++)
            {
                _flashTweens[i]?.FullKill();
            }
        }

        public HitResponse OnHit(ref HitData damageData)
        {
            float finalDamage = DamageCalculator.CalculateDamage(in damageData);

            TakeDamage(finalDamage, damageData.attackDefinition.hitStopTime);

            if (damageData.attackDir != Vector2.zero)
            {
                _playerOwner?.IncrementHorizontalVelocity(damageData.knockbackStrength * Mathf.Sign(damageData.attackDir.x));
            }

            return new HitResponse(
                response: Response.DamagePlayer,
                consumeCharges: true,
                finalDamage: finalDamage
            );
        }

        #region Health members

        [field: SerializeField] public bool IsDead { get; set; }
        [field: SerializeField] public float MaxHealth { get; set; }
        public GameObject GameObject { get; set; }
        public float CurrentHealth { get; set; }

        #endregion

        #region Script members

        private readonly List<Material> _materials = new();
        private readonly List<TidiTween<float>> _flashTweens = new();
        private readonly HashSet<SpriteRenderer> _registeredRenderers = new();

        /// <summary>
        ///     Checks if wer'e hit stopped (meaning the time is currently freezing)
        /// </summary>
        private bool _isHitStopped;

        private Rigidbody2D Rb => GetComponent<Rigidbody2D>();
        private float _hitStopTimer;
        [SerializeField] private float flashTime;

        [ColorUsage(true, true)]
        [SerializeField]
        private Color flashColor;

        [SerializeField] private EaseType damageFlashEaseType;
        [SerializeField] private GameObject visuals;
        private Color _originalColor;
        private bool _hasOriginalColor;

        #endregion
    }
}
