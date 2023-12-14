//  MADE 7rzr 2023-01-07
// # GoogleSheetManager
// 조건 : singleton
// 세팅 : SheetURL[구글스프레드시트] , SheetDataURL[데이터 받아올시트 "SheetDataURL" d이후부터 export 이전까지 세팅해놔야함]
// 구글 스프레드 시트와 연동하는 코드임  
//     유저들이 다른 유저 정보 볼 수있는 혹은 자기꺼 저장해서 남한테 보여주는기능[ex 랭킹] 등 간단한 정보를 웹에 뿌려 사용할때 쓸꺼임  
//     라이브러리화 시키고싶은데 워낙 수정이 많을 것 같아서 이후 수정하기 용이한 정도만 구현 해놈 
// GoogleData 클래스에서 뭐 얻을지 설정하고 구글 스프레드시트 apps script 에서 수정해 이용하자 
///sheet.getrange(2,2,2,2).setvalue("value"); 값넣기
/// UnityWebRequest www =UnityWebRequest.Get(SheetURL);
///doget함수실행
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

public class GoogleSheetManager : SINGLETON<GoogleSheetManager,SINGLETONE.SINGLETONEType.DontDestroy>
{
    //시트 코드 들어간 url
    private const string SheetURL =
        "https://script.google.com/macros/s/AKfycbzym5ZZrUMGM7DBnLUMdy4qdL7QZQ4pgvQJw5wCLeXAT-86KfEqKSUlOUSl44IwROEi/exec";
    //시트 데이터 url //A2:B
    private const string SheetDataURL = "https://docs.google.com/spreadsheets/d/1vh9r5U06xLEDy3_7rc-1L9sNFuDwEN50hDablbkq5lE/export?format=tsv"; 

    [SerializeField] GoogleData ProcessGoogleData;


    

    IEnumerator Post(WWWForm form,Action<bool,string> afterProcess=null) //등록
    {
        using (UnityWebRequest www = UnityWebRequest.Post(SheetURL, form))
        {
            yield return www.SendWebRequest();
            if (www.isDone) Response(www.downloadHandler.text, afterProcess);
            else afterProcess?.Invoke(false,"PostFAil");
        }
    }

    void Response(string json, Action<bool, string> afterProcess=null)
    {
        if (string.IsNullOrEmpty(json))
        {
            afterProcess?.Invoke(false,"데이터가없는걸");
            return;
        }
        Debug.Log(json);
        ProcessGoogleData = JsonUtility.FromJson<GoogleData>(json);
        switch (ProcessGoogleData.order)
        {
            case  "login":
                if (ProcessGoogleData.result == "T")
                {
                    afterProcess?.Invoke(true,ProcessGoogleData.value);
                }
                else if(ProcessGoogleData.result=="F")
                {
                    afterProcess.Invoke(false,ProcessGoogleData.message);
                }
                break;
            case "register":
                if (ProcessGoogleData.result == "T")
                {
                    afterProcess?.Invoke(true,ProcessGoogleData.message);
                }
                else if(ProcessGoogleData.result=="F")
                {
                    afterProcess.Invoke(false,ProcessGoogleData.message);
                }
                break;
            case  "reRegister":
                if (ProcessGoogleData.result == "T")
                {
                    afterProcess?.Invoke(true,ProcessGoogleData.message);
                }
                else if(ProcessGoogleData.result=="F")
                {
                    afterProcess.Invoke(false,ProcessGoogleData.message);
                }
                break;

        }
    }

    public void Register(string nickName, Action<bool, string> afterProcess) //회원가입
    {
        WWWForm form = new WWWForm();
        form.AddField("order","register");
        form.AddField("nickname",nickName);
        StartCoroutine(Post(form, afterProcess));
    }

    public void ReRiegister(string nickName,string value, Action<bool, string> afterProcess)//값다시 넣기
    {
        WWWForm form = new WWWForm();
        form.AddField("order","reRegister");
        form.AddField("nickname",nickName);
        form.AddField("value",value);
        StartCoroutine(Post(form, afterProcess));
    }

    public void Login(string nickName, Action<bool, string> afterProcess)
    {
        WWWForm form = new WWWForm();
        form.AddField("order","login");
        form.AddField("nickname",nickName);
        StartCoroutine(Post(form, afterProcess));
    }

    private void OnApplicationQuit()
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "logout");

        StartCoroutine(Post(form));
    }


    public void LoadData(Action<bool, string, string> afterProcess)
    {
        StartCoroutine(C_LoadData(afterProcess));
    }
    IEnumerator C_LoadData(Action<bool,string,string> afterPrcess)
    {
        UnityWebRequest request = new UnityWebRequest();
        using (request = UnityWebRequest.Get(SheetDataURL))
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
    }
    

    
    

    protected override void Awake()
    {
        base.Awake();
    }
    
    

}
