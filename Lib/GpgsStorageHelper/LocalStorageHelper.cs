using UnityEngine;
using System.IO;
using System;
//https://blog.naver.com/PostView.naver?blogId=bysmk14&logNo=221601332079&categoryNo=151&parentCategoryNo=0&viewDate=&currentPage=1&postListTopCurrentPage=1&from=search
//https://3dmpengines.tistory.com/1745
//Application.datapath 는 런타임중 파일 수정 작성 불가능한 읽기정용이다 / 저장위치 : 프로젝트 폴더 내부 (Asset 폴더)
//Application.streamingAssetsPath 는 런타임중 작성 수정 불가능한 파일이다. / 저장위치 : 프로젝트 폴더 내부 (Asset 폴더)
//Application.perststentDataPath 는 읽기 쓰기가 가능하다   / 저장위치 : Window- userproflie/appdata 안 localState  ios,android - 장치의 공통 디렉터리

public static class LocalStorageHelper
{
    public static string Message
    {
        get;
        private set;
    }
    #region SAVE
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
    #endregion


    #region LOAD
    public static void LoadLocalStorage(string filename, Action<bool,string,string> onload = null) // 저장된 값(Json)을 불러옴
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (path == "")
        {
            Debug.LogError("pathSetFail");
            Message = "파일 위치 설정 실패";
            onload?.Invoke(false,null, Message);
            return;
        }

        string jsondata = File.ReadAllText(path);
        if (jsondata == "")
        {
            Message = "파일 읽기 실패";
            Debug.LogError("FileReadFail");
            onload.Invoke(false, null, Message);
            return;
        }
        Message = "성공";
        onload?.Invoke(true, jsondata, Message);

    }
    #endregion
}
