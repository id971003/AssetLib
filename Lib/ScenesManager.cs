/*
 *씬 전환 하는 메니저다
 
 조건
  SINGLETONE , DoTween 
  MoveScene_Delay 이용할꺼면
  ISceneLisenter 상속
  ISceneLisenter 상속 받은 친구들보다 ScenesManager 가 늦게 시작해야함
  

  MoveScene_Direct : 즉시 씬 넘겨버림
  MoveScene_Loding
  (A>B Scene 으로 넘어과는과정)
  1. Event_MoveSceneStart : 이동시작
  3. Event_SceneSetUpEnd : 우리 셋업 끝났음 로딩창치워두됨
  5. Event_LodingFaddedEnd :  로딩창 다치움 게임시작
  
  로딩창 설정해줘야함
  
  
  
 */

using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using DG.Tweening;
using Sirenix.Utilities;


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
    

    protected override void Init()
    {
        if (instance)
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        
        EventScene_AddListenerAll(this);
        Go_LodingPanel.gameObject.SetActive(false);
        
    }

    
    #region 로딩창이동

    [SerializeField] private RectTransform Go_LodingPanel;
    [SerializeField] private Image Image_Lodingbar;
    private readonly Vector2 Vector2_LodingBar_GoToPosition = new Vector2(0, 1080);
    private const float LodingPanelMoveTIme=2;
    private const float LodingBarDelayTime = 2;
    public void MoveScene_Loding(string sceneName)
    {
        StopAllCoroutines();
        EventPost(EVENT_SCENE.MoveSceneStart, this);
        FaddedOn(sceneName);
        
    }
    IEnumerator C_MoveScene_Loding(string sceneName)
    {
        AsyncOperation op =  SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false; // 바로 씬 넘어가지 않게금 설정
        float time = 0;
        while (!op.isDone) //로딩 완료 되지 않을경우 계속 반복
        {
            Image_Lodingbar.fillAmount = op.progress;
            yield return null;
            if (op.progress >= 0.9f) // 씬이동은끝남 
            {
                op.allowSceneActivation = true;
                Image_Lodingbar.fillAmount = 0.9f;
                break;
            }

        }
        yield return new WaitUntil(() => b_SceneSetUpEnd); //이동씬 쌧업 기다림
        Image_Lodingbar.DOFillAmount(1, LodingBarDelayTime);
        yield return new WaitForSeconds(LodingBarDelayTime);
        FaddedOff();
        
    }

    void FaddedOn(string name)
    {
        Go_LodingPanel.anchoredPosition = Vector2_LodingBar_GoToPosition;
        Go_LodingPanel.gameObject.SetActive(true);
        Image_Lodingbar.fillAmount = 0;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(Go_LodingPanel.DOMoveY(Vector2_LodingBar_GoToPosition.y/2, LodingPanelMoveTIme))
            .AppendCallback(() =>
            {
                StartCoroutine(C_MoveScene_Loding(name));
            });
    }
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


