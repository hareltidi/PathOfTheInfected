using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    public interface ISpottable
    {
        Transform transform { get; set; }
    }
}