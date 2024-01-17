/*
made 7rzr 2024-1-17
데이터 저장 매니저 정도임

Local , Android 정도로 나눠서 저장할거 + 이후 딴거 추가

StorageManager 는 싱글톤 나머지 는 Static 으로 접근해서 저장

기본로직
비동기로 넘기는게 많다보니 뭔가 진행중 뭔가 돌아가는거 막으려고 로직하나 만듬

isProsses : 이거 true 면 뭔가 진행중임

RETURNDATA_STATUS : 성공 or 실패 반환

RETURNDATA_MESSAGE : 실패 메시지 반환

RETURNDATA_DATA : 로드시 데이터 반환

위 4개 기반으로 뭔가 다른 플렛폼 저장할때 만들어서 추가해주면됨

Login : storage manager 에 로그인이 있어도 될지 모르겠지만  저장하려면 로그인해야 하니까 넣어놈
(나중에 분리할수도?)

SaveData : 데이터 저장 

LoadData : 데이터 로드
 */




using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;



public class StorageManager : SINGLETON<StorageManager, Ns_SINGLETONE.SINGLETONEType.DontDestroy>
{
        
    #region 프로세스

    private readonly WaitForSeconds wait01 = new WaitForSeconds(0.1f);
    private bool isProsses { get; set; }
    private bool RETURNDATA_STATUS { get; set; }

    private string RETURNDATA_MESSAGE { get; set; }

    private string RETURNDATA_DATA { get; set; }

    private bool Process(Action<bool, string> afterProcessing = null)
    {
        if (isProsses)
        {
            afterProcessing?.Invoke(false, "뭔가작동중임");
            return false;
        }

        isProsses = true;
        StartCoroutine(C_Process(afterProcessing));
        return true;
    }

    private bool Process(Action<bool, string, string> afterProcessing)
    {
        if (isProsses)
        {
            afterProcessing?.Invoke(false, null, "뭔가작동중임");
            return false;
        }

        isProsses = true;
        StartCoroutine(C_Process(afterProcessing));
        return true;
    }

    IEnumerator C_Process(Action<bool, string> afterProcessing = null)
    {
        while (isProsses)
        {
            yield return wait01;
        }

        afterProcessing?.Invoke(RETURNDATA_STATUS, RETURNDATA_MESSAGE);
        RETURNDATA_MESSAGE = null;
        RETURNDATA_DATA = null;
    }

    IEnumerator C_Process(Action<bool, string, string> afterProcessing)
    {
        while (isProsses)
        {
            yield return wait01;
        }

        afterProcessing.Invoke(RETURNDATA_STATUS, RETURNDATA_DATA, RETURNDATA_MESSAGE);
        RETURNDATA_MESSAGE = null;
        RETURNDATA_DATA = null;
    }

    #endregion

    #region Local 저장파일이름

    private readonly string LocalSaveFileName="FileName";
    #endregion

    public void Gpgs_Login(Action<bool, string> logined = null)
    {
        if (!Process(logined))
        {
            return;
        }
        
        GpgsStorageHelper.Menual_Login((status,message) =>
        {
            isProsses = false;
        });
    }
    
    public void SaveData(bool b_local,  string savedata, Action<bool, string> onSave)
    {
        if (b_local) //로컬 저장
        {
            if (!Process(onSave))
            {
                return;
            }

            LocalStorageHelper.SaveLocalStorage(LocalSaveFileName, savedata, (a,b) =>
            {
                RETURNDATA_STATUS = a;
                RETURNDATA_MESSAGE = b;
                isProsses = false;
            });
        }
        else //클라우드저장
        {
            if (!Process(onSave))
            {
                return;
            }

#if UNITY_ANDROID
            GpgsStorageHelper.SavedGame_Save(savedata, (a, b) =>
            {
                RETURNDATA_STATUS = a;
                RETURNDATA_MESSAGE = b;
                isProsses = false;
            });
#endif
#if UNITY_IOS

#endif
        }
    }


    public void LoadData(bool b_local, Action<bool, string, string> onLoad = null)
    {
        if (b_local) //로컬 저장
        {
            if (!Process(onLoad))
            {
                return;
            }

            LocalStorageHelper.LoadLocalStorage(LocalSaveFileName, (a,b,c) =>
            {
                RETURNDATA_STATUS = a;
                RETURNDATA_DATA = b;
                RETURNDATA_MESSAGE = c;
                isProsses = false;
            });
        }
        else //클라우드저장
        {
            if (!Process(onLoad))
            {
                return;
            }
#if UNITY_ANDROID
            GpgsStorageHelper.SavedGame_Load( (a,b,c) =>
            {
                RETURNDATA_STATUS = a;
                RETURNDATA_DATA = b;
                RETURNDATA_MESSAGE = c;
                isProsses = false;
            });
#endif
#if UNITY_IOS

#endif
        }
    }

    
    
    protected override void Awake()
    {
        base.Awake();
        
    }
}
