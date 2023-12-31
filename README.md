# AssetLib  
내가 자주쓰는 에셋이나 내가만든 라이브러리 설명해놓을꺼임  
update 12.14 내가만든 뭔가를 쓰는데 에셋이나 다른 라이브러리 들과의 의존성을 고민해봤는데
어차피 같이쓸거 같아서 엄청 의존적이게 해놈 조건 잘보고 써야한다

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
  * GoogleSheetManager
  * GpgsStorageHelper  //나중에할꺼임
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
## Singelton [Update 2023-12-18]
조건 : Odin, 상속  
세팅 : SINGLETONTYPE[Dontdestroy 할껀지 안할껀지]  
싱글톤이다  
```
public class test : SINGLETON<kk,SINGLETONE.SINGLETONEType.DoNotDontDestroy>  
```
* 끌고 다니고 [dontdestroy] 싶으면 SINGLETONEType을 dontdestroy로 하자  
* 상속받은친구에 Awake 에 base.Awake 넣어주자  
* 오딘안쓸꺼면 SerializedMonoBehaviour 대신 MonoBehaviour상속시키면됨  
* 아래 두경우는 결과 값이 같다 외부에서 생성 후 함수 실행하면 Awake>Enable>함수>Start 순으로 실행한다.  
  [이유는모름 gpt는 Awake>Enable>Start>함수 순이라는데 틀림]  
  즉 어떤 세팅값이 필요한 함수를 실행시키고 싶으면 Awkae 에서 셋업하자 Start 말고
![1](https://github.com/id971003/AssetLib/assets/79170289/c499f21e-ceaf-48f0-8522-f9daf29f6c8b)
![2](https://github.com/id971003/AssetLib/assets/79170289/b6736657-115e-4fb7-88a9-b936afef8296)

***
## SoundManager   [Update 2023-12-14]
조건 : odin , singleton , Dotween  
세팅 : 
 ReSourcePath_Bgm[bgm 파일들 경로]
 ReSourcePath_Ef[ef 파일들 경로]
 filePath[ENUM타입 저장할위치]  
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
* INIT_Sound : 
  * ENUM 만들어진 친구들 기준으로 SOUND 집어넣음
* play
  * SoundPlay_Bgm_Direct : bgm 즉시재생
  * SoundPlay_Bgm_Increaas_DoTween : bgm 점점커지면서 재생
     
  * SoundPlay_Ef : ef 재생
  * SoundPlay_Ef_Decrease : ef 재생하는데 이미 동일한 타입 재생중이면 소리 작게 재생

* Stop
  * StopBgm_Direct : bgm 즉시 정지
  * StopBgm_Decrease_DoTween : bgm 점점 정지
    
  * StopEf_Direct : ef 해당타입 모두 정지
    
  * StopAllSource_Direct : 모든 소리 즉시 정지
  * StopAllSource_Decrease_DoTween : 모든소리 점점 작아지면서 정지
  
  
***
## ScenesManager [Update 2023-12-17]
조건 : SINGLETON
세팅 : Script Excution Order 에 Defailt Time 위로 SceneManager 올리자 [정확히는 SceneManager 의 이벤트 콜받는 애들보다만 위에있으면 됨]
       ISceneLisenter 상속받고 구현 해주고 Awake에 EventScene_AddListenerAll 추가해서 이벤트 등록해주면됨
       
Scene 전환을 관리하고 유니티 플로우차트[Start,Awake] 말고 씬전환 타임에 이벤트 관리하는 EventManager 역할도 함

ISceneLisenter 는 5개 타입으로 나눔

SceneMoveStart, //씬이동시작              Call : OtherObject  /  Listen : CurrentSceneObject , LodingPannel  
CanSceneMove, //로딩바 다내려와서 씐가림    Call : LodingPannel /  Listen : SceneManager  
SceneMoveSucces, //씬씬넘어감            Call : SceneManager /  Listen : NextSceneObejct  
NextSceneSetUpEnd, //씬 셋업 끝         Call : NextSceneObject  / Listen : LodingPannel   
SceneStart //씬이동 종료                 Call : LodingPannel / Listen : NextSceneObejct  
  
GetAsynWlsgod 프로퍼티 친구로 로딩 얼마나 됬는지 로딩 패널에서 관리후 제어 

MoveScene_Loding : 비동기 로딩 시작

MoveScene_Direct : 즉시 이동 

이벤트  
AddListener : 해당 오브젝트 해당 이벤트 등록  
EventScene_AddListenerAll : 해당 오브젝트 모든 이벤트에 등록  
EventPost : 이벤트 발동  
RemoveEvent : 이벤트 지우기  
EventReSet : 씬이동하는 매니저다보니 저번씬에 있던 친구들이 아직 있을 수 도 있어 저번씬에서 등록한 오브젝트들 삭제 시킴 [ C_MoveScene_Loding 에서 씬이동 전에 실행]



***
## ScenesManager_LodingBar Vers [Update 2023-12-17]
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
# ObjectPooler [Update 2023-12-21]  
조건 : SingleTon , Odin  
세팅 : filePath[enum 스크립트 저장될 경로] , PrefabPath[Prefab 받아올 경로 reousrce 안에]  
오브젝트 풀러임  
Init_EnumType : PrefabPath 에서 프리팹 받아와서 프리팹 이름으로 enum 타입만들고 Reflash함
 ex) object1_5 >object1 [5개만듬]  
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
# DatasManager [Update 2023-12-27]  
조건 : SingleTon
세팅 : 뭐없음 미리 캐싱해서만 쓰자

 DataList datalistref = DatasManager.Instance.Datalist;

1-7 일 기준으로 내가 데이터를 저장할때는 클래스하나를 json 화 시켜서 저장한다 그래서 저장되는 모든데이터를 가지고있는 클레스 하나를 Serializable 해서 가지고 다니면서 바꾸고 저장하고 할생각임  
Datalist 를 캐싱했을때 후 로드 진행하면 datalistref.Value 값이 제대로 안들어간다 다시한번 캐싱해줘야한다  
***
 # GoogleSheetManager [Update 2023-12-30]
 조건 : singleton  
 세팅  
    SheetURL[구글스프레드시트] : AppsScprie 의 배포 > 웹앱 >URL  
    SheetDataURL[데이터 받아올시트 "SheetDataURL" 이후부터 export 이전까지 세팅해놔야함] 스프래드 시트 링크 + 범위  
    [edit 전까지 + 불러올 시트 범위 export?format=tsv&range=A2:B]    

   
 구글 스프레드 시트와 연동하는 코드임    
 웹에 데이터 단순히 뿌리고 받아오고 하지말고 비슷하고 다루시 쉬운 googlesheet 통해서 뿌리고 받아오는거  
 라이브러리화 시키고싶은데 워낙 수정이 많을 것 같아서 이후 수정하기 용이한 정도만 구현 해놈   
 GoogleData 클래스에서 뭐 얻을지 설정하고 구글 스프레드시트 apps script 에서 수정해 이용하자   

##### Post : 실제 통신 일어나는 곳임 
```
  IEnumerator Post(WWWForm form,Action<bool,string> afterProcess)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(SheetURL, form))
        {
            yield return www.SendWebRequest();

            if (www.isDone)
            {
                print(www.downloadHandler.text);
                GD = JsonUtility.FromJson<GoogleData>(www.downloadHandler.text);
                if (GD.result=="T")
                {
                    afterProcess?.Invoke(true,"성공");    
                }
                else
                {
                    afterProcess?.Invoke(false,"실패");
                }
                
            }
            else
            {
                print("웹응답없음");
            }
        }
    }
 ```

##### Login : 로그인 

 ```
    public void Login(string NickName, Action<bool, string> afterProcess = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("order","login");
        form.AddField("nickname",NickName);

        StartCoroutine(Post(form,afterProcess));
    }
 ```
##### Register : 회원가입
 ```
    public void Register(string NickName,string data,Action<bool,string> afterProcess=null)
    {
        WWWForm form = new WWWForm();
        form.AddField("order","register");
        form.AddField("nickname",NickName);
        form.AddField("value",data);
        StartCoroutine(Post(form,afterProcess));
    }
 ```
##### Reregister : 정보재등록
 ```
    public void Reregister(string Nickname, string value, Action<bool, string> afterProcess = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("order","reregister");
        form.AddField("nickname",Nickname);
        form.AddField("value",value);

        StartCoroutine(Post(form,afterProcess));
    }
 ```
##### C_LoadData : 데이터 받아오는 코드 캐싱해서 사용하면됨  
 ```
     IEnumerator C_LoadData(Action<bool,string,string> afterPrcess)
    {
        UnityWebRequest request = new UnityWebRequest();
        using (request = UnityWebRequest.Get(SheetDataURL))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                afterPrcess?.Invoke(true,request.downloadHandler.text,"성공");
            }
            else
            {
                afterPrcess?.Invoke(false,null,"실패");
            }
        }
    }
 ```
 
 ***
 # Utility
 나머지 잡다한거 모아논거 static 클래스임
 
 * 정렬 : quick정렬이용해놈
 * Waitfor : WaitForSeconds 캐싱해놈 readonly 로해놨음 더추가해서 쓰면됨
 * ValueTounit 꼬리표 : 단위 설정할거임 ex) 1.5a 1.7a 등
   * Unit : 단위 적어주면됨
   * devicevAlue : 몇개단위로 끊을껀지 ex) 3이면 1000 > 1.0a 됨 
```
public static readonly string[] Unit =
{
    "a", 
    "b",
    "c",
    "d"
    };
private static readonly int devicevAlue = 3;
double kk = 1334;
Debug.Log(kk.DoubleToString()); > 1.33a
 ```
 * Time_SecendToTime : 초를 시간으로 
 ```
 int kk = 12345;
Debug.Log(kk.Time_SecendToTime()); > 3시간 25분 45초
 ```
  * Time_MinuteToTime : 분을 시간으로
 ```
int kk = 12345;
Debug.Log(kk.Time_MinuteToTime()); > 205시간 45분
 ```
 

