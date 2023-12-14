/*
 MADE 7rzr 2023-01-06
 update 2023-12-14 로딩창 의존성 제거

조건 : SINGLETON, DoTween  
세팅 : 로딩패널세팅 , iscenesLisener 상속  
씬 이동하는 매니저임  
    ISceneLisenter 는 3개타입으로 이벤트나눔
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

*/

  
  

using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using DG.Tweening;

public interface ISceneLisenter
{
    void OnSceneEvent(EVENT_SCENE eventType, Component sender, float param = 0);
    
}

public enum EVENT_SCENE
{
    SceneMovestart, //이동시작
    SceneLodingObjectSettingEnd, //로딩화면 이 화면 다가림 넘어가도됨
    SceneMoveEnd //이동끝 로딩화면치우셈
}

public class ScenesManager : SINGLETON<ScenesManager,SINGLETONE.SINGLETONEType.DontDestroy>, ISceneLisenter
{

    public void OnSceneEvent(EVENT_SCENE eventType, Component sender, float param = 0)
    {
        if (eventType.Equals(EVENT_SCENE.SceneMovestart))
        {
            Debug.Log("SceneMovestart");
            //자이제 시작한다
            b_LodingSetEnd = false;
        }
        if (eventType.Equals(EVENT_SCENE.SceneLodingObjectSettingEnd))
        {
            Debug.Log("SceneLodingObjectSettingEnd");
            //로딩화면 나올때 까지 대기
            b_LodingSetEnd = true;
        }
        if (eventType.Equals(EVENT_SCENE.SceneMoveEnd))
        {
            Debug.Log("SceneMoveEnd");
            //로딩화면 치워도됨
        }
        

    }
    [SerializeField] private Dictionary<EVENT_SCENE, List<ISceneLisenter>> Dic_Listeners = new Dictionary<EVENT_SCENE, List<ISceneLisenter>>();

    [SerializeField] private Dictionary<EVENT_SCENE, List<bool>>Dic_Listeners_Canremove = new Dictionary<EVENT_SCENE, List<bool>>();
    [SerializeField] private bool b_LodingSetEnd; //로딩창 과 같은 기능이 있을때 그 로딩창 나오기전에 씬 넘어가면  안되서 막는 친구
    
    

    private AsyncOperation MoveSceneOp;

    public float GetAsynWlsgod => MoveSceneOp.progress;

    protected override void Awake()
    {
        base.Awake();
        EventScene_AddListenerAll(this);
    }

    
    #region 로딩창이동
    /// <summary>
    /// 씬이동 - 로딩
    /// </summary>
    /// <param name="sceneName"></param> 씬이름
    public void MoveScene_Loding(string sceneName) 
    {
        EventPost(EVENT_SCENE.SceneMovestart, this);
        StartCoroutine(C_MoveScene_Loding(sceneName));
    }
    /// <summary>
    /// //비동기 씬이동시작
    /// </summary>
    /// <param name="sceneName"></param> 씬이름
    /// <returns></returns>
    IEnumerator C_MoveScene_Loding(string sceneName)
    {
        
        MoveSceneOp =  SceneManager.LoadSceneAsync(sceneName);
        MoveSceneOp.allowSceneActivation = false; // 바로 씬 넘어가지 않게금 설정
        yield return new WaitUntil(() => b_LodingSetEnd); //로딩화면 나올때까지 대기 
        
        while (!MoveSceneOp.isDone)  
        {
            yield return null;
            if (MoveSceneOp.progress >= 0.9f) // 씬이동은끝남
            {
                MoveSceneOp.allowSceneActivation = true; //씬이동시키고
                break;
            }
        }

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
    public void EventScene_AddListenerAll(ISceneLisenter go,bool CanRemove=true)
    {
        foreach (EVENT_SCENE event_scene in Enum.GetValues(typeof(EVENT_SCENE)))
        {
            AddListener(event_scene, go,CanRemove);
        }
    }
    public void AddListener(EVENT_SCENE eventType, ISceneLisenter lisTener,bool CanRemove)
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

    public void EventReSet()
    {
        for (int i = 0; i < Dic_Listeners.Count; i++)
        {
            for (int j = 0; j < Dic_Listeners[(EVENT_SCENE)i].Count; j++)
            {
                if (Dic_Listeners_Canremove[(EVENT_SCENE)i][j])
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


