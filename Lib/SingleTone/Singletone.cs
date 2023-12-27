/*
 
 MADE 7rzr 2023-01-04
 Update 2023-12-20
싱글톤이다  
조건 : Odin, 상속  
세팅 : SINGLETONTYPE[Dontdestroy 할껀지 안할껀지]  
ex) public class kk : SINGLETON<kk,SINGLETONE.SINGLETONEType.DoNotDontDestroy>  
끌고 다니고 [dontdestroy] 싶으면 SINGLETONEType을 dontdestroy로 하자  
상속받은친구에 Awake 에 base.Awake 넣어주자  
오딘안쓸꺼면 SerializedMonoBehaviour 대신 MonoBehaviour상속시키면됨

+
유니티는 중간 씬에서 부터 에디터상에서 실행 후 테스트 해야한다.
아무리 시작하는 Scene 에서 모든 메니저[싱글톤] 친구들이 생성된다하더라도 말이다.
즉 중간 씬 부터 실행하면 아직 생성되지 않은 매니저 친구들이 생성된다  
이떄 발동 순서는
1.생성된매니저의 Awake
2.생성된매니저의 Enable
3.생성된매니저의 함수
4.생선된매니저의 Start
즉 생성 시 Awake>Start 바로 되는게아니라 Start에서 뭔가 셋업이일어나고  생성과 동시에 어떤 함수를 실행하면 오류난다
[왜? : Start 에서 일어날 셋업보다 함수의 실행이 먼저 일어 나기떄문이지] 
즉 셋업은 Awake 에서 앵간하면 다하자

gpt 한태물어보니 특정함수보다[3번] 이악물고 Start[4번] 가 먼저라는데 왜 내경우는 start가 뒤일까?
결과는 바뀌는거없다 애매하면 Awake 에서 다하자

+
싱글톤 타입인 친구들중에  dontdestroy로 인 애들은 scripte  순서를 defalt 타임 위로 올려주자
[없는거 생성할떈 잘 되는데 이미 있는거 dontdestroy 로 넘어갈때 재생성 되기떄문이지]

*/


/// </summary>
using Ns_SINGLETONE;
using Sirenix.OdinInspector;
using UnityEngine;




namespace Ns_SINGLETONE
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
    where  SINGLETONTYPE : Ns_SINGLETONE.ISingletone_type
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
        if (null == instance)
        {
            instance = this as T;
            if (_singletoneType.Get_SingletoneType() == SINGLETONE_TYPE.DONTDESTROY)
            {
                DontDestroyOnLoad(gameObject);
            }

        }
        else
        {
            DestroyImmediate(gameObject);
        }

    }
    

}
