/*
MADE 7rzr 2023-01-06
Update 2023-12-27
조건 : SingleTon
세팅 : 뭐없음 미리 캐싱해서만 쓰자
DataList datalistref = DatasManager.Instance.Datalist;
1-7 일 기준으로 내가 데이터를 저장할때는 클래스하나를 json 화 시켜서 저장한다 그래서 저장되는 모든데이터를 가지고있는 클레스 하나를 Serializable 해서 가지고 다니면서 바꾸고 저장하고 할생각임


Datalist 를 캐싱했을때 후 로드 진행하면 datalistref.Value 값이 제대로 안들어간다 다시한번 캐싱해줘야한다
*/

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

