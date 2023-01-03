using UnityEngine;
using System.IO;
using System;
//https://blog.naver.com/PostView.naver?blogId=bysmk14&logNo=221601332079&categoryNo=151&parentCategoryNo=0&viewDate=&currentPage=1&postListTopCurrentPage=1&from=search
//https://3dmpengines.tistory.com/1745
//Application.datapath �� ��Ÿ���� ���� ���� �ۼ� �Ұ����� �б������̴� / ������ġ : ������Ʈ ���� ���� (Asset ����)
//Application.streamingAssetsPath �� ��Ÿ���� �ۼ� ���� �Ұ����� �����̴�. / ������ġ : ������Ʈ ���� ���� (Asset ����)
//Application.perststentDataPath �� �б� ���Ⱑ �����ϴ�   / ������ġ : Window- userproflie/appdata �� localState  ios,android - ��ġ�� ���� ���͸�

public static class LocalStorageHelper
{
    public static string Message
    {
        get;
        private set;
    }
    #region SAVE
    public static void SaveLocalStorage(string filename,string jsondata,Action<bool,string> OnSave=null) //Ư�� string ���� ���� 
    {   
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (path == "")
        {
            Debug.LogError("pathSetFail");
            Message = "���� ��ġ ���� ����";
            OnSave.Invoke(false, Message);
            return;
        }
        Message = "����";
        File.WriteAllText(path, jsondata);
        OnSave.Invoke(true, Message);

    }
    #endregion


    #region LOAD
    public static void LoadLocalStorage(string filename, Action<bool,string,string> onload = null) // ����� ��(Json)�� �ҷ���
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (path == "")
        {
            Debug.LogError("pathSetFail");
            Message = "���� ��ġ ���� ����";
            onload?.Invoke(false,null, Message);
            return;
        }

        string jsondata = File.ReadAllText(path);
        if (jsondata == "")
        {
            Message = "���� �б� ����";
            Debug.LogError("FileReadFail");
            onload.Invoke(false, null, Message);
            return;
        }
        Message = "����";
        onload?.Invoke(true, jsondata, Message);

    }
    #endregion
}
