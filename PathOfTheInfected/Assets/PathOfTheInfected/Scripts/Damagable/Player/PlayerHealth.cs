using System;
using PathOfTheInfected.Enemy;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Damagable
{
    public class PlayerHealth : MonoBehaviour, IDamageable, IHitStoppable
    {
        #region IDamageable members

        [field: SerializeField] public bool IsDead { get; set; }
        [field: SerializeField] public int MaxHealth { get; set; }
        public GameObject GameObject { get; set; }
        public int CurrentHealth { get; set; }

        #endregion

        #region Script members

        private SpriteRenderer[] _spriteRenderers;
        private Material[] _materials;

        ///<summary>
        ///Checks if wer'e hit stopped (meaning the time is currently freezing)
        ///</summary>
        private bool _isHitStopped = false;

        private Rigidbody2D Rb => GetComponent<Rigidbody2D>();
        private float _hitStopTimer;
        [SerializeField] private float flashTime;

        [ColorUsage(true, true)] [SerializeField]
        private Color flashColor;

        [SerializeField] private EaseType damageFlashEaseType;
        [SerializeField] private GameObject visuals;

        #endregion


        private void Awake()
        {
            _spriteRenderers = visuals.GetComponentsInChildren<SpriteRenderer>();
            InitMaterials();
        }

        ///<summary>
        ///Initiating the materials for our player to make hit flash work only for this object and not all of the other
        ///objects who have the hit effect material to be affected.
        ///</summary>
        private void InitMaterials()
        {
            _materials = new Material[_spriteRenderers.Length];
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                Material instance = Instantiate(_spriteRenderers[i].sharedMaterial);
                _spriteRenderers[i].material = instance;
                _materials[i] = instance;
            }
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


        public void TakeDamage(DamageData damageData)
        {
            if (CurrentHealth > 0)
            {
                CurrentHealth -= damageData.Damage;
                FlashDamage();
                HitStop(damageData.HitStopTime);
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

        ///<summary>
        ///Make the player flash.
        ///</summary>
        private void FlashDamage()
        {
            SetFlashColor(flashColor);
            int i = 0;
            foreach (var t in _materials)
            {
                Material localMat = t;
                localMat.name += $"Hit Flash Material_{i}";
                float currentAmount = localMat.GetFloat("_FlashAmount");
                TidiTweenManager
                    .TweenFloat(localMat, currentAmount, 1, flashTime,
                        (value) => { localMat.SetFloat("_FlashAmount", value); }).SetPingPong(2)
                    .SetEase(damageFlashEaseType);
                i++;
            }
        }

        ///<summary>
        /// Set the flash color when we need to flash
        ///</summary>
        /// <param name="color">The color the flash should be in</param>
        private void SetFlashColor(Color color)
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetColor("_FlashColor", color);
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
                new GameObject("HitStopManager").AddComponent<HitStopManager>();
            }

            HitStopManager.Instance?.HitStop(duration);
        }
    }
}
