/// </summary>
/// MADE 7rzr 2023-01-04
//싱글톤이다  
//조건 : Odin, 상속  
//세팅 : SINGLETONTYPE[Dontdestroy 할껀지 안할껀지]  
//ex) public class kk : SINGLETON<kk,SINGLETONE.SINGLETONEType.DoNotDontDestroy>  
//끌고 다니고 [dontdestroy] 싶으면 SINGLETONEType을 dontdestroy로 하자  
//상속받은친구에 Awake 에 base.Awake 넣어주자  
//오딘안쓸꺼면 SerializedMonoBehaviour 대신 MonoBehaviour상속시키면됨  
/// </summary>
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

public class SINGLETON<T,SINGLETONTYPE> : SerializedMonoBehaviour//MonoBehaviour  오딘안쓸꺼면 모노 쓰셈
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

    protected virtual void Awake()
    {
        Debug.Log(gameObject.name);
        if (_singletoneType.Get_SingletoneType() == SINGLETONE_TYPE.DONTDESTROY)
        {
            if (instance)
            {
                DestroyImmediate(gameObject);
                return;
            }

            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }

    }
    

}
