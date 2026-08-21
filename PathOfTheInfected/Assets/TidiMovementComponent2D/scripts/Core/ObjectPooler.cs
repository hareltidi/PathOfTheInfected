using UnityEngine;
using TidiGenericObjectPooling;

namespace TidiMovementComponent2D.Core
{
    public class ObjectPooler : ObjectPoolManager
    {

        protected override void SetupEmpties()
        {
            base.SetupEmpties();

        }

        protected override GameObject SetParentObject(PoolType type)
        {
            return base.SetParentObject(type);
        }
    }
}