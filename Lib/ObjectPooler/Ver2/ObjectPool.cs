using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class ObjectPoolBase
{
    public abstract void ReleaseAll();
    public abstract void DestroyAll();
}

public abstract class ObjectPoolCommon<T> : ObjectPoolBase
{
    public delegate void OnCreateHandler(T target);
    public delegate void OnBeforeReleaseHandler(T target);

    // 비활성 컨테이너
    protected readonly Stack<T> m_DiabledObjectList = new Stack<T>(16);
    // 활성 컨테이너
    protected readonly HashSet<T> m_EnabledObjectHashSet = new HashSet<T>();

    // 임시 버퍼(재사용)
    private static readonly List<T> s_TempList = new List<T>(128);

    public ulong m_TotalCreatedObjectCount = 0;

    private int m_MaxRecapacityCount = 5;
    public int MaxRecapacityCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_MaxRecapacityCount;
        set => m_MaxRecapacityCount = (value > 0) ? value : 1;
    }

    public event OnCreateHandler OnCreate;
    public event OnBeforeReleaseHandler OnBeforeRelease;

    protected abstract T[] InstantiateObjects(int _count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool Recapacity(int _count = -1)
    {
        if (_count <= 0)
            _count = m_MaxRecapacityCount;

        var newObjects = InstantiateObjects(_count);
        if (newObjects == null || newObjects.Length == 0)
            return false;

        m_TotalCreatedObjectCount += (ulong)newObjects.Length;

        for (int i = 0; i < newObjects.Length; ++i)
        {
            m_DiabledObjectList.Push(newObjects[i]);
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual T Create(bool _isActive = true)
    {
        if (m_DiabledObjectList.Count == 0)
        {
            if (Recapacity() == false)
                return default;
        }

        var newObject = m_DiabledObjectList.Pop();

        if (!m_EnabledObjectHashSet.Add(newObject))
        {
            return default;
        }

        OnCreate?.Invoke(newObject);
        return newObject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool Release(T _target)
    {
        if (_target == null)
            return false;

        OnBeforeRelease?.Invoke(_target);

        if (!m_EnabledObjectHashSet.Remove(_target))
            return false;

        m_DiabledObjectList.Push(_target);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Destroy(T _target)
    {
        if (_target == null)
            return;

        m_EnabledObjectHashSet.Remove(_target);

        if (m_DiabledObjectList.Count > 0)
        {
            s_TempList.Clear();
            foreach (var it in m_DiabledObjectList)
                if (!EqualityComparer<T>.Default.Equals(it, _target))
                    s_TempList.Add(it);

            m_DiabledObjectList.Clear();
            for (int i = 0; i < s_TempList.Count; ++i)
                m_DiabledObjectList.Push(s_TempList[i]);
            s_TempList.Clear();
        }
    }

    public override void ReleaseAll()
    {
        if (m_EnabledObjectHashSet.Count == 0)
            return;

        s_TempList.Clear();
        foreach (var it in m_EnabledObjectHashSet)
            s_TempList.Add(it);

        for (int i = s_TempList.Count - 1; i >= 0; --i)
            Release(s_TempList[i]);

        s_TempList.Clear();
    }

    public override void DestroyAll()
    {
        if (m_DiabledObjectList.Count > 0)
        {
            s_TempList.Clear();
            foreach (var it in m_DiabledObjectList)
                s_TempList.Add(it);

            for (int i = 0; i < s_TempList.Count; ++i)
                Destroy(s_TempList[i]);

            s_TempList.Clear();
            m_DiabledObjectList.Clear();
        }

        if (m_EnabledObjectHashSet.Count > 0)
        {
            s_TempList.Clear();
            foreach (var it in m_EnabledObjectHashSet)
                s_TempList.Add(it);

            for (int i = 0; i < s_TempList.Count; ++i)
                Destroy(s_TempList[i]);

            s_TempList.Clear();
            m_EnabledObjectHashSet.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetEnabledCount()
    {
        return m_EnabledObjectHashSet.Count;
    }

    public List<T> GetEnabledObjects()
    {
        return new List<T>(m_EnabledObjectHashSet);
    }

    public T GetFirstEnableObject()
    {
        foreach (var it in m_EnabledObjectHashSet)
            return it;
        return default;
    }

    public int GetEnabledObjects(ref List<T> _list, bool _doNotClear = true)
    {
        if (_list == null)
            return 0;

        if (!_doNotClear)
            _list.Clear();

        if (_list.Capacity < m_EnabledObjectHashSet.Count)
            _list.Capacity = m_EnabledObjectHashSet.Count;

        foreach (var it in m_EnabledObjectHashSet)
            _list.Add(it);

        return m_EnabledObjectHashSet.Count;
    }

    public bool GetEnabledObjects(ref List<T> _resultList)
    {
        if (_resultList == null)
            return false;

        _resultList.Clear();
        if (_resultList.Capacity < m_EnabledObjectHashSet.Count)
            _resultList.Capacity = m_EnabledObjectHashSet.Count;

        foreach (var it in m_EnabledObjectHashSet)
            _resultList.Add(it);

        return true;
    }

    public bool GetDisabledObjectList(ref List<T> _getListRef)
    {
        if (_getListRef == null)
            return false;

        _getListRef.Clear();

        if (_getListRef.Capacity < m_DiabledObjectList.Count)
            _getListRef.Capacity = m_DiabledObjectList.Count;

        foreach (var it in m_DiabledObjectList)
            _getListRef.Add(it);

        return true;
    }
}

/*
    컴포넌트를 가진 게임오브젝트 풀

    - 프리팹 미지정: 빈 게임오브젝트 생성 후 AddComponent<T>()
    - 프리팹 지정: 프리팹 기반 인스턴스
*/
public class ComponentGameObjectPool<T> : ObjectPoolCommon<T> where T : UnityEngine.Component
{
    private Transform m_TargetParent;

    public ComponentGameObjectPool() : base() { }
    public ComponentGameObjectPool(int _recapacity = -1) : base()
    {
        if (_recapacity > 1)
            Recapacity(_recapacity);
    }

    public Type ComponentType => typeof(T);

    private T m_PrefabObject;
    public bool SetPrefabObject(GameObject _prefab)
    {
        if (_prefab == null)
            return false;

        var component = _prefab.GetComponent<T>();
        if (component == null)
            return false;

        m_PrefabObject = component;
        return true;
    }

    public T GetPrefabObject() => m_PrefabObject;

    // 기존 생성분의 부모는 변경하지 않음
    public void SetParent(Transform _parent, bool _worldPositionStays = true)
    {
        if (_parent == null)
        {
            m_TargetParent = null;
            return;
        }

        foreach (var comp in m_DiabledObjectList)
            comp.transform.SetParent(_parent, false);

        foreach (var comp in m_EnabledObjectHashSet)
            comp.transform.SetParent(_parent, _worldPositionStays);

        m_TargetParent = _parent;
    }

    protected override T[] InstantiateObjects(int _count)
    {
        if (_count <= 0)
            _count = 1;

        var components = new T[_count];

        for (int i = 0; i < _count; ++i)
        {
            GameObject go;
            if (m_PrefabObject == null)
            {
                go = new GameObject(typeof(T).Name);
                go.transform.SetParent(m_TargetParent, false);
                components[i] = go.AddComponent<T>();
            }
            else
            {
                go = GameObject.Instantiate(m_PrefabObject.gameObject, m_TargetParent, false);
                go.name = m_PrefabObject.gameObject.name;
                components[i] = go.GetComponent<T>();
            }

            go.SetActive(false);
        }

        return components;
    }

    public override T Create(bool _isActive = true)
    {
        var newComponent = base.Create();
        if (newComponent == null)
            return null;

        if (_isActive)
            newComponent.gameObject.SetActive(true);

        return newComponent;
    }

    public override bool Release(T _target)
    {
        if (_target == null)
            return false;

        if (!base.Release(_target))
            return false;

        _target.gameObject.SetActive(false);
        return true;
    }

    public override void Destroy(T _target)
    {
        base.Destroy(_target);
        if (_target != null)
        {
            if (Application.isPlaying)
                GameObject.Destroy(_target.gameObject);
            else
                GameObject.DestroyImmediate(_target.gameObject);
        }
    }
}

/*
    프리팹 기반 게임오브젝트 풀

    - 프리팹 미지정: 빈 게임오브젝트 생성
    - 신규 객체는 비활성 상태로 보관
*/
public class PrefabGameObjectPool : ObjectPoolCommon<GameObject>
{
    private GameObject m_PrefabObject;

    public void SetPrefabObject(GameObject _prefab)
    {
        m_PrefabObject = _prefab;
    }

    protected override GameObject[] InstantiateObjects(int _count)
    {
        if (_count <= 0) _count = 1;

        var gameObjects = new GameObject[_count];

        if (m_PrefabObject == null)
        {
            for (int i = 0; i < _count; ++i)
            {
                var go = new GameObject("PooledGO");
                go.SetActive(false);
                gameObjects[i] = go;
            }
        }
        else
        {
            for (int i = 0; i < _count; ++i)
            {
                var go = GameObject.Instantiate(m_PrefabObject);
                go.name = m_PrefabObject.name;
                go.SetActive(false);
                gameObjects[i] = go;
            }
        }

        return gameObjects;
    }

    public override GameObject Create(bool _isActive = true)
    {
        var newObject = base.Create();
        if (newObject == null)
            return null;

        if (_isActive)
            newObject.SetActive(true);

        return newObject;
    }

    public override bool Release(GameObject _target)
    {
        if (_target == null)
            return false;

        if (!base.Release(_target))
            return false;

        _target.SetActive(false);
        return true;
    }

    public override void Destroy(GameObject _target)
    {
        base.Destroy(_target);
        if (_target != null)
        {
            if (Application.isPlaying)
                GameObject.Destroy(_target);
            else
                GameObject.DestroyImmediate(_target);
        }
    }
}

public class NormalObjectPool<T> : ObjectPoolCommon<T> where T : new()
{
    public Action<T> OnNewAction;

    public NormalObjectPool() : base() { }
    public NormalObjectPool(int _recapacity = -1) : base()
    {
        if (_recapacity > 1)
            Recapacity(_recapacity);
    }

    public Type ComponentType => typeof(T);

    // 커스텀 생성 경로
    private Func<int, T[]> m_CustomInstantiateFunc;
    public void SetCustomInstantiateFunc(Func<int, T[]> _func)
    {
        m_CustomInstantiateFunc = _func;
    }

    protected override T[] InstantiateObjects(int _count)
    {
        if (m_CustomInstantiateFunc != null)
            return m_CustomInstantiateFunc.Invoke(_count);

        if (_count <= 0) _count = 1;

        var objects = new T[_count];
        for (int i = 0; i < _count; ++i)
        {
            var obj = new T();
            OnNewAction?.Invoke(obj);
            objects[i] = obj;
        }
        return objects;
    }

    public override T Create(bool _isActive = true)
    {
        return base.Create();
    }

    public override bool Release(T _target)
    {
        if (_target == null)
            return false;

        return base.Release(_target);
    }

    public override void Destroy(T _target)
    {
        base.Destroy(_target);
    }
}