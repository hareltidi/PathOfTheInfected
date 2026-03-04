using PathOfTheInfected.Combat;
using PathOfTheInfected.Enemy;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Damagable
{
    public class PlayerHealth : MonoBehaviour, IHitResponder, IHitStoppable
    {
        private void Awake()
        {
            _spriteRenderers = visuals.GetComponentsInChildren<SpriteRenderer>();
            InitMaterials();
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
            if (!HitStopManager.Instance) HitStopManager.Initialize();

            HitStopManager.Instance?.HitStop(duration);
        }

        /// <summary>
        ///     Initiating the materials for our player to make hit flash work only for this object and not all the other
        ///     objects who have the hit effect material to be affected.
        /// </summary>
        private void InitMaterials()
        {
            _materials = new Material[_spriteRenderers.Length];
            for (var i = 0; i < _spriteRenderers.Length; i++)
            {
                var instance = Instantiate(_spriteRenderers[i].sharedMaterial);
                _spriteRenderers[i].material = instance;
                _materials[i] = instance;
            }
        }


        public void TakeDamage(int finalDamage, float hitStopTime)
        {
            if (CurrentHealth > 0)
            {
                CurrentHealth -= finalDamage;
                FlashDamage();
                HitStop(hitStopTime);
            }
            else if (!IsDead)
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
        ///     Make the player flash.
        /// </summary>
        private void FlashDamage()
        {
            SetFlashColor(flashColor);
            var i = 0;
            foreach (var t in _materials)
            {
                var localMat = t;
                localMat.name += $"Hit Flash Material_{i}";
                var currentAmount = localMat.GetFloat("_FlashAmount");
                TidiTweenManager
                    .TweenFloat(localMat, currentAmount, 1, flashTime,
                        value => { localMat.SetFloat("_FlashAmount", value); }).SetPingPong(2)
                    .SetEase(damageFlashEaseType);
                i++;
            }
        }

        /// <summary>
        ///     Set the flash color when we need to flash
        /// </summary>
        /// <param name="color">The color the flash should be in</param>
        private void SetFlashColor(Color color)
        {
            for (var i = 0; i < _materials.Length; i++) _materials[i].SetColor("_FlashColor", color);
        }

        public HitResponse OnHit(HitData damageData)
        {
            int finalDamage = DamageCalculator.CalculateDamage(damageData);

            TakeDamage(finalDamage, damageData.attackDefinition.hitStopTime);

            return new HitResponse(
                response: Response.DamagePlayer,
                consumeCharges: true,
                finalDamage: finalDamage
            );
        }

        #region IDamageable members

        [field: SerializeField] public bool IsDead { get; set; }
        [field: SerializeField] public int MaxHealth { get; set; }
        public GameObject GameObject { get; set; }
        public int CurrentHealth { get; set; }

        #endregion

        #region Script members

        private SpriteRenderer[] _spriteRenderers;
        private Material[] _materials;

        /// <summary>
        ///     Checks if wer'e hit stopped (meaning the time is currently freezing)
        /// </summary>
        private bool _isHitStopped;

        private Rigidbody2D Rb => GetComponent<Rigidbody2D>();
        private float _hitStopTimer;
        [SerializeField] private float flashTime;

        [ColorUsage(true, true)] [SerializeField]
        private Color flashColor;

        [SerializeField] private EaseType damageFlashEaseType;
        [SerializeField] private GameObject visuals;

        #endregion
    }
}