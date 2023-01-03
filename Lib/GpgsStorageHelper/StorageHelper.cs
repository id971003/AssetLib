using UnityEngine;
using System.IO;
using System;
public class StorageHelper :MonoBehaviour
{
    private static StorageHelper instance = new StorageHelper();
    public static StorageHelper Instance => instance;
    public void SaveData(bool b_local,string filename,string savedata,Action<bool,string> onsave, Action ontrylogin=null, Action onproccesing=null)
    {
        //use localstoragehelper
        if (b_local)
        {
            LocalStorageHelper.SaveLocalStorage(filename,savedata, onsave);
        }
        //use cloudstoragehelper
        else
        {
            CloudStorageHelper.SaveData(filename, savedata, onsave);
        }
    }
    public void LoadData(bool b_local,string filename, Action<bool,string,string> onload = null, Action ontrylogin = null, Action onproccesing = null)
    {
        if (b_local)
        {
            LocalStorageHelper.LoadLocalStorage(filename,onload);
        }
        else
        {
            CloudStorageHelper.LoadData(filename, onload);
        }
    }
}
