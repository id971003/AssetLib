/*
MADE 7rzr 2023-01-07
Update 2023-12-30
GoogleSheetManager
조건 : singleton
세팅 
    SheetURL[구글스프레드시트] : AppsScprie 의 배포 > 웹앱 >URL
    SheetDataURL[데이터 받아올시트 "SheetDataURL" 이후부터 export 이전까지 세팅해놔야함] 스프래드 시트 링크 + 범위
    [edit 전까지 + 불러올 시트 범위 export?format=tsv&range=A2:B]

웹에 데이터 단순히 뿌리고 받아오고 하지말고 비슷하고 다루시 쉬운 googlesheet 통해서 뿌리고 받아오는거
랭킹 이용했음
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleData
{
    public string 
        order,  //어떤건가 ? (ex 로그인, 로딩, 회원가입 등등)
        result, //성공했음 ? (ex T성공, F실패)
        message, //상태 메시지 (ex성공했다 or 실패했다 왜 ?)
        value; // 결과
}

public class GoogleSheetManager : SINGLETON<GoogleSheetManager,Ns_SINGLETONE.SINGLETONEType.DontDestroy>
{
     public GoogleData GD;
    //시트 코드 들어간 url
    private const string SheetURL =
        "https://script.google.com/macros/s/AKfycbzYT-m6BG4MgPREgJ2hAAHoL5Z6hAFsqAAnA6ui_fiYAAUlK5f4FoQsc-uaDnyGfuea/exec";
    
    //시트 데이터 url //A2:Z
    private const string SheetDataURL = "https://docs.google.com/spreadsheets/d/1eWG5B-zXXaIEzmSm5ObE27at2CmlWr-N0NqHKQ72tRg/export?format=tsv&range=A2:B";
    //랭킹데이터
    private const string RadeDataURL =
        "https://docs.google.com/spreadsheets/d/1eWG5B-zXXaIEzmSm5ObE27at2CmlWr-N0NqHKQ72tRg/export?format=tsv&gid=679387445&range=A2:Z";

    IEnumerator Post(WWWForm form,Action<bool,string> afterProcess)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(SheetURL, form))
        {
            yield return www.SendWebRequest();

            if (www.isDone)
            {
                print(www.downloadHandler.text);
                GD = JsonUtility.FromJson<GoogleData>(www.downloadHandler.text);
                if (GD.result=="T")
                {
                    afterProcess?.Invoke(true,"성공");    
                }
                else
                {
                    afterProcess?.Invoke(false,"실패");
                }
                
            }
            else
            {
                print("웹응답없음");
            }
        }
    }

    /// <summary>
    /// 회원가입
    /// </summary>
    /// <param name="NickName"></param> 등록할 닉내임
    /// <param name="afterProcess"></param> 이후 작업
    public void Register(string NickName,string data,Action<bool,string> afterProcess=null)
    {
        WWWForm form = new WWWForm();
        form.AddField("order","register");
        form.AddField("nickname",NickName);
        form.AddField("value",data);
        StartCoroutine(Post(form,afterProcess));
    }
    /// <summary>
    /// 로그인
    /// </summary>
    /// <param name="NickName"></param> 로그인할 닉네임
    /// <param name="afterProcess"></param> 이후 작업
    public void Login(string NickName, Action<bool, string> afterProcess = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("order","login");
        form.AddField("nickname",NickName);

        StartCoroutine(Post(form,afterProcess));
    }

    /// <summary>
    /// 랭킹 업데이트
    /// </summary>
    /// <param name="Nickname"></param> 다시 등록할 닉네임 [위치잡기]
    /// <param name="value"></param> 등록할 값
    /// <param name="afterProcess"></param> 이후 작업
    public void Reregister(string Nickname, string value, Action<bool, string> afterProcess = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("order","reregister");
        form.AddField("nickname",Nickname);
        form.AddField("value",value);

        StartCoroutine(Post(form,afterProcess));
    }
    
    /// <summary>
    /// 데이터받아오기
    /// </summary>
    /// <param name="afterProcess"></param> 이후작업
    public void LoadData(Action<bool, string, string> afterProcess) 
    {
        StartCoroutine(C_LoadData(afterProcess,SheetDataURL));
    }
    
    IEnumerator C_LoadData(Action<bool,string,string> afterPrcess,string URL)
    {
        UnityWebRequest request = new UnityWebRequest();
        using (request = UnityWebRequest.Get(URL))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                afterPrcess?.Invoke(true,request.downloadHandler.text,"성공");
            }
            else
            {
                afterPrcess?.Invoke(false,null,"실패");
            }
        }
        request.Dispose();
    }
    

    
    

    protected override void Awake()
    {
        base.Awake();
    }
    
    

}