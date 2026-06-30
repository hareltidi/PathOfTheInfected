using System;
using PathOfTheInfected.Combat;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Scripts
{
    public class GOEnabler : MonoBehaviour
    {
        public GameObject objectToEnable;
        public Vector2 size = new Vector2(10f, 5f);
        public LayerMask spottableMask;

        private void Start()
        {
            if (objectToEnable)
            {
                objectToEnable.SetActive(false);
            }
        }

        private void Update()
        {
            Collider2D hit = Physics2D.OverlapBox(transform.position, size, 0f, spottableMask);

            if (hit)
            {
                IHitResponder hitResponder = hit.gameObject.GetComponent<IHitResponder>();

                if (hitResponder != null)
                {
                    objectToEnable.SetActive(true);
                    Destroy(gameObject);
                }
            }
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}