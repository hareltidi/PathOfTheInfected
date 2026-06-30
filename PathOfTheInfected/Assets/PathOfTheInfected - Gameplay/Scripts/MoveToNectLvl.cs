using System;
using PathOfTheInfected.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PathOfTheInfected___Gameplay.Scripts
{
    public class MoveToNectLvl : MonoBehaviour
    {
        public Vector2 size;
        public LayerMask spottableMask;

        private bool _touched;
        private void Update()
        {
            if (_touched)  return;

            Collider2D hit = Physics2D.OverlapBox(transform.position, size, 0f,spottableMask);

            IHitResponder hitResponder = hit?.gameObject.GetComponent<IHitResponder>();

            if (hitResponder != null)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                _touched = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}