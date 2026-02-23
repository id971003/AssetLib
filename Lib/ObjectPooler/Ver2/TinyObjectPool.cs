using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TinyObjectPool 원형
/// </summary>
public abstract class TinyObjectPool
{
    /// <summary>
    /// 생성된 TinyObjectPool 전체 리스트
    /// </summary>
    /// <remarks>타이니 오브젝트 풀은 보통 static으로 사용될 것이므로 List.Remove는 쓰지 않을 것임</remarks>
    protected static List<TinyObjectPool> tinyObjectPools = new List<TinyObjectPool>();

    /// <summary>
    /// PooledList 전체 클리어(내용만 날림; List.Clear)
    /// </summary>
    public static void ClearAll()
    {
#if UNITY_EDITOR
        Debug.Log($"{nameof(TinyObjectPool)}.{nameof(ClearAll)}"
            + $" tinyObjectPools.Count: {tinyObjectPools.Count}");
#endif
        for (int i = 0; i < tinyObjectPools.Count; i++)
        {
            tinyObjectPools[i].Clear();
        }
    }

    #region 디버그용
    /// <summary>
    /// Pool의 Stack내에 보관중인 재활용 오브젝트 수
    /// </summary>
    public abstract int PoolCount { get; }

    /// <summary>
    /// Object를 New한 횟수를 기록 (디버그용) Clear할 때 초기화.
    /// </summary>
    public abstract int NewCount { get; }

    public static IReadOnlyList<TinyObjectPool> TinyObjectPools
        => tinyObjectPools.AsReadOnly();
    #endregion

    /// <summary>
    /// TinyObjectPool 대기 목록 청소
    /// </summary>
    public abstract void Clear();
}

/// <summary>
/// 간단한 오브젝트 풀 관리자
/// <para>매 프레임 new 하는 등 자주 메모리를 잡아야 할 경우에 사용한다.</para>
/// </summary>
/// <remarks>멀티스레드 대응 안 함</remarks>
public class TinyObjectPool<T> : TinyObjectPool where T : class, new()
{
    /// <summary>
    /// 사용 가능한 오브젝트 인스턴스 목록
    /// </summary>
    private Stack<T> pool = new Stack<T>();

    /// <summary>
    /// GetOrCreate에서 New로 생성한 오브젝트의 수 (Clear시 리셋)
    /// </summary>
    private int newCount;

    /// <summary>
    /// pool (stack)에 들어있는 오브젝트의 수에 대한 외부 참조용
    /// </summary>
    public override int PoolCount => pool.Count;

    /// <summary>
    /// newCount의 외부 참조용
    /// </summary>
    public override int NewCount => newCount;

    public TinyObjectPool()
    {
        // 생성될 때 전체 리스트에 추가
        tinyObjectPools.Add(this);
        newCount = 0;
    }

    /// <summary>
    /// 풀에서 오브젝트 인스턴스 얻기(없으면 하나 생성)
    /// </summary>
    public T GetOrCreate()
    {
        if (pool.Count > 0)
        {
            return pool.Pop();
        }
        else
        {
            ++newCount;
            return new T();
        }
    }

    /// <summary>
    /// 사용이 끝난 오브젝트 인스턴스 반납
    /// </summary>
    /// <param name="obj">반납할 오브젝트 인스턴스</param>
    public void Return(T obj)
    {
#if UNITY_EDITOR
        if (pool.Contains(obj))
        {
            // 풀에 같은 오브젝트가 두 번 들어가면 안 됨.
            UnityEngine.Debug.LogError(
                $"Pushing an object {obj} already exists in TinyObjectPool<{typeof(T)}>. It might cause a serious problem!");

            return;
        }
#endif
        pool.Push(obj);
    }

    /// <summary>
    /// 현재 풀에 남아있는 객체 모두 제거(스테이지 끝날 때 등 호출)
    /// </summary>
    /// <remarks>사용 중인 오브젝트는 해제되지 않음</remarks>
    public override void Clear()
    {
        newCount = 0;
        pool.Clear();
    }
}


