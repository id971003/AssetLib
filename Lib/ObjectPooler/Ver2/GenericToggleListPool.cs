using System;
using System.Collections;
using System.Collections.Generic;


public class GenericToggleListPool<T> : IEnumerable where T : class, new()
{
    private NormalObjectPool<T> m_Pool;
    private List<T> m_EnableList;
    private List<T> m_DisableList;
    private List<T> m_TempList;

    public int m_Capacity;
    public int Count
    {
        get { return EnableCount + DisableCount; }
    }
    public int EnableCount
    {
        get { return m_EnableList.Count; }
    }
    public int DisableCount
    {
        get { return m_DisableList.Count; }
    }

    public GenericToggleListPool(int _capacity = 2)
    {
        m_Capacity = _capacity;
        if (m_Capacity <= 2)
            m_Capacity = 2;
        m_Pool = new NormalObjectPool<T>();
        m_EnableList = new List<T>();
        m_DisableList = new List<T>();
        m_TempList = new List<T>();
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

        m_DisableList.Add(instance);
        return instance;
    }
    public List<T> CreateOnCapacity()
    {
        m_TempList.Clear();
        for (int i = 0; i < m_Capacity; i++)
        {
            T instance = m_Pool.Create();
            if (instance == null)
                return null;

            m_DisableList.Add(instance);
            m_TempList.Add(instance);
        }
        m_Capacity *= 2;
        return m_TempList;
    }

    public T EnableObject()
    {
        if (m_DisableList.Count == 0)
            return null;

        T obj = m_DisableList[0];
        m_DisableList.RemoveAt(0);
        m_EnableList.Add(obj);
        return obj;
    }

    public T EnableObject(int _index)
    {
        if (_index < 0 || _index >= m_DisableList.Count)
            return null;

        T obj = m_DisableList[_index];
        m_DisableList.RemoveAt(_index);
        m_EnableList.Add(obj);
        return obj;
    }


    public T GetDisableObject(int _index)
    {
        if (_index < 0 || _index >= m_DisableList.Count)
            return null;
        return m_DisableList[_index];
    }

    public T GetEnableObject(int _index)
    {
        if (_index < 0 || _index >= m_EnableList.Count)
            return null;
        return m_EnableList[_index];
    }

    public bool DisableObject(int _index)
    {
        if (_index < 0 || _index >= m_EnableList.Count)
            return false;

        var target = m_EnableList[_index];

        m_EnableList.RemoveAt(_index);
        m_DisableList.Add(target);
        return true;
    }


    public IEnumerator GetEnumerator()
    {
        return m_EnableList.GetEnumerator();
    }

    public T this[int _index]
    {
        get
        {
            if (_index < 0 || _index >= m_EnableList.Count)
                return null;

            return m_EnableList[_index];
        }
    }

}
