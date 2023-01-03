using UnityEngine;
using System;

//https://docs.unity3d.com/kr/530/Manual/PlatformDependentCompilation.html
//https://includecoding.tistory.com/105
//if(application.platform==Runtimeplatform.Android,iponeplayer) : ����̽��� �÷����� Ȯ���� ����
//#if UNITY_ANDROID,IOS : unity buildsetting ���� android  �� ios �� ��� ���� �����
//�÷����� ( Ÿ�� ����̽������� �۵��ϴ�) ���� if(application.paltform) �����ͻ󿡵� �÷������� ����������Ѵٸ� #if ���
public static class CloudStorageHelper
{
    //use gpgsStoragehelper
    public static void SaveData(string filename,string savedata,Action<bool,string> onsave)
    {
#if UNITY_ANDROID
        GpgsHelper.Instance.SaveGame( 
            savedata, 
            onsave);
#elif UNITY_IOS
    
#endif

    }
    public static void LoadData(string filename, Action<bool,string,string> onload)
    {
#if UNITY_ANDROID
        GpgsHelper.Instance.LoadGame(
            onload);

#elif UNITY_IOS
    
#endif  
    }
}
