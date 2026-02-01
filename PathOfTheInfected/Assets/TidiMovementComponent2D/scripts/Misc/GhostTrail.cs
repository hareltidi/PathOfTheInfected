using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace TidiMovementComponent2D.Misc
{
    public class GhostTrail : MonoBehaviour
    {
        [FormerlySerializedAs("_testGhost")] [SerializeField] private GameObject testGhost;

        [FormerlySerializedAs("_numberOfTrails")] [SerializeField] private int numberOfTrails = 3;

        [FormerlySerializedAs("_fadeTime")] [SerializeField] private float fadeTime = 1f;

        [FormerlySerializedAs("PlayerSpriteRenderers")] public SpriteRenderer[] playerSpriteRenderers;

        private readonly int _matAlpha = Shader.PropertyToID("_GhostAlpha");

        private GameObject[] _ghosts;

        private Vector3[] _positions;

        private Quaternion[] _rotations;

        private Vector3[] _scales;

        private void Awake()
        {
            _positions = new Vector3[playerSpriteRenderers.Length];
            _rotations = new Quaternion[playerSpriteRenderers.Length];
            _scales = new Vector3[playerSpriteRenderers.Length];
            _ghosts = new GameObject[numberOfTrails];
        }

        public void LeaveGhostTrail(float time)
        {
            StartCoroutine(LeaveTrail(time));
        }

        private IEnumerator LeaveTrail(float time)
        {
            var numberSpawned = 0;
            while (numberSpawned < numberOfTrails)
                for (var i = 0; i < numberOfTrails; i++)
                {
                    Spawn();
                    yield return new WaitForSeconds(time / numberOfTrails);
                    numberSpawned++;
                }
        }

        private void Spawn()
        {
            for (var i = 0; i < playerSpriteRenderers.Length; i++)
            {
                _positions[i] = playerSpriteRenderers[i].transform.position;
                _rotations[i] = playerSpriteRenderers[i].transform.rotation;
                _scales[i] = playerSpriteRenderers[i].transform.localScale;
            }

            var gameObject = Instantiate(testGhost, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
            var componentsInChildren = gameObject.GetComponentsInChildren<SpriteRenderer>();
            for (var j = 0; j < componentsInChildren.Length; j++)
            {
                componentsInChildren[j].transform.position = _positions[j];
                componentsInChildren[j].transform.rotation = _rotations[j];
                componentsInChildren[j].transform.localScale = _scales[j];
            }

            gameObject.SetActive(true);
            StartCoroutine(FadeGhost(componentsInChildren, gameObject));
        }

        private IEnumerator FadeGhost(SpriteRenderer[] rends, GameObject go)
        {
            var elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                for (var i = 0; i < rends.Length; i++)
                {
                    var value = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                    rends[i].material.SetFloat(_matAlpha, value);
                }

                yield return null;
            }

            Destroy(go);
        }
    }
}