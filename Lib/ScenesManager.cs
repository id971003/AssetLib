/*
 *씬 전환 하는 메니저다
 
 조건
  SINGLETONE ,  
  MoveScene_Delay 이용할꺼면
  ISceneLisenter 상속
  ISceneLisenter 상속 받은 친구들보다 ScenesManager 가 늦게 시작해야함

  MoveScene_Direct : 즉시 씬 넘겨버림
  MoveScene_Delay
  (A>B Scene 으로 넘어과는과정)
  1. Event_MoveSceneStart : 이동시작
  2. Event_Processing : 얼마나 이동했는지 정보를 넘김
  3. Event_SceneSetUpEnd : 우리 셋업 끝났음 로딩창치워두됨
  4. Event_FaddedOffStart : 로딩창 치움 시작
  5. Event_FaddedOffEnd :  로딩창 다치움 게임시작
  
  
  
 */

using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;



public interface ISceneLisenter
{
    void OnSceneEvent(EVENT_SCENE evettpye, Component sender, float param = 0);
    
}

public enum EVENT_SCENE
{
    MoveSceneStart, //이동시작
    Processing, // 얼마나 이동했는지 정보를 넘김
    SceneSetUpEnd, // 우리 셋업 끝났음 로딩창치워두됨
    FaddedOffStart, //로딩창 치움 시작
    FaddedOffEnd // 로딩창 다치움 게임시작
}


public class ScenesManager : SINGLETON<ScenesManager,SINGLETONE.SINGLETONEType.DontDestroy>, ISceneLisenter
{
    public void OnSceneEvent(EVENT_SCENE eventType, Component sender, float param = 0)
    {
        if (eventType.Equals(EVENT_SCENE.MoveSceneStart))
        {
            b_SceneSetUpEnd = false;
        }
        if (eventType.Equals(EVENT_SCENE.Processing))
        {
        }
        if (eventType.Equals(EVENT_SCENE.SceneSetUpEnd))
        {
            b_SceneSetUpEnd = true;
        }
        if (eventType.Equals(EVENT_SCENE.FaddedOffStart))
        {
        }
        if (eventType.Equals(EVENT_SCENE.FaddedOffEnd))
        {
        }
        
    }
    private Dictionary<EVENT_SCENE, List<ISceneLisenter>> Dic_Listeners = new Dictionary<EVENT_SCENE, List<ISceneLisenter>>();

    private bool b_SceneSetUpEnd;


    
    

    public void MoveScene_Delay(string sceneName)
    {
        EventPost(EVENT_SCENE.MoveSceneStart, this);
        StopAllCoroutines();
        StartCoroutine((C_MoveScene_Delay(sceneName)));
    }

    IEnumerator C_MoveScene_Delay(string sceneName)
    {
        AsyncOperation op =  SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false; // 바로 씬 넘어가지 않게금 설정
        while (!op.isDone) //로딩 완료 되지 않을경우 계속 반복
        {
            yield return null;
            
            EventPost(EVENT_SCENE.Processing,this,op.progress);
            
            if (op.progress >= 0.9f && b_SceneSetUpEnd)
            {
                op.allowSceneActivation = true;
                break;
            }
        }
        EventPost(EVENT_SCENE.FaddedOffStart,this);
    }
    
    protected override void Init()
    {
        EventScene_AddListenerAll(this);
    }

 
    #region 바로이동

    public void MoveScene_Direct(string sceneName)//씬 바로이동
    {
        SceneManager.LoadScene(sceneName);
    }

    #endregion

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
