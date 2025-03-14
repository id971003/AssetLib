/*
MADE 7rzr 2023-01-06
Update 2025-03-14
조건 : SingleTon
세팅 : 뭐없음 미리 캐싱해서만 쓰자

이전 코드는 사실 Class Json 화 시키는 정도임


*/
#region OldCode
/*
DataList datalistref = DatasManager.Instance.Datalist;
1-7 일 기준으로 내가 데이터를 저장할때는 클래스하나를 json 화 시켜서 저장한다 그래서 저장되는 모든데이터를 가지고있는 클레스 하나를 Serializable 해서 가지고 다니면서 바꾸고 저장하고 할생각임


Datalist 를 캐싱했을때 후 로드 진행하면 datalistref.Value 값이 제대로 안들어간다 다시한번 캐싱해줘야한다

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

public class DatasManager :  SINGLETON<DatasManager,Ns_SINGLETONE.SINGLETONEType.DontDestroy>
{
    public DataList DataList;
    
    protected override void Awake()
    {
        base.Awake();
    }
    [Button]
    public void Save_Test()
    {
        string path = Path.Combine(Application.dataPath, "TestData.json");
        string data = JsonUtility.ToJson(DataList,true);
        File.WriteAllText(path,data);
    }
    [Button]
    public void Load_Test()
    {
        string path = Path.Combine(Application.dataPath, "TestData.json");
        string a = File.ReadAllText(path);
        DataList = JsonUtility.FromJson<DataList>(a);
    }
}

[System.Serializable]
public class DataList
{
}
*/
#endregion

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;


public class UserDataManager : MonoBehaviour
{
    [SerializeField] private string m_folderPath = Application.persistentDataPath;

    [SerializeField] private string m_userFilePath = "";


    private bool m_isDataLoaded = false;
    public bool IsDataLoaded() => m_isDataLoaded;

    #region Data Load & Save

    public async Task Save<T>(T t, string filePath) => await JsonSerialize(t, filePath);

    private async Task JsonSerialize<T>(T t, string filePath)
    {
        try
        {
            string json = JsonConvert.SerializeObject(t, Formatting.Indented);
            string base64Encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
            await File.WriteAllTextAsync(filePath, base64Encoded);

            Debug.Log("Save Data: " + filePath);
        }
        catch (Exception error)
        {
            Debug.Log("Error Save Data: " + filePath + "-----" + error);
        }
    }


    public async Task<T> Load<T>(string filePath) => await JsonDeserializeAsync<T>(filePath);

    private async Task<T> JsonDeserializeAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }

        try
        {
            // 파일을 비동기적으로 읽기
            string base64Encoded = await File.ReadAllTextAsync(filePath);

            // Base64 디코딩
            string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Encoded));

            // JSON 문자열을 객체로 변환 (비동기 실행)
            T obj = JsonConvert.DeserializeObject<T>(json);

            if (obj == null)
            {
                Debug.LogError("역직렬화된 객체가 null입니다.");
            }

            return obj;
        }
        catch (Exception ex)
        {
            Debug.LogError($"JsonDeserialize 실패: {ex.Message}");
            return default;
        }
    }

    #endregion

    async void SampleCode()
    {
        m_userFilePath = m_folderPath + "/UserData.json";

        //저장
        _ = Save(new List<int> { 0, 1, 2 }, m_userFilePath); //비동기 실행이지만 결과 안기다림

        //로드
        List<int> userData = await Load<List<int>>(m_userFilePath); //비동기 처리를 위해서 async 함수로 로드 하길 권장.

        //상태
        m_isDataLoaded = m_isDataLoaded; //위상태가 비동기로 실행되므로, 데이터 로드가 완료되었는지 확인하기 위해 사용
    }
}
