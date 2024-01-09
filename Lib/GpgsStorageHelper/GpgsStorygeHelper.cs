


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif
public class GpgsStorygeHelper : SINGLETON<GpgsStorygeHelper,Ns_SINGLETONE.SINGLETONEType.DontDestroy>
{
#if UNITY_ANDROID
    public bool isProsses
    {
        get;
        set;
    }

    public bool IsAuthenticated
    {
        get
        {
            return PlayGamesPlatform.Instance.IsAuthenticated();
        }
    }

    private bool RETURNDATA_STATUS
    {
        get;
        set;
    }

    private string RETURNDATA_MESSAGE
    {
        get;
        set;
    }

    private string RETURNDATA_DATA
    {
        get;
        set;
    }

    ISavedGameClient SavedGame =>
        PlayGamesPlatform.Instance.SavedGame;
    private readonly WaitForSeconds wait01 = new WaitForSeconds(0.1f);

    #region 프로세스

    private bool Process(Action<bool, string> afterProcessing=null)
    {
        if (isProsses)
        {
            afterProcessing?.Invoke(false,"뭔가작동중임");
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
    IEnumerator C_Process(Action<bool,string> afterProcessing=null)
    {
        while(isProsses)
        {
            yield return wait01;
        }
        afterProcessing?.Invoke(RETURNDATA_STATUS, RETURNDATA_MESSAGE);
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
            if (status == SignInStatus.Success)
            {
                ProcessEnd_Succes("로그인 성공");
            }
            else
            {
                ProcessEnd_Fail("로그인 실패");
            }
        });
    }

    public void Menual_Login(Action<bool, string> logined) //로그인 안됬으면 강제 로그인
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

    #region Save
    public void SavedGame_Save(string saveData, Action<bool, string> saveed = null)
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.Log("로그인안됨");
            return;
        }

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
    void SaveGmae_Save(string filename, string saveData, Action<bool> onSaved = null)
    {
        SavedGame.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime, (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
                    SavedGameMetadataUpdate updatedMetadata = builder.Build();
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(saveData);
                    SavedGame.CommitUpdate(game, updatedMetadata, bytes, (status, game) =>
                    {
                        if (status == SavedGameRequestStatus.Success)
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
    #endregion

    #region Load
    public void SavedGame_Load(Action<bool, string, string> loaded)
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

        SavedGame_Load("gamedata", (status, data) =>
        {
            if (status)
            {
                ProcessEnd_Succes("로드 성공", data);
            }
            else
            {
                ProcessEnd_Fail("로드 실패");
            }
        });
    }
    void SavedGame_Load(string filename, Action<bool, string> onLoaded = null)
    {
        SavedGame.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime, (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    SavedGame.ReadBinaryData(game, (status2, loadedData) =>
                    {
                        if (status2 == SavedGameRequestStatus.Success)
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
    #endregion
#endif
}

