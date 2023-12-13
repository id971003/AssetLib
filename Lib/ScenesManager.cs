/*
 MADE 7rzr 2023-01-06
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
*/

  
  

using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public interface ISceneLisenter
{
    void OnSceneEvent(EVENT_SCENE eventType, Component sender, float param = 0);
    
}

public enum EVENT_SCENE
{
    MoveSceneStart, //이동시작
    SceneSetUpEnd, // 우리 셋업 끝났음 로딩창치워두됨
    Event_LodingFaddedEnd // 로딩창 다치움 게임시작
}

public class ScenesManager : SINGLETON<ScenesManager,SINGLETONE.SINGLETONEType.DontDestroy>, ISceneLisenter
{

    public void OnSceneEvent(EVENT_SCENE eventType, Component sender, float param = 0)
    {
        if (eventType.Equals(EVENT_SCENE.MoveSceneStart))
        {
            b_SceneSetUpEnd = false;
        }
        if (eventType.Equals(EVENT_SCENE.SceneSetUpEnd))
        {
            b_SceneSetUpEnd = true;
        }
        if (eventType.Equals(EVENT_SCENE.Event_LodingFaddedEnd))
        {
        }
        
    }
    [SerializeField] private Dictionary<EVENT_SCENE, List<ISceneLisenter>> Dic_Listeners = new Dictionary<EVENT_SCENE, List<ISceneLisenter>>();

    [SerializeField] private bool b_SceneSetUpEnd;

    protected override void Awake()
    {
        base.Awake();
        EventScene_AddListenerAll(this);
        Go_LodingPanel.gameObject.SetActive(false);

    }

    
    #region 로딩창이동



    
    /// <summary>
    /// 1. MoveScene_Loding
    ///     로딩시작
    ///     모든코루틴정지
    ///     EVENT_SCENE.MoveSceneStart 넘김
    ///     faddedon 실행
    /// 2. FaddedOn
    ///     로딩패널 키고 이동시킨후
    ///     C_MoveScene_Loding 콜백
    /// 3. C_MoveScene_Loding
    ///     비동시 씬로딩시키고
    ///     b_SceneSetUpEnd true 될때 까지 대기
    ///     이후 LodingBarDelayTime 타임에 맞춰서 로딩바 풀로채우고
    ///     FaddedOff 콜백
    /// 4. FaddedOff
    ///     로딩패널 치우고
    ///     Event_LodingFaddedEnd 콜백
    ///     로딩패널 끔
    /// </summary>
    [SerializeField] private RectTransform Go_LodingPanel; //로딩패널
    [SerializeField] private Image Image_Lodingbar; //로딩바
    private readonly Vector2 Vector2_LodingBar_GoToPosition = new Vector2(0, 1080);//로딩바 초기위치
    private readonly float LodingPanelMoveTIme=2; //로딩바 움직이는시작
    private readonly float LodingBarDelayTime = 2; //로딩바 최대치 시 인위적으로 움직이는 시간
    /// <summary>
    /// 씬이동 - 로딩
    /// </summary>
    /// <param name="sceneName"></param> 씬이름
    public void MoveScene_Loding(string sceneName) 
    {
        StopAllCoroutines();
        EventPost(EVENT_SCENE.MoveSceneStart, this);
        FaddedOn(sceneName);
        
    }
    /// <summary>
    /// 로딩창을 어디서 어디론가로 이동시킴 
    /// </summary>
    /// <param name="name"></param> 씬이름
    void FaddedOn(string name)
    {
        Go_LodingPanel.anchoredPosition = Vector2_LodingBar_GoToPosition; //로딩씬 초기위치
        Go_LodingPanel.gameObject.SetActive(true); //킴
        Image_Lodingbar.fillAmount = 0; //로딩바 fill 값 끔
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(Go_LodingPanel.DOMoveY(Vector2_LodingBar_GoToPosition.y/2, LodingPanelMoveTIme))  //로딩씬 이동시키고
            .AppendCallback(() =>
            {
                StartCoroutine(C_MoveScene_Loding(name)); // 로딩시작
            });
    }
    /// <summary>
    /// //비동기 씬이동시작
    /// </summary>
    /// <param name="sceneName"></param> 씬이름
    /// <returns></returns>
    IEnumerator C_MoveScene_Loding(string sceneName)
    {
        AsyncOperation op =  SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false; // 바로 씬 넘어가지 않게금 설정
        while (!op.isDone) //로딩 완료 되지 않을경우 계속 반복 
        {
            Image_Lodingbar.fillAmount = op.progress; //lodingbar에 process전달
            yield return null;
            if (op.progress >= 0.9f) // 씬이동은끝남  
            {
                op.allowSceneActivation = true; //씬이동시키고
                Image_Lodingbar.fillAmount = 0.9f;  //lodingbar 0.9로고정
                break;
            }

        }
        yield return new WaitUntil(() => b_SceneSetUpEnd); //이동씬 쌧업 기다림 //이동씬에서 쎗업이끝나면
        Image_Lodingbar.DOFillAmount(1, LodingBarDelayTime); // 로딩바 끝까지채움
        yield return new WaitForSeconds(LodingBarDelayTime); //이후 다채워지면
        FaddedOff(); //faddedoff
        
    }
    

    /// <summary>
    /// 씬이동 끝나고 로딩씬 지우기
    /// 1. 로딩씬 을 옮김
    /// 2. Event_LodingFaddedEnd 호출
    /// 3. 로딩바지움
    /// </summary>
    void FaddedOff() 
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(Go_LodingPanel.DOMoveY(Vector2_LodingBar_GoToPosition.y*2, LodingPanelMoveTIme))
            .AppendCallback(() =>
            {
                EventPost(EVENT_SCENE.Event_LodingFaddedEnd, this);
                gameObject.SetActive(false);
            });
    }
    #endregion

 
    #region 바로이동

    /// <summary>
    /// 바로이동
    /// </summary>
    /// <param name="sceneName"></param> 씬이름
    public void MoveScene_Direct(string sceneName)//씬 바로이동
    {
        SceneManager.LoadScene(sceneName);
    }

    #endregion

    /// <summary>
    /// 이벤트 종류
    /// </summary>
    /// <param name="go"></param>
    #region  MoveSceneEvent
    public void EventScene_AddListenerAll(ISceneLisenter go)
    {
        foreach (EVENT_SCENE event_scene in Enum.GetValues(typeof(EVENT_SCENE)))
        {
            AddListener(event_scene, go);
        }
    }
    public void AddListener(EVENT_SCENE eventType, ISceneLisenter lisTener)
    {
        List<ISceneLisenter> listenList = null;
        if (Dic_Listeners.TryGetValue(eventType, out listenList))
        {
            listenList.Add(lisTener);
            return;
        }

        listenList = new List<ISceneLisenter>();
        listenList.Add(lisTener);
        Dic_Listeners.Add(eventType,listenList);
    }

    public void EventPost(EVENT_SCENE eventType, Component sender, float param = 0)
    {
        List<ISceneLisenter> listenList = null;
        if (!Dic_Listeners.TryGetValue(eventType, out listenList)) //등록된 이벤트가 없다면 지워버림
            return;
        foreach (ISceneLisenter listener in listenList)
        {
            if (!listener.Equals(null))
            {
                listener.OnSceneEvent(eventType,sender,param);
            }
        }
    }

    public void RemoveEvent(EVENT_SCENE eventType)
    {
        Dic_Listeners.Remove(eventType);
    }
    
    #endregion
}


