/// <summary>
/// 싱글톤
/// 데이터 메니저임 여기서 가따쓰자
/// 
/// </summary>



public class DatasManager :  SINGLETON<DatasManager,SINGLETONE.SINGLETONEType.DontDestroy>
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
