using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace TidiGenericObjectPooling
{
    public class ObjectPoolManager : MonoBehaviour
    {
        #region Initial Variables
        public static ObjectPoolManager Instance;
        [SerializeField] protected bool bAddToDontDestroyOnLoad = false;
        protected GameObject emptyHolder;
        // Feel free to add more empty holders for some better organizations.
        [SerializeField] protected GameObject gameObjectEmptyHolder;
        [SerializeField] protected GameObject soundsEmptyHolder;
        [SerializeField] protected GameObject particleEmptyHolder;

        private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPools;
        private static Dictionary<GameObject, GameObject> _cloneToPrefabMap;
        #endregion

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            _objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
            _cloneToPrefabMap = new Dictionary<GameObject, GameObject>();
            SetupEmpties();
        }


        #region Creating The pool

        private static void CreatePool(GameObject go, Vector3 parent, Quaternion rotation, PoolType type)
        {
            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(

               createFunc: () => CreateObject(go, parent, rotation, type),
               actionOnGet: OnGetObject,
               actionOnRelease: OnReleasedObject,
               actionOnDestroy: OnDestroyObject
               );
            _objectPools.Add(go, pool);
        }

        private static void CreatePool(GameObject go, Transform parent, Quaternion rotation, PoolType type)
        {
            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(

               createFunc: () => CreateObject(go, parent, rotation, type),
               actionOnGet: OnGetObject,
               actionOnRelease: OnReleasedObject,
               actionOnDestroy: OnDestroyObject
               );
            _objectPools.Add(go, pool);
        }

        private static GameObject CreateObject(GameObject go, Vector3 parent, Quaternion rotation, PoolType type)
        {
            go.SetActive(false);

            GameObject obj = Instantiate(go, parent, rotation);

            go.SetActive(true);

            GameObject parentObj = SetParentForObject(type);

            obj.transform.SetParent(parentObj.transform);

            return obj;
        }

        private static GameObject CreateObject(GameObject go, Transform parent, Quaternion rotation, PoolType type)
        {
            go.SetActive(false);

            GameObject obj = Instantiate(go, parent);

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = rotation;
            obj.transform.localScale = Vector3.one;

            go.SetActive(true);

            return obj;
        }

        private static void OnGetObject(GameObject go)
        {
            // optional logic
        }

        private static void OnReleasedObject(GameObject go)
        {
            go.SetActive(false);
        }

        private static void OnDestroyObject(GameObject go)
        {
            if (_cloneToPrefabMap.ContainsKey(go))
            {
                _cloneToPrefabMap.Remove(go);
            }
        }

        private static GameObject SetParentForObject(PoolType type)
        {
            return Instance.SetParentObject(type);
        }

        private static T SpawnObject<T>(GameObject goToSpawn, Vector3 parent, Quaternion spawnRotation, PoolType type) where T : Object
        {
            if (!_objectPools.ContainsKey(goToSpawn))
            {
                CreatePool(goToSpawn, parent, spawnRotation, type);
            }
            GameObject obj = _objectPools[goToSpawn].Get();

            if (obj)
            {
                if (!_cloneToPrefabMap.ContainsKey(obj))
                {
                    _cloneToPrefabMap.Add(obj, goToSpawn);
                }
                obj.transform.position = parent;
                obj.transform.rotation = spawnRotation;
                obj.SetActive(true);

                if (typeof(T) == typeof(GameObject))
                {
                    return obj as T;
                }

                T component = obj.GetComponent<T>();

                if (!component)
                {
                    Debug.LogError($"Object {goToSpawn.name} doesn't have a component of type {typeof(T)}!");
                    return null;
                }

                return component;
            }
            return null;
        }

        private static T SpawnObject<T>(GameObject goToSpawn, Transform parent, Quaternion spawnRotation, PoolType type) where T : Object
        {
            if (!_objectPools.ContainsKey(goToSpawn))
            {
                CreatePool(goToSpawn, parent, spawnRotation, type);
            }
            GameObject obj = _objectPools[goToSpawn].Get();

            if (obj)
            {
                if (!_cloneToPrefabMap.ContainsKey(obj))
                {
                    _cloneToPrefabMap.Add(obj, goToSpawn);
                }
                obj.transform.SetParent(parent);
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = spawnRotation;
                obj.SetActive(true);

                if (typeof(T) == typeof(GameObject))
                {
                    return obj as T;
                }

                T component = obj.GetComponent<T>();

                if (!component)
                {
                    Debug.LogError($"Object {goToSpawn.name} doesn't have a component of type {typeof(T)}!");
                    return null;
                }

                return component;
            }
            return null;
        }
        #endregion

        #region virtual functions
        protected virtual void SetupEmpties()
        {
            emptyHolder = new("ObjectPools");

            gameObjectEmptyHolder = new GameObject("GameObjects Pool");
            gameObjectEmptyHolder.transform.SetParent(emptyHolder.transform);

            soundsEmptyHolder = new GameObject("Sounds Pool");
            soundsEmptyHolder.transform.SetParent(emptyHolder.transform);

            particleEmptyHolder = new GameObject("Particles Pool");
            particleEmptyHolder.transform.SetParent(emptyHolder.transform);

            if (bAddToDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObjectEmptyHolder.transform.root);
            }

            if (bAddToDontDestroyOnLoad)
            {
                DontDestroyOnLoad(emptyHolder.transform.root);
            }
        }

        protected virtual GameObject SetParentObject(PoolType type)
        {
            switch (type)
            {
                case PoolType.GameObjects:
                    return gameObjectEmptyHolder;
                case PoolType.Sounds:
                    return soundsEmptyHolder;
                case PoolType.Particles:
                    return particleEmptyHolder;
                default:
                    return null;

            }
        }
        #endregion

        #region Public Methods

        public static T SpawnObject<T>(T TypePrefab, Vector3 spawnPosition, Quaternion spawnRotation, PoolType type) where T : Component
        {
            return SpawnObject<T>(TypePrefab.gameObject, spawnPosition, spawnRotation, type);
        }

        public static GameObject SpawnObject(GameObject goToSpawn, Vector3 spawnPosition, Quaternion spawnRotation, PoolType type)
        {
            return SpawnObject<GameObject>(goToSpawn, spawnPosition, spawnRotation, type);
        }

        public static void ReturnToPool(GameObject go, PoolType type)
        {
            if (_cloneToPrefabMap.TryGetValue(go, out GameObject prefab))
            {
                GameObject parentObj = SetParentForObject(type);

                if (go.transform.parent != parentObj.transform)
                {
                    go.transform.SetParent(parentObj.transform);
                }

                if (_objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
                {
                    pool.Release(go);
                }
            }
            else
            {
                Debug.LogWarning("Trying to return to pool an object that isn't pooled: " + go.name);
            }
        }
        #endregion
    }
}
