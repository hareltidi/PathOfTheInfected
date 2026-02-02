namespace PathOfTheInfected.Enemy
{
    public class EnemyBaseState
    {
        protected Enemy _enemy;
        protected EnemyStateMachine _stateMachine;

        public EnemyBaseState(Enemy enemy, EnemyStateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
        }


        #region Virtual Methods

        virtual public void StateEnter()
        {

        }

        virtual public void StateExit()
        {

        }

        virtual public void StateUpdate()
        {

        }

        virtual public void StateFixedUpdate()
        {

        }
        #endregion
    }
}
