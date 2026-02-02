using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PathOfTheInfected.Enemy
{
    public enum InitialFacingDirection
    {
        Left = 0,
        Right = 1
    }


    public class Enemy : MonoBehaviour, IDamageable, IEnemyMoveable
    {
        #region Interface Variables
        #region IDamageable

        public bool IsDead { get; set; }
       [field: SerializeField] public int CurrentHealth { get; set; }

        public int MaxHealth { get; set; }
        #endregion

        #region IEnemyMoveable

        public Rigidbody2D RB { get; set; }
        public bool IsFacingRight { get; set; } = true;

        #endregion
        #endregion

        #region Damage

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
        }

        public void Die()
        {
            Destroy(gameObject);
        }

        #endregion

        #region Plug-In States
        public EnemyStateMachine stateMachine;
        public EnemyBaseState noSpottableDetectedState;
        public EnemyBaseState spottableDetectedState;
        public EnemyBaseState spottableInAttackRangeState;
        #endregion

        #region EnemyMovement

        public float moveSpeed = 1f;
        public float acceleration = 1f;
        public InitialFacingDirection initialFacingDirection = InitialFacingDirection.Right;


        public void MoveEnemy(Vector2 velocity)
        {
           var b = Mathf.Sign(velocity.x) * moveSpeed;
           var t = Mathf.Clamp01(acceleration * Time.fixedDeltaTime);
           velocity.x = Mathf.Lerp(velocity.x, b, t);
           CheckForLeftOrRightFacing(velocity);
           if (Mathf.Abs(velocity.x - b) > 0.0099999997764825821) return;
           velocity.x = b;
        }

        public void CheckForLeftOrRightFacing(Vector2 velocity)
        {
            if (IsFacingRight && velocity.x < 0f)
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                IsFacingRight = !IsFacingRight;
            }
            else if (!IsFacingRight && velocity.x > 0f)
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                IsFacingRight = !IsFacingRight;
            }
        }

        #endregion

        #region Members
        public bool isSpottableDetected = false;
        public bool isSpottableInAttackRange = false;
        #endregion

        private void Awake()
        {
            stateMachine = new EnemyStateMachine();
            noSpottableDetectedState = Instantiate(noSpottableDetectedState);
            spottableDetectedState = Instantiate(spottableDetectedState);
            spottableInAttackRangeState = Instantiate(spottableInAttackRangeState);
        }

        private void Start()
        {
            noSpottableDetectedState.StateInit(this, stateMachine);
            spottableDetectedState.StateInit(this, stateMachine);
            spottableInAttackRangeState.StateInit(this, stateMachine);

            stateMachine?.InitializeDefaultState(noSpottableDetectedState);
        }

        private void Update()
        {
            stateMachine?.CurrentState.StateUpdate();
        }

        private void FixedUpdate()
        {
            stateMachine?.CurrentState.StateFixedUpdate();
        }

        private void OnDrawGizmosSelected()
        {
            EnemyWanderSO test = (EnemyWanderSO)noSpottableDetectedState;
            if (test)
            {
                Vector2 maxPos =
            }
        }
    }
}