using System;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Damagable
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        #region IDamageable members
        [field: SerializeField] public bool IsDead { get; set; }
        [field: SerializeField] public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        #endregion

        #region Script members
        SpriteRenderer[] _spriteRenderers;
        Material[] _materials;
        [SerializeField] private float flashTime;
        [ColorUsage(true, true)]
        [SerializeField] private Color flashColor;
        [SerializeField] private EaseType damageFlashEaseType;
        [SerializeField] private GameObject visuals;
        #endregion


        private void Awake()
        {
            _spriteRenderers = visuals.GetComponentsInChildren<SpriteRenderer>();
            InitMaterials();
        }

        private void InitMaterials()
        {
            _materials = new Material[_spriteRenderers.Length];
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                _materials[i] = _spriteRenderers[i].material;
            }
        }

        private void Start()
        {
            CurrentHealth = MaxHealth;
        }


        public void TakeDamage(int damage)
        {

            if (CurrentHealth > 0)
            {
                CurrentHealth -= damage;
                FlashDamage();
                Debug.Log("ouch");
            }
            else if (!IsDead)
            {
                Die();
            }

        }

        public void Die()
        {
            IsDead = true;
            Debug.Log("death");
        }

        private void FlashDamage()
        {
            SetFlashColor();
            foreach (var t in _materials)
            {
                float currentAmount = t.GetFloat("_FlashAmount");
                TidiTweenManager.TweenFloat(this, currentAmount, 1, flashTime, (value) =>
                {
                    t.SetFloat("_FlashAmount", value);
                }).SetPingPong(2).SetEase(damageFlashEaseType);
            }
        }

        private void SetFlashColor()
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetColor("_FlashColor",  flashColor);
            }
        }
    }
}