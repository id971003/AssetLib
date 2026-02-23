using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// TValue타입 데이터를 풀링하는 딕셔너리입니다.
/// 
/// [ ] 연산자를 통해 각 인덱스 요소 접근도 되지만 순회가 필요한 경우 foreach를 사용할 수 있습니다.
/// </summary>
public class GenericDictPool<TKey, TValue>
    : IEnumerable<KeyValuePair<TKey, TValue>> where TValue : class, new()
{
    private NormalObjectPool<TValue> m_Pool;
    private Dictionary<TKey, TValue> m_Dict;

    public int Capacity
    {
        get;
        private set;
    }

    public int Count
    {
        get { return m_Dict.Count; }
    }


    public GenericDictPool(int recapacity = -1)
    {
        m_Pool = new NormalObjectPool<TValue>();
        m_Dict = new Dictionary<TKey, TValue>();

    }

    public void SetCustomInstantiateFunc(Func<int, TValue[]> _func)
    {
        m_Pool.SetCustomInstantiateFunc(_func);
    }

    public bool Create(TKey _key, out TValue _data)
    {
        if (m_Dict.ContainsKey(_key) == true)
        {
            _data = null;
            return false;
        }


        TValue instance = m_Pool.Create();
        if (instance == null)
        {
            _data = null;
            return false;
        }

        m_Dict.Add(_key, instance);
        _data = instance;
        return true;
    }

    public TValue GetObject(TKey _key)
    {
        TValue result;
        if (m_Dict.TryGetValue(_key, out result) == false)
            return null;
        return result;
    }

    public bool Release(TKey _key)
    {
        TValue result;
        if (m_Dict.TryGetValue(_key, out result) == false)
            return false;

        m_Dict.Remove(_key);
        return m_Pool.Release(result);
    }

    public bool Destroy(TKey _key)
    {
        TValue result;
        if (m_Dict.TryGetValue(_key, out result) == false)
            return false;

        m_Dict.Remove(_key);
        m_Pool.Destroy(result);
        return true;
    }



    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_Dict.GetEnumerator();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return m_Dict.GetEnumerator();
    }

    public TValue this[TKey _key]
    {
        get
        {
            TValue result;
            if (m_Dict.TryGetValue(_key, out result) == false)
                return null;
            return result;
        }
    }
}
