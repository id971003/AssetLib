using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 고유의(Unique) 정수 인덱스를 발급하는 풀 리스트입니다.
/// Pooled List와는 다르게 한번 발급된 인덱스는 고유하며 고유한 인덱스를 발급하는경우 이 풀을 사용하면 유용합니다.
/// [ ] 연산자를 통해 각 인덱스 요소 접근도 되지만 순회가 필요한 경우 foreach를 사용할 수 있습니다.
/// 단 foreach 순회시 순서는 삽입순서에 맞게 보장되지 않음에 주의하세요.
/// </summary>
public class GenericIndexDictPool<T> : IEnumerable<T> where T : class, new()
{
    private NormalObjectPool<T> m_Pool;
    private Dictionary<long, T> m_IndexDict;    //현실적으로 인덱스가 long범위를 넘어서진 않을것
    private long m_AutoIncrement;

    public int Capacity
    {
        get;
        private set;
    }

    public int Count
    {
        get { return m_IndexDict.Count; }
    }

    public GenericIndexDictPool()
    {
        m_Pool = new NormalObjectPool<T>();
        m_IndexDict = new Dictionary<long, T>();
        m_AutoIncrement = 0;
    }

    public void SetCustomInstantiateFunc(Func<int, T[]> _func)
    {
        m_Pool.SetCustomInstantiateFunc(_func);
    }

    public long Create(out T _data)
    {
        T instance = m_Pool.Create();
        if (instance == null)
        {
            _data = null;
            return -1;
        }

        m_IndexDict.Add(m_AutoIncrement, instance);
        _data = instance;
        return m_AutoIncrement++;
    }

    public T GetObject(int _index)
    {
        T result;
        if (m_IndexDict.TryGetValue(_index, out result) == false)
            return null;
        return result;
    }

    public bool Release(long _index)
    {
        T result;
        if (m_IndexDict.TryGetValue(_index, out result) == false)
            return false;

        m_IndexDict.Remove(_index);
        return m_Pool.Release(result);
    }

    public bool Destroy(long _index)
    {
        T result;
        if (m_IndexDict.TryGetValue(_index, out result) == false)
            return false;

        m_IndexDict.Remove(_index);
        m_Pool.Destroy(result);
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_IndexDict.Values.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return m_IndexDict.Values.GetEnumerator();
    }

    public T this[long _index]
    {
        get
        {
            T result;
            if (m_IndexDict.TryGetValue(_index, out result) == false)
                return null;
            return result;
        }
    }
}
