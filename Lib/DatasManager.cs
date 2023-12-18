//  <summary>
// MADE 7rzr 2023-01-06
// 조건 : SingleTon
// 세팅 : 뭐없음 미리 캐싱해서만 쓰자
//     ```
// DataList datalistref = DatasManager.Instance.Datalist;
//     ```
// 1-7 일 기준으로 내가 데이터를 저장할때는 클래스하나를 json 화 시켜서 저장한다 그래서 저장되는 모든데이터를 가지고있는 클레스 하나를 Serializable 해서 가지고 다니면서 바꾸고 저장하고 할생각임
//  </summary>



public class DatasManager :  SINGLETON<DatasManager,Ns_SINGLETONE.SINGLETONEType.DontDestroy>
{
    public DataList DataList;
    
    protected override void Awake()
    {
        base.Awake();
    }
}

[System.Serializable]
public class DataList
{
}
