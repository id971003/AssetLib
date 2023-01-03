#if UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;

/*GPGS 사용조건
 * 안드로이드SDK
 * 안드로이드V4.0이상
 * UNITY 2017.4 이상
 * GOOGLEPLAY 서비스 라이브러리 , 버전 11.6 이상
 */
//https://docs.unity3d.com/ScriptReference/Social.html
//https://docs.unity3d.com/ScriptReference/Social.Active.html
//https://docs.unity3d.com/Manual/net-SocialAPI.html
/* unity Social
 * 특정 소셜 플랫폼을 구현 대상으로 사용한다 , active 과정에서 따로설정하지않으면 특정 플랫폼 별 기본 값으로 선택됨 (ex: ios 의 경우 gamecenter)
 * 즉 특정 소셜 플랫폼과 연결시 유니티 Unityengine.socialPlatforms 인터페이스를 이용한다.
 * 보통 특정 소셜 플랫폼에 Social Api 클래스의 인터페이스를 사용함 , unity social 플랫폼 으론 기본기능밖에 제공안함 , 특정 소셜플랫폼과 연결시 해당 api 의 social 인터페이스를 사용 
 * unity 소셜 인터페이스는 다른 플랫폼의 인터페이스와 통합가능 ,  즉 표준소셜 인터페이스의 확장으로 제공됨
 * 표준 api 호출은 ISocailPlatform 인터페이스의 참조인 Social.Active 개체를 통해 액티브가능
 * Unity social , googleplaygame social
 * The Google Play Games plugin implements Unity's social interface, for compatibility with games that already use that interface when integrating with other platforms. However, 
 * some features are unique to Play Games and are offered as extensions to the standard social interface provided by Unity.
 * The standard API calls can be accessed through the Social.Active object, which is a reference to an ISocialPlatform interface. 
 * The non-standard Google Play Games extensions can be accessed by casting the Social.Active object to the PlayGamesPlatform class, where the additional methods are available.
 * Unity social 은 다른 플랫폼과 호환시 사용할 unity 에서 구현해논 기능이다.
 * gpgs 플러그인 에서는 unity social 인터페이스를 이미 구현해놓았다. 
 */


public class gpgsmanager : MonoBehaviour
{
    private static gpgsmanager instance;

    public static gpgsmanager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<gpgsmanager>();
                if (instance == null)
                {
                    instance = new GameObject("gpgsmanager").AddComponent<gpgsmanager>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance)
        {
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public bool isProsses
    {
        get;
        set;
    }
    public bool IsAuthenticated //로그인 되었는지
    {
        get
        {
            return PlayGamesPlatform.Instance.IsAuthenticated();
        }
    }

    private bool RETURNDATA_STATUS
    {
        get;set;
    }
    private string RETURNDATA_MESSAGE
    {
        get;set;
    }
    private string RETURNDATA_DATA
    {
        get;set;
    }



    ISavedGameClient SavedGame =>
    PlayGamesPlatform.Instance.SavedGame;

    private readonly WaitForSeconds wait01 = new WaitForSeconds(1);


#region 프로세스
    private bool Process(Action<bool,string> afterProcessing)
    {
        if(isProsses)
        {
            afterProcessing?.Invoke(false, "뭔가작동중임");
            return false;
        }
        isProsses = true;
        StartCoroutine(C_Process(afterProcessing));
        return true;
    }
    private bool Process(Action<bool,string, string> afterProcessing)
    {
        if (isProsses)
        {
            afterProcessing?.Invoke(false,null, "뭔가작동중임");
            return false;
        }
        isProsses = true;
        StartCoroutine(C_Process(afterProcessing));
        return true;

    }

     IEnumerator C_Process(Action<bool,string> afterProcessing)
    {
        while(isProsses)
        {
            yield return wait01;
        }
        afterProcessing.Invoke(RETURNDATA_STATUS, RETURNDATA_MESSAGE);
        RETURNDATA_MESSAGE = null;
        RETURNDATA_DATA = null;
    }
    IEnumerator C_Process(Action<bool, string,string> afterProcessing)
    {
        while (isProsses)
        {
            yield return wait01;
        }
        afterProcessing.Invoke(RETURNDATA_STATUS, RETURNDATA_DATA,RETURNDATA_MESSAGE);
        RETURNDATA_MESSAGE = null;
        RETURNDATA_DATA = null;
    }
    private void ProcessEnd_Succes(string message, string data)
    {
        RETURNDATA_STATUS = true;
        RETURNDATA_MESSAGE = message;
        RETURNDATA_DATA = data;
        isProsses = false;
    }
    private void ProcessEnd_Succes(string message)
    {
        RETURNDATA_STATUS = true;
        RETURNDATA_MESSAGE = message;
        RETURNDATA_DATA = null;
        isProsses = false;
    }

    private void ProcessEnd_Fail(string message)
    {
        RETURNDATA_STATUS = false;
        RETURNDATA_DATA = null;
        RETURNDATA_MESSAGE = message;
        isProsses = false;
    }
#endregion


#region 로그인
    public void Auto_Login(Action<bool, string> logined) //시작 시 자동 로그인
    {
        if (!Process(logined))
        {
            return;
        }
            PlayGamesPlatform.Instance.Authenticate((status) =>
            {
                if(status == SignInStatus.Success)
                {
                    ProcessEnd_Succes("로그인 성공");
                }
                else
                {
                    ProcessEnd_Fail("로그인 실패");
                }
            });
    }

    public void Menual_Login(Action<bool,string> logined) //로그인 안됬으면 강제 로그인
    {
        if (!Process(logined))
        {
            return;
        }
        if (IsAuthenticated)
        {
            ProcessEnd_Fail("이미 로그인 됨");
            return;
        }
        PlayGamesPlatform.Instance.ManuallyAuthenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                ProcessEnd_Succes("자동 로그인 성공");
            }
            else
            {
                ProcessEnd_Fail("자동 로그인 실패");
            }
        });
    }
#endregion





    public void Save_GameData(string saveData,Action<bool,string> saveed)
    {
        if (!Process(saveed))
        {
            return;
        }
        if (!IsAuthenticated)
        {
            ProcessEnd_Fail("로그인 안됨");
            return;
        }
        SaveGmae_Save("gamedata", saveData, (status) =>
         {
             if (status)
             {
                ProcessEnd_Succes("저장 성공");
             }
             else
             {
                 ProcessEnd_Fail("저장 실패");
             }
         });
    }
    public void SavedGame_Load(Action<bool , string, string> loaded)
    {
        if (!Process(loaded))
        {
            return;
        }
        if (!IsAuthenticated)
        {
            ProcessEnd_Fail("로그인 안됨");
            return;
        }

        SavedGame_Load("gamedata",  (status, data) =>
        {
            if (status)
            {
                ProcessEnd_Succes("저장 성공", data);
            }
            else
            {
                ProcessEnd_Fail("저장 실패");
            }
        });
    }



    void SaveGmae_Save(string filename,string saveData,Action<bool> onSaved=null)
    {
        SavedGame.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime, (status,game)=>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
                    SavedGameMetadataUpdate updatedMetadata = builder.Build();
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(saveData);
                    SavedGame.CommitUpdate(game, updatedMetadata, bytes, (status, game) =>
                       {
                           if(status == SavedGameRequestStatus.Success)
                           {
                               onSaved?.Invoke(true);
                           }
                           else
                           {
                               onSaved?.Invoke(false);
                           }
                       });
                }
                else
                {
                    onSaved?.Invoke(false);
                }
            });
    }

    void SavedGame_Load(string filename,Action<bool,string> onLoaded=null)
    {
        SavedGame.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime, (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    SavedGame.ReadBinaryData(game, (status2, loadedData) =>
                     {
                         if(status2==SavedGameRequestStatus.Success)
                         {
                             string data = System.Text.Encoding.UTF8.GetString(loadedData);
                             onLoaded?.Invoke(true, data);
                         }
                         else
                         {
                             onLoaded?.Invoke(false, null);
                         }
                     });
                }
                else
                {
                    onLoaded?.Invoke(false, null);
                }
            });
    }

}
#endif