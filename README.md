# AssetLib  
내가 자주쓰는 에셋이나 내가만든 라이브러리 설명해놓을꺼임  


* Asset  
  * Dotween  
  * Odin  
  * DgDataBase[아직안함]  



* Lib  
  * CameraResolution_Canvas
  * Singelton  
  * SoundManager
  * ScenesManager  
  * ObjectPooler   
  * DatasManager   
  * GpgsStorageHelper
  * GoogleSheetManager  
  * Utility   
  
  
***
## CameraResolution_Canvas  
조건 : 켄버스에 집에넣어야함  
세팅 : setwidth[가로],setheight[세로],letterboxcolor[레터박스색]  
Canvas 해상도 고정하는친구임  
* Scale With Screen Size 로 변경후 Reference Resolution 을 setWhith 랑 setHeight 로 맞춘다  
* 가로기준으로 해상도 맞추고 색변경함  
* screenMatchMode 를 Expand 로 변경함   
***
## Singelton
조건 : Odin, 상속  
세팅 : SINGLETONTYPE[Dontdestroy 할껀지 안할껀지]  
싱글톤이다  
```
public class test : SINGLETON<kk,SINGLETONE.SINGLETONEType.DoNotDontDestroy>  
```
* 끌고 다니고 [dontdestroy] 싶으면 SINGLETONEType을 dontdestroy로 하자  
* 상속받은친구에 Awake 에 base.Awake 넣어주자  
* 오딘안쓸꺼면 SerializedMonoBehaviour 대신 MonoBehaviour상속시키면됨  
***
## SoundManager
조건 : odin , singleton , Dotween  
세팅 : ReSourcePath_Bgm[bgm 파일들 경로], ReSourcePath_Ef[ef 파일들 경로], filePath[ENUM타입 저장할위치]  
sound 관리하는 manager 임  
resource사운드파일들 이름들을 ENUM 타입으로 만들어서 ENUM으로 타입으로 사운드 콜가능  
EF 들의 경우 소리_몇개 설정할지 해줘야함 즉 이름 마지막에 _"몇개설정할지" 해주면됨  
인스펙터에서 Init_EnumType , Init_Object  순으로 실행하고 쓰면됨
```
using SoundManager_Enum;
SoundManager.Instance.SoundPlay_Ef(soundManager_Enum.bgm1,1);
```
* INIT_ENUM : 
  * RESOURCE 불러와서 ENUM 타입만듬
* INIT_ENUM : 
  * ENUM 만들어진 친구들 기준으로 SOUND 집어넣음
* SoundPlay_Ef  
  * ef 재생
  * 볼륨,루핑 설정가능
* SoundPlay_Ef_Decrease  
  * ef 중첩하면 소리가 점점 작아지는 재생
  * 기소리,점점작아지는 정도 [ex : 0.9 는 10%씩작아짐] , 최소값 설정가능  
* SoundPlay_Bgm_Direct
  * 배경음 재생  
* SoundPlay_Bgm_Increaas
  * 배경음 점점 커지면서 재생
  * 시작볼륨, 끝볼륨, 경과시간 설정가능
* StopAllSource_Direct
  * 모든소리즉시정지
* StopBgm_Direct
  * 배경음정지
* StopEf_Direct
  * 해당이팩트즉시정지
* StopBgm_Decrease
  * 배경음 점점정지
  * 몇초동안 설정가능
  
  
***
## ScenesManager
조건 : SINGLETON, DoTween  
세팅 : 로딩패널세팅 , iscenesLisener 상속  
씬 이동하는 매니저임  
ISceneLisenter 는 3개타입으로 이벤트나눔
 * MoveSceneStart : 이동시작함 씬에 모든기능 정지 [씬매니저가 콜]
 * SceneSetUpEnd : 이동한씬에서 쎗업이 끝남 [넘어간 씬에서 콜]
 * Event_LodingFaddedEnd : 로딩창이 다치워짐 게임시작 [씬매니저가 콜]
로딩을 비동기로 넘기려고 만듬  
1.씬기능을 정지시키고  
2.로딩창을 등장시키고 다음씬 로딩이 끝나고 셋업이 끝나면   
3.로딩바를 치우고 게임을 시작하는구조  

```
    public class test : MonoBehaviour,ISceneLisenter //상속
    
    public void OnSceneEvent(EVENT_SCENE eventType, Component sender, float param = 0) //이벤트 콜받음
    {
        if (eventType.Equals(EVENT_SCENE.Event_LodingFaddedEnd))
        {
            Debug.Log("Start");
        }
    }
    scenesmanager.EventPost(EVENT_SCENE.SceneSetUpEnd,this); //이벤트 콜링
    
    scenesmanager.EventScene_AddListenerAll(this); // 이벤트 등록
```
    
* MoveScene_Loding
  * 로딩시작
  * 모든코루틴정지
  * MoveSceneStart 실행
  * faddedon 실행
* FaddedOn
  * 로딩패널 키고 이동시킨후
  * C_MoveScene_Loding 콜백
* C_MoveScene_Loding
  * 비동시 씬로딩시키고
  * LodingBarDelayTime 타임에 맞춰서 로딩바 풀로채우고
  * b_SceneSetUpEnd 가 true 일때 까지 대기
  * 넘어간씬에서 SceneSetUpEnd 실행되면 b_SceneSetUpEnd true 됨
  * FaddedOff 실행
* FaddedOff
  * 로딩패널 치우고
  * Event_LodingFaddedEnd 콜백
  * 로딩채널끔
EventScene_AddListenerAll : 모든 이벤트 등록  
씬매니저의 콜을 받거나 콜을 할 친구들에게는 ISceneLisenter 상속후 Awake에서 EventScene_AddListenerAll 해주면됨

***
# ObjectPooler
조건 : SingleTon , Odin
세팅 : filePath[enum 스크립트 저장될 경로] , PrefabPath[Prefab 받아올 경로 reousrce 안에]
오브젝트 풀러임
Init_EnumType : PrefabPath 에서 프리팹 받아와서 프리팹 이름으로 enum 타입만듬 이름마지막에 몇개 만들지  
 ex) object1_5 >object1
Init_Object : Init_EnumType 다음에실행 , object 받아와서 실 갯수로 집어넣음
Dictionary<ObjectPool, Queue<GameObject>> 구조로 큐 안에 오브젝트 집어넣고 setactive 가 꺼진 상태로 들어가 셋업된다

SpawnFormPool : 오브젝트 꺼내다 가져다씀 Init_EnumType 에서 만든 Enum(ObjectPool) 으로 콜해 키고 사용  
들어있는게 없으면 하나 만들어서 다시 집어넣음
 ```
  ObjectPooler.SpawnFormPool(ObjectPool.a,gameObject.transform.position);
 ```
 
 여러게 만들어놈 필요한거 쓰면됨
 ```   
public static GameObject SpawnFormPool(ObjectPool objectName, Vector3 position)
public static GameObject SpawnFormPool(ObjectPool objectName, Vector3 position,quaternion rotation)
public static T SpawnFormPool<T> (ObjectPool objectName, Vector3 position,quaternion rotation) where T : Component
public static T SpawnFormPool<T> (ObjectPool objectName,GameObject parent) where T : Component
```

ReTurnPool : 다시 풀안에 집어넣는다 (오브젝트 죽었을때 꺼질때 콜백 되게 OnDisable 에 넣고 사용) 셋업시 오브젝트 만들고 이걸로 다시 집어넣음
```
public static void ReturnToPool(GameObject obj)
 ```

ObjectPoolObject : 모든 풀링할 오브젝트 들이 상속받아쓸 추상 클래스 
```
 ///사용
   public class objectpooltest : ObjectPoolObject
   public override void SetUp()
 //구현
    public abstract void SetUp();
    protected virtual void OnDisable()
    {
        ObjectPooler.ReturnToPool(gameObject);
        StopAllCoroutines();
        CancelInvoke();
    }
 ```
 
 ***
# DatasManager
조건 : SingleTon
세팅 : 뭐없음 미리 캐싱해서만 쓰자
```
 DataList datalistref = DatasManager.Instance.Datalist;
```
1-7 일 기준으로 내가 데이터를 저장할때는 클래스하나를 json 화 시켜서 저장한다 그래서 저장되는 모든데이터를 가지고있는 클레스 하나를 Serializable 해서 가지고 다니면서 바꾸고 저장하고 할생각임


 

