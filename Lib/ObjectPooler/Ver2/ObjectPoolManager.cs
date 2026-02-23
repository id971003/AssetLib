using System;
using System.Collections.Generic;
using UnityEngine;


public class ObjectPoolManager
{
    private readonly Dictionary<string, ObjectPoolBase> m_PoolDictionary;

    public ObjectPoolManager()
    {
        // 문자열 키 비교는 Ordinal 고정
        m_PoolDictionary = new Dictionary<string, ObjectPoolBase>(StringComparer.Ordinal);
    }

    // 특정 타입의 컴포넌트를 가진 프리팹을 등록
    public bool RegisterPrefab<T>(string _key, GameObject _prefab, int _recapacity = -1) where T : Component
    {
        if (string.IsNullOrEmpty(_key) || _prefab == null)
            return false;

        if (m_PoolDictionary.ContainsKey(_key))
            return false;

        var newPool = new ComponentGameObjectPool<T>();
        if (!newPool.SetPrefabObject(_prefab))
            return false;

        if (_recapacity != 0) // -1/0 모두 풀 기본 정책 사용
            newPool.Recapacity(_recapacity);

        m_PoolDictionary.Add(_key, newPool);
        return true;
    }

    // 특정 경로로부터 특정 타입의 컴포넌트를 가진 프리팹을 로드하여 등록 (Resources)
    public bool RegisterPrefabFromPath<T>(string _key, string _path, int _recapacity = -1) where T : Component
    {
        if (string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_path))
            return false;

        if (m_PoolDictionary.ContainsKey(_key))
            return false;

        GameObject prefab = Resources.Load<GameObject>(_path);
        if (prefab == null)
            return false;

        return RegisterPrefab<T>(_key, prefab, _recapacity);
    }

    // 프리팹 인스턴스를 직접 받아 등록 (이름은 기존 유지)
    public bool RegisterPrefabFromPath<T>(string _key, T _prefab, int _recapacity = -1) where T : Component
    {
        if (string.IsNullOrEmpty(_key) || _prefab == null)
            return false;

        if (m_PoolDictionary.ContainsKey(_key))
            return false;

        var pool = new ComponentGameObjectPool<T>();
        if (!pool.SetPrefabObject(_prefab.gameObject))
            return false;

        if (_recapacity != 0)
            pool.Recapacity(_recapacity);

        m_PoolDictionary.Add(_key, pool);
        return true;
    }

    // Addressables 경로에서 로드하여 등록
    public bool RegisterPrefabFromAddressablePath<T>(string _key, string _path, int _recapacity = -1) where T : Component
    {
        if (string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_path))
            return false;

        if (m_PoolDictionary.ContainsKey(_key))
            return false;

        GameObject prefab = ResourceSystem.Instance.LoadResource<GameObject>(
            ResourcePathType.Addressable, _path);

        if (prefab == null)
            return false;

        var comp = prefab.GetComponent<T>();
        if (comp == null)
        {
            Debug.Log($"Resource loaded but component <{typeof(T).Name}> not found. [Key:{_key}, Path:{_path}]");
            return false;
        }

        return RegisterPrefab<T>(_key, comp.gameObject, _recapacity);
    }

    public bool IsRegistered(string _key)
    {
        if (string.IsNullOrEmpty(_key))
            return false;
        return m_PoolDictionary.ContainsKey(_key);
    }

    public bool GetRegisteredPrefab<T>(string _key, out T _prefab) where T : Component
    {
        _prefab = default;
        if (!m_PoolDictionary.TryGetValue(_key, out var pool))
            return false;

        var componentPool = pool as ComponentGameObjectPool<T>;
        if (componentPool == null)
            return false;

        _prefab = componentPool.GetPrefabObject();
        return (_prefab != null);
    }

    public bool ReleasePool(string _key)
    {
        if (!m_PoolDictionary.TryGetValue(_key, out var pool))
        {
            Debug.Log($"ObjectPoolManager::ReleasePool() failed. [Key: {_key}]");
            return false;
        }

        pool.ReleaseAll();
        return true;
    }

    // 제네릭 시그니처는 유지하되, 캐스팅 없이 공통 인터페이스로 파괴 처리
    public bool DestroyPool<T>(string _key) where T : Component
    {
        if (!m_PoolDictionary.TryGetValue(_key, out var pool))
            return false;

        pool.DestroyAll();
        m_PoolDictionary.Remove(_key);
        return true;
    }

    public void DestroyAllPool()
    {
        foreach (var pool in m_PoolDictionary.Values)
            pool.DestroyAll();

        m_PoolDictionary.Clear();
    }

    public void ReleaseAllPool()
    {
        // Release는 풀 정의를 유지하는 편이 일반적
        foreach (var pool in m_PoolDictionary.Values)
            pool.ReleaseAll();
    }

    // 오브젝트 생성
    public T Create<T>(string _key) where T : Component
    {
        if (!m_PoolDictionary.TryGetValue(_key, out var pool))
            return null;

        var componentPool = pool as ComponentGameObjectPool<T>;
        if (componentPool == null)
            return null;

        return componentPool.Create();
    }

    public GameObject Create(string _key)
    {
        if (!m_PoolDictionary.TryGetValue(_key, out var pool))
            return null;

        var prefabPool = pool as PrefabGameObjectPool;
        if (prefabPool == null)
            return null;

        return prefabPool.Create();
    }

    // 오브젝트 제거
    public bool Release<T>(T _target) where T : Component
    {
        if (_target == null)
            return false;

        foreach (var pool in m_PoolDictionary.Values)
        {
            var componentPool = pool as ComponentGameObjectPool<T>;
            if (componentPool == null)
                continue;

            if (componentPool.Release(_target))
                return true;
        }
        return false;
    }

    public bool Release<T>(string _key, T _target) where T : Component
    {
        if (_target == null)
            return false;

        if (!m_PoolDictionary.TryGetValue(_key, out var pool))
            return false;

        var componentPool = pool as ComponentGameObjectPool<T>;
        if (componentPool == null)
            return false;

        return componentPool.Release(_target);
    }

    /* 검색 관련 */
    public IEnumerable<KeyValuePair<string, ObjectPoolBase>> GetPools()
    {
        return m_PoolDictionary;
    }

    public int GetEnabledCount<T>(string _key) where T : Component
    {
        if (!m_PoolDictionary.TryGetValue(_key, out var pool))
            return 0;

        var realPool = pool as ObjectPoolCommon<T>;
        if (realPool == null)
            return 0;

        return realPool.GetEnabledCount();
    }

    // key에 상관없이 T타입만 맞으면 전부 반환 (집계)
    public List<T> GetEnabledObjects<T>() where T : Component
    {
        var resultList = new List<T>();

        foreach (var kv in m_PoolDictionary)
        {
            var realPool = kv.Value as ObjectPoolCommon<T>;
            if (realPool == null)
                continue;

            // 용량 선확보로 재할당 최소화
            int addCount = realPool.GetEnabledCount();
            if (addCount <= 0)
                continue;

            int needed = resultList.Count + addCount;
            if (resultList.Capacity < needed)
                resultList.Capacity = needed;

            realPool.GetEnabledObjects(ref resultList, true);
        }
        return resultList;
    }

    public List<T> GetEnabledObjects<T>(string _key) where T : Component
    {
        if (!m_PoolDictionary.TryGetValue(_key, out var pool))
            return null;

        var realPool = pool as ObjectPoolCommon<T>;
        if (realPool == null)
            return null;

        return realPool.GetEnabledObjects();
    }

    public bool GetEnabledObjects<T>(string _key, ref List<T> _resultObjectList) where T : Component
    {
        if (_resultObjectList == null)
            return false;

        if (!m_PoolDictionary.TryGetValue(_key, out var pool))
            return false;

        var realPool = pool as ObjectPoolCommon<T>;
        if (realPool == null)
            return false;

        return realPool.GetEnabledObjects(ref _resultObjectList);
    }
}
