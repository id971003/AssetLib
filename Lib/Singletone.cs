/*
 * 싱글톤 이다
 * 
 * ex public class kk : SINGLETON<kk,SINGLETONE.SINGLETONEType.DoNotDontDestroy>
 *
 * 끌고 다니고 [dontdestroy] 싶으면 SINGLETONEType을 dontdestroy로 하자 
 * 한개만 존재하고싶으면 Init 에 해당 주석처리된 dontdestroy 를 적용한다
 * Dontdestroy 로 할시 awake 대신 init 으로 초기화하자
 * 
 */

using SINGLETONE;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SINGLETONE
{
    public enum SINGLETONE_TYPE
    {
        DONTDESTROY,
        DONOT_DONTDESTROY
    }

    public interface ISingletone_type
    {
        SINGLETONE_TYPE Get_SingletoneType();
    }

    namespace SINGLETONEType
    {
        public struct DontDestroy : ISingletone_type
        {
            public SINGLETONE_TYPE Get_SingletoneType()
            {
                return SINGLETONE_TYPE.DONTDESTROY;
            }
        }
        public struct DoNotDontDestroy : ISingletone_type
        {
            public SINGLETONE_TYPE Get_SingletoneType()
            {
                return SINGLETONE_TYPE.DONOT_DONTDESTROY;
            }
        }
    }
}

public abstract class SINGLETON<T,SINGLETONTYPE> : SerializedMonoBehaviour//MonoBehaviour  오딘안쓸꺼면 모노 쓰셈
    where T : MonoBehaviour
    where  SINGLETONTYPE : SINGLETONE.ISingletone_type
{
    private static SINGLETONTYPE _singletoneType;
    protected static T instance = null;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
            }

            return instance;
        }


    }

    private void Awake()
    {
        if (_singletoneType.Get_SingletoneType() == SINGLETONE_TYPE.DONTDESTROY)
        {
            Init();
        }
    }

    protected abstract void Init();
    /*
    protected override void Init()
    {
        if (instance)
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
     */

}
