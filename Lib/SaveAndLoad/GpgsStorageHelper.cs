/*
made 7rzr 2023-1-28
android 플랫폼 중 googlePlayGame 에 로그인 , 세이브 , 로드 구현해놓음 
StorageManager 포멧 따라감

저장 로드 filename 은  storageManager 에서 설정해줌

SaveLocalStorage : 데이터 저장 , 이후 콜벡  ( 성공 실패 ? , 실패 메시지)

LoadLocalStorage : 데이터 로드  이후콜백 ( 성공 실패 ? , 데이터 , 실패 메시지)
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif

public static class GpgsStorageHelper 
{
    static PlayGamesPlatform PlayGAmesPlatformref = PlayGamesPlatform.Instance;
    private static  bool IsAuthenticated
    {
        get
        {
            return PlayGAmesPlatformref.IsAuthenticated();
        }
    }
    
    static ISavedGameClient  SavedGame =>
        PlayGAmesPlatformref.SavedGame;

    public static void Auto_Login(Action<bool, string> logined=null) //시작 시 자동 로그인
    {
        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                logined(true,"로그인 성공");
            }
            else
            {
                logined?.Invoke(false,"Auto_Login : SignInStatus fail로그인 실패");
            }
        });
    }
    public static void Menual_Login(Action<bool, string> logined=null) //로그인 안됬으면 강제 로그인
    {
        if (IsAuthenticated)
        {
            logined(false,"이미 로그인 됨");
            return;
        }
        PlayGamesPlatform.Instance.ManuallyAuthenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                logined?.Invoke(true,"로그인 성공");
            }
            else
            {
                logined?.Invoke(false,"Menual_Login : SignInStatus fail로그인 실패");
            }
        });
    }
    public static void DevieceCheck_Login(Action<bool, string> logined) //로그인 안됬으면 강제 로그인 (미완성)
    {
        if (IsAuthenticated)
        {
            logined(false,"이미 로그인 됨");
            return;
        }
        PlayGamesPlatform.Instance.ManuallyAuthenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                logined?.Invoke(true,"로그인 성공");
            }
            else
            {
                logined?.Invoke(false,"DevieceCheck_Login : SignInStatus fail로그인 실패");
            }
        });
    }
    #region Save
    public static void SavedGame_Save(string saveData, Action<bool, string> saveed = null)
    {
        if (!IsAuthenticated)
        {
            Debug.Log("로그인안됨");
            return;
        }

        SaveGmae_Save("gamedata", saveData, (status,message) =>
        {
            if (status)
            {
                saveed?.Invoke(true, "저장 성공");
            }
            else
            {
                saveed?.Invoke(false, message);
            }
        });
    }
    static void SaveGmae_Save(string filename, string saveData, Action<bool, string> onSaved = null)
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
                            onSaved?.Invoke(true, null);
                        }
                        else
                        {
                            onSaved?.Invoke(false, "SaveGame_Save : fail");
                        }
                    });
                }
                else
                {
                    onSaved?.Invoke(false, "SaveGame_Save : fail");
                }
            });
    }
    #endregion

    #region Load
    public static void SavedGame_Load(Action<bool, string, string> loaded)
    {
        if (!IsAuthenticated)
        {
            Debug.Log("로그인안됨");
            return;
        }



        SavedGame_Load("gamedata", (status, data,message) =>
        {
            if (status)
            {
                loaded?.Invoke(true, data,message);
            }
            else
            {
                loaded?.Invoke(false, null,message);
            }
        });
    }
    static void SavedGame_Load(string filename, Action<bool, string,string> onLoaded = null)
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
                            onLoaded?.Invoke(true, data,null);
                        }
                        else
                        {
                            onLoaded?.Invoke(false, null,"SavedGame_Load : fail");
                        }
                    });
                }
                else
                {
                    onLoaded?.Invoke(false, null,"SavedGame_Load : fail");
                }
            });
    }
    #endregion
}
/* OldCode



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
                SavedGame_Load("deviceId", (a, b) =>
                {


                });
                ProcessEnd_Succes("자동 로그인 성공");
                
            }
            else
            {
                ProcessEnd_Fail("자동 로그인 실패");
            }
        });
    }

    public void DevieceCheck_Login(Action<bool, string> logined)
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

*/