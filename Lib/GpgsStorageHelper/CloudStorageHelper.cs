using UnityEngine;
using System;

//https://docs.unity3d.com/kr/530/Manual/PlatformDependentCompilation.html
//https://includecoding.tistory.com/105
//if(application.platform==Runtimeplatform.Android,iponeplayer) : 디바이스의 플랫폼을 확인후 실행
//#if UNITY_ANDROID,IOS : unity buildsetting 값이 android  나 ios 일 경우 에도 실행됨
//플러그인 ( 타겟 디바이스에서만 작동하는) 경우는 if(application.paltform) 에디터상에도 플랫폼마다 대응해줘야한다면 #if 사용
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
