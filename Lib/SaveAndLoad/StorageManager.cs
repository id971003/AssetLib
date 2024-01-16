using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

public class StorageManager : SINGLETON<StorageManager, Ns_SINGLETONE.SINGLETONEType.DontDestroy>
{
    #region 참조

    
    

    #endregion
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
