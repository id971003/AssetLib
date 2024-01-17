/*
made 7rzr 2024-1-14
로컬 저장 하는거임
StorageManager 포멧 따라감

저장 로드 filename 은  storageManager 에서 설정해줌

SaveLocalStorage : 데이터 저장 , 이후 콜벡  ( 성공 실패 ? , 실패 메시지)

LoadLocalStorage : 데이터 로드  이후콜백 ( 성공 실패 ? , 데이터 , 실패 메시지)
*/

using UnityEngine;
using System.IO;
using System;

public static class LocalStorageHelper
{
    private static string Message
    {
        get;
        set;
            
    }
    public static void SaveLocalStorage(string filename,string jsondata,Action<bool,string> OnSave=null) //특정 string 값을 저장 
    {   
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (path == "")
        {
            Debug.LogError("pathSetFail");
            Message = "파일 위치 설정 실패";
            OnSave.Invoke(false, Message);
            return;
        }
        Message = "성공";
        File.WriteAllText(path, jsondata);
        OnSave.Invoke(true, Message);

    }

    public static void LoadLocalStorage(string filename, Action<bool,string,string> OnLoad = null) // 저장된 값(Json)을 불러옴
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (path == "")
        {
            Debug.LogError("pathSetFail");
            Message = "파일 위치 설정 실패";
            OnLoad?.Invoke(false,null, Message);
            return;
        }

        string jsondata = File.ReadAllText(path);
        if (jsondata == "")
        {
            Message = "파일 읽기 실패";
            Debug.LogError("FileReadFail");
            OnLoad.Invoke(false, null, Message);
            return;
        }
        Message = "성공";
        OnLoad?.Invoke(true, jsondata, Message);
    }
    
}
