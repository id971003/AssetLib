/*
 MADE 7rzr 2023-01-04
Update 12-21 InitEnum 에 Reflash 자동화
 
ObjectPooler.SpawnFormPool(ObjectPool.a,gameObject.transform.position);

자동인잇하는 오브젝트플러
1. objectpool enum filepath 지정
2. 풀링할 오브젝트들 있는 리소스폴더에 prefabpath 지정
3. init 버튼 누르면 체크됨 안눌러도 ㄱㅊ


+Update 2024-02-05
부모가 Destory 됬을때 문제가 좀있어 보수 고드 넣어놨다.
ObjectPoolObject-OnDestoryParent
ObjectPooler-returnpool 예외 코드 추가

*Update 2024-02-09
ReturnTopool 을 할때 돌아온친구들을 Pooler 자식으로 넣어주는 setParent 를해주고싶었다.
근데 이걸 각오브젝트들에게 OnDisable 에서 실행하는데 오브젝트의 Active 가 꺼져있을때 계층구조를 바꿀수가 없어서
풀링할 오브젝트들이 상복받는 ObjectPoolObject 에 BackPool을 Invoke 로 실행해줬다 나중에 문제생기면 바꾸면 되겠지

*Update 2024-02-16
2-9 내용에서 Invoke 사용하니 초기셋업 시 오브젝트가 꺼지는걸 Soawnpool 이후에 해 오브젝트가 한번에 안켜져
ObjectPoolOjbect 에 FirstSetUp 변수 추가
FirstSetUp은 풀링오브젝트가 생성시에만 관리해주면됨
SpawnPool중 Parent 를 지정하는 타입의 경우 Parent 의 Active 가 false 되면 다시 풀로 돌아오게 해놨다
*/
using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using OBJECTPOOL;
using UnityEditor;
using Unity.Mathematics;



public class ObjectPooler : SINGLETON<ObjectPooler,Ns_SINGLETONE.SINGLETONEType.DoNotDontDestroy>
{

    [SerializeField] Dictionary<ObjectPool, Queue<GameObject>> Dic_NameToQueueGameObject = new Dictionary<ObjectPool, Queue<GameObject>>();
    [SerializeField] Dictionary<ObjectPool, GameObject> Dic_NameToPreFab = new Dictionary<ObjectPool, GameObject>();

    private string filePath = @"Assets\AssetLib\Lib\ObjectPooler\OBJECTPOOL.cs";
    private readonly string message = "namespace OBJECTPOOL { public enum ObjectPool {";
    private readonly string message2 = "}}\n";

    private string PrefabPath = "ObjectPool";
    #region init
    [Button]
    void Init_EnumType()
    {        
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);

        StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.Unicode);

        writer.WriteLine(message);
        
        GameObject[] poolArray=Resources.LoadAll<GameObject>(PrefabPath);
        for (int i = 0; i < poolArray.Length; i++)
        {
            int countslide = poolArray[i].name.LastIndexOf('_');
            //enum 생성
            var prefabName = poolArray[i].name.Substring(0, countslide);
            if (i != 0)
            {
                writer.WriteLine(","+ prefabName);
            }
            else
            {
                writer.WriteLine(prefabName);    
            }
            
        }
        writer.WriteLine(message2);
        writer.Close();
        fileStream.Close();
#if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    [Button]
    void Init_Object()
    {
        //초기화
        for (int i = transform.childCount-1; i >=0 ; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        Dic_NameToQueueGameObject.Clear();
        Dic_NameToPreFab.Clear();
        GameObject[] poolArray=Resources.LoadAll<GameObject>(PrefabPath);

        for (int i = 0; i < poolArray.Length; i++)
        {
            //enum 생성
            int countslide = poolArray[i].name.LastIndexOf('_');
            //enum 생성
            var prefabName = poolArray[i].name.Substring(0, countslide);
            if(!int.TryParse(poolArray[i].name.Substring(countslide+1),out int count))
                Debug.LogError("ObjectPool_Error : "+prefabName+"LastName Is Not Int");
            ObjectPool OBJECTPOOL = (ObjectPool)Enum.Parse(typeof(ObjectPool), prefabName);
            
            Dic_NameToPreFab.Add(OBJECTPOOL,poolArray[i]);
            CreateNewObjectPool(OBJECTPOOL, count);
        }
        
    }

    #endregion

    void CreateNewObjectPool(ObjectPool objectName,int count)
    {
        Dic_NameToQueueGameObject.Add(objectName,new Queue<GameObject>());
        
        for (int i = 0; i < count; i++)
        {
            CreateNewObject(objectName);
        }
    }
    void CreateNewObject(ObjectPool objectName)
    {
        var obj = Instantiate(Dic_NameToPreFab[objectName], transform);
        obj.name = objectName.ToString();
        Debug.Log("4");
        obj.SetActive(false);
        ArrayObject(obj);
        Dic_NameToQueueGameObject[objectName].Enqueue(obj);
    }

    void ArrayObject(GameObject obj)
    {
        bool isFind = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i == transform.childCount - 1)
            {
                obj.transform.SetSiblingIndex(i);
                break;
            }
            else if (transform.GetChild(i).name == obj.name)
                isFind = transform;
            else if (isFind)
            {
                obj.transform.SetSiblingIndex(i); 
                break;
            }
        }
    }
    

    #region SpawnPool
    public static GameObject SpawnFormPool(ObjectPool objectName, Vector3 position)
    {
        return Instance._SpawnFormPool(objectName, position, quaternion.identity);
    }
    public static GameObject SpawnFormPool(ObjectPool objectName,GameObject parent)
    {
        return Instance._SpawnFormPool(objectName, parent);
    }
    public static GameObject SpawnFormPool(ObjectPool objectName, Vector3 position,quaternion rotation)
    {
        return Instance._SpawnFormPool(objectName, position, rotation);
    }
    public static T SpawnFormPool<T> (ObjectPool objectName, Vector3 position) where T : Component
    {
        return Instance._SpawnFormPool(objectName, position, quaternion.identity).GetComponent<T>();
    }
    public static T SpawnFormPool<T> (ObjectPool objectName, Vector3 position,quaternion rotation) where T : Component
    {
        return Instance._SpawnFormPool(objectName, position, rotation).GetComponent<T>();
    }
    public static T SpawnFormPool<T> (ObjectPool objectName,GameObject parent) where T : Component
    {
        return Instance._SpawnFormPool(objectName, parent).GetComponent<T>();
    }

    #endregion

    #region _SpawnPool
    GameObject _SpawnFormPool(ObjectPool objectName, Vector3 position, quaternion rotation)
    {
        if(!Dic_NameToQueueGameObject.ContainsKey(objectName))
            Debug.LogError("error - objectpooler"+objectName);
        GameObject go;
        
        if (Dic_NameToQueueGameObject[objectName].Count <= 0)
        {
            CreateNewObject(objectName);
        }

        go = Dic_NameToQueueGameObject[objectName].Dequeue();
        go.transform.position = position;
        go.transform.transform.rotation = rotation;
        go.SetActive(true);
        return go;
    }
    GameObject _SpawnFormPool(ObjectPool objectName,GameObject parent)
    {
        if(!Dic_NameToQueueGameObject.ContainsKey(objectName))
            Debug.LogError("error - objectpooler"+objectName);
        
        GameObject go;
        if (Dic_NameToQueueGameObject[objectName].Count <= 0)
        {
            CreateNewObject(objectName);
        }
        go = Dic_NameToQueueGameObject[objectName].Dequeue();
        go.transform.position = parent.transform.position;
        go.transform.parent = parent.transform;
        go.SetActive(true);
        return go;
    }

    #endregion
    #region returnpool

    public static void ReturnToPool(GameObject obj)
    {
        if(!Enum.TryParse(obj.name,out ObjectPool value))
            Debug.LogError("error = objectpooler return error"+obj.name);
        
        if(!Instance.Dic_NameToQueueGameObject.ContainsKey(value))
            Debug.LogError("error = objectpooler return error"+obj.name);


        if (Instance.Dic_NameToQueueGameObject[value].Contains(obj))
            return;

        Instance.Dic_NameToQueueGameObject[value].Enqueue(obj);
        obj.transform.SetParent(Instance.gameObject.transform);
    }
    

    #endregion
    protected override void Awake()
    {
        base.Awake();
        Init_Object();
    }

    
}
