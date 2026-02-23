using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// List와 동일하지만 내부적으로 풀링을 하는 리스트 풀입니다.
/// 삭제시 표준 컬렉션 List 처럼 뒤의 요소들이 앞으로 당겨집니다.
/// </summary>
public class GenericListPool<T> : IEnumerable where T : class, new()
{
    private NormalObjectPool<T> m_Pool;
    private List<T> m_List;

    public int Capacity
    {
        get;
        private set;
    }

    public int Count
    {
        get { return m_List.Count; }
    }

    public GenericListPool()
    {
        m_Pool = new NormalObjectPool<T>();
        m_List = new List<T>();
    }

    public void SetOnNew(Action<T> _callback)
    {
        m_Pool.OnNewAction += _callback;
    }

    public void SetCustomInstantiateFunc(Func<int, T[]> _func)
    {
        m_Pool.SetCustomInstantiateFunc(_func);
    }

    public T Create()
    {
        T instance = m_Pool.Create();
        if (instance == null)
            return null;

        m_List.Add(instance);
        return instance;
    }

    public T GetObject(int _index)
    {
        if (_index < 0 || _index >= m_List.Count)
            return null;
        return m_List[_index];
    }

    public bool Release(int _index)
    {
        if (_index < 0 || _index >= m_List.Count)
            return false;

        var target = m_List[_index];

        m_List.RemoveAt(_index);
        return m_Pool.Release(target);
    }

    public bool Destroy(int _index)
    {
        if (_index < 0 || _index >= m_List.Count)
            return false;

        var target = m_List[_index];

        m_List.RemoveAt(_index);
        m_Pool.Destroy(target);
        return true;
    }

    public IEnumerator GetEnumerator()
    {
        return m_List.GetEnumerator();
    }

    public T this[int _index]
    {
        get
        {
            if (_index < 0 || _index >= m_List.Count)
                return null;

            return m_List[_index];
        }
    }

}
