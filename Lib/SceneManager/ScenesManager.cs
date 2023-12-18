/*
 MADE 7rzr 2023-01-06
 update 2023-12-14 로딩창 의존성 제거

public class test : MonoBehaviour,ISceneLisenter //상속
    
public void OnSceneEvent(EVENT_SCENE eventType, Component sender, float param = 0) //이벤트 콜받음
{
    if (eventType.Equals(EVENT_SCENE.Event_LodingFaddedEnd))
    {
        Debug.Log("Start");
    }
}

조건 : SINGLETON  
세팅 : 로딩패널세팅 , iscenesLisener 상속  
씬 이동하는 매니저임
유니티 기본 플로우차트 [Start, Awake..] 에 관련없이 비동기 씬 이동시 자체 로직 만들었음
EVENT_SCENE 참고


EventPost : 이벤트 콜링
    
EventScene_AddListenerAll : 이벤트 등록

AddListener : 리스너 등록
    
RemoveEvent : 이벤트 삭제

EventReSet 리스너 들중 씬 이동시 유지할친구들[DontDestory] 은 다시 등록하고 이전씬에 있던 애들은 다지움

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
    SceneMoveStart, //씬이동시작              Call : OtherObject  /  Listen : CurrentSceneObject , LodingPannel
    CanSceneMove, //로딩바 다내려와서 씐가림    Call : LodingPannel /  Listen : SceneManager, 
    SceneMoveSucces, //씬씬넘어감            Call : SceneManager /  Listen : NextSceneObejct
    NextSceneSetUpEnd, //씬 셋업 끝         Call : NextSceneObject  / Listen : LodingPannel 
    SceneStart //씬이동 종료                 Call : LodingPannel / Listen : NextSceneObejct
}

public class ScenesManager : SINGLETON<ScenesManager,Ns_SINGLETONE.SINGLETONEType.DontDestroy>, ISceneLisenter
{

    public void OnSceneEvent(EVENT_SCENE eventType, Component sender, float param = 0)
    {
        if (eventType.Equals(EVENT_SCENE.SceneMoveStart))
        {
        }
        if (eventType.Equals(EVENT_SCENE.CanSceneMove))
        {
            b_LodingSetEnd = true;
        }
        if (eventType.Equals(EVENT_SCENE.SceneMoveSucces))
        {
   
        }
        if (eventType.Equals(EVENT_SCENE.NextSceneSetUpEnd))
        {
        }
        if (eventType.Equals(EVENT_SCENE.SceneStart))
        {
        }
        

    }
    [SerializeField] private Dictionary<EVENT_SCENE, List<ISceneLisenter>> Dic_Listeners = new Dictionary<EVENT_SCENE, List<ISceneLisenter>>();

    [SerializeField] private Dictionary<EVENT_SCENE, List<bool>>Dic_Listeners_Canremove = new Dictionary<EVENT_SCENE, List<bool>>();
    private bool b_LodingSetEnd; //로딩창 과 같은 기능이 있을때 그 로딩창 나오기전에 씬 넘어가면  안되서 막는 친구
    [SerializeField] private bool b_TestStartMiddleScene; //정상적으로 넘어와서 시작하는거말고 테스트시 중간씬 시작하는친구

    private AsyncOperation MoveSceneOp;

    public float GetAsynWlsgod => MoveSceneOp.progress;

    protected override void Awake()
    {
        base.Awake();
        EventScene_AddListenerAll(this);

    }

    void Start()
    {
        if (b_TestStartMiddleScene)
        {
            EventPost(EVENT_SCENE.SceneMoveSucces, this);
        }
    }
    
    
    #region 로딩창이동
    /// <summary>
    /// 씬이동 - 로딩
    /// </summary>
    /// <param name="sceneName"></param> 씬이름
    public void MoveScene_Loding(string sceneName) 
    {
        EventPost(EVENT_SCENE.SceneMoveStart, this);
        StartCoroutine(C_MoveScene_Loding(sceneName));
    }
    /// <summary>
    /// //비동기 씬이동시작
    /// </summary>
    /// <param name="sceneName"></param> 씬이름
    /// <returns></returns>
    IEnumerator C_MoveScene_Loding(string sceneName)
    {
        b_LodingSetEnd = false;
        MoveSceneOp =  SceneManager.LoadSceneAsync(sceneName);
        MoveSceneOp.allowSceneActivation = false; // 바로 씬 넘어가지 않게금 설정
        yield return new WaitUntil(() => b_LodingSetEnd); //로딩화면 나올때까지 대기 
        while (!MoveSceneOp.isDone)  
        {
            yield return null;
            if (MoveSceneOp.progress >= 0.9f) // 씬이동은끝남
            {
                EventReSet();
                MoveSceneOp.allowSceneActivation = true; //씬이동시키고
                break;
            }
        }

        yield return new WaitUntil(() => MoveSceneOp.isDone);
        EventPost(EVENT_SCENE.SceneMoveSucces, this);
    }

    #endregion

 
    #region 바로이동

    /// <summary>
    /// 바로이동
    /// </summary>
    /// <param name="sceneName"></param> 씬이름
    public void MoveScene_Direct(string sceneName)//씬 바로이동
    {
        EventReSet();
        SceneManager.LoadScene(sceneName);
    }

    #endregion

    #region  MoveSceneEvent
    /// <summary>
    /// 오든 이벤트에 해당 오브젝트 등록하기
    /// </summary>
    /// <param name="go"></param> 리스너
    /// <param name="CanRemove"></param> 지워지는 친구인지
    public void EventScene_AddListenerAll(ISceneLisenter go,bool CanRemove=true)
    {
        foreach (EVENT_SCENE event_scene in Enum.GetValues(typeof(EVENT_SCENE)))
        {
            AddListener(event_scene, go,CanRemove);
        }
    }
     /// <summary>
     /// 오브젝트 이벤트 등록하기
     /// </summary>
     /// <param name="eventType"></param> 이벤트 종류
     /// <param name="lisTener"></param> 리스너
     /// <param name="CanRemove"></param> 지워지는친구인가?
    public void AddListener(EVENT_SCENE eventType, ISceneLisenter lisTener,bool CanRemove=true)
    {
        List<ISceneLisenter> listenList = null;
        
        if (Dic_Listeners.TryGetValue(eventType, out listenList))
        {
            listenList.Add(lisTener);
            Dic_Listeners_Canremove[eventType].Add(CanRemove);
            return;
        }
        
        listenList = new List<ISceneLisenter>();
        listenList.Add(lisTener);

        List<bool> ListenCanRemoveList = new List<bool>();
        ListenCanRemoveList.Add(CanRemove);
        Dic_Listeners.Add(eventType,listenList);
        Dic_Listeners_Canremove.Add(eventType,ListenCanRemoveList);
    }

     /// <summary>
     /// 이벤트 발동
     /// </summary>
     /// <param name="eventType"></param> 이벤트종류
     /// <param name="sender"></param> 보내는친구
     /// <param name="param"></param> 넘길 값
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
     /// <summary>
     /// 이벤트 자체를 제거 
     /// </summary>
     /// <param name="eventType"></param>

    public void RemoveEvent(EVENT_SCENE eventType)
    {
        Dic_Listeners.Remove(eventType);
    }

     /// <summary>
     /// 씬이동할때 등록된 오브젝트를 중에 씬 못넘어오는친구들[DontDestory 가 아닌친구] 들 삭제하고 넘어온친구들은 유지시킴
     /// </summary>
    public void EventReSet()
    {
        for (int i = 0; i < Dic_Listeners.Count; i++)
        {
            for (int j = 0; j < Dic_Listeners[(EVENT_SCENE)i].Count; j++)
            {
                if (!Dic_Listeners_Canremove[(EVENT_SCENE)i][j])
                {
                    Dic_Listeners_Canremove[(EVENT_SCENE)i].RemoveAt(j);
                    Dic_Listeners[(EVENT_SCENE)i].RemoveAt(j);
                    j--;
                }
            }
        }
    }
    
    #endregion
}


