using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName =  "EnemyStateBase", menuName = "Enemy/States/Core/EnemyStateBase")]
    public class EnemyBaseState : ScriptableObject
    {
       #region protected Fields
       protected Enemy _enemy;
       protected EnemyStateMachine _stateMachine;
       #endregion

        #region Virtual Methods
        public virtual void StateInit(Enemy enemy, EnemyStateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
        }

        public virtual void StateEnter()
        {

        }

        public virtual void StateExit()
        {

        }

        public virtual void StateUpdate()
        {
            TransitionChecks();
        }

        public virtual void StateFixedUpdate()
        {

        }

        public virtual void TransitionChecks()
        {

        }

        #endregion
    }
}
