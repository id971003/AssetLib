using UnityEngine;
using System.IO;
using System;

public class LocalStorageHelper : SINGLETON<LocalStorageHelper, Ns_SINGLETONE.SINGLETONEType.DontDestroy>
{
    private string Message
    {
        get;
        set;
        
    }
    public void SaveLocalStorage(string filename,string jsondata,Action<bool,string> OnSave=null) //특정 string 값을 저장 
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

    public void LoadLocalStorage(string filename, Action<bool,string,string> OnLoad = null) // 저장된 값(Json)을 불러옴
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
