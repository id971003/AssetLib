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
    GameObject CreateNewObject(ObjectPool objectName)
    {
        var obj = Instantiate(Dic_NameToPreFab[objectName], transform);
        obj.name = objectName.ToString();
        obj.SetActive(false);
        ArrayObject(obj);
        return obj;
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
    public static T _SpawnFormPool_Ui<T> (ObjectPool objectName,GameObject parent) where T : Component
    {
        return Instance._SpawnFormPool_Ui(objectName, parent).GetComponent<T>();
    }
    

    GameObject _SpawnFormPool(ObjectPool objectName, Vector3 position, quaternion rotation)
    {
        if(!Dic_NameToQueueGameObject.ContainsKey(objectName))
            Debug.LogError("error - objectpooler"+objectName);
        GameObject go;
        if (Dic_NameToQueueGameObject[objectName].Count <= 0)
            go= CreateNewObject(objectName);

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
            go= CreateNewObject(objectName);

        go = Dic_NameToQueueGameObject[objectName].Dequeue();
        go.transform.position = parent.transform.position;
        go.transform.parent = parent.transform;
        go.SetActive(true);
        return go;
    }
    GameObject _SpawnFormPool_Ui(ObjectPool objectName,GameObject parent)
    {
        if(!Dic_NameToQueueGameObject.ContainsKey(objectName))
            Debug.LogError("error - objectpooler"+objectName);
        GameObject go;
        if (Dic_NameToQueueGameObject[objectName].Count <= 0)
            go= CreateNewObject(objectName);

        go = Dic_NameToQueueGameObject[objectName].Dequeue();
        go.transform.position = parent.transform.position;
        go.transform.SetParent(parent.transform);
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
    }
    

    #endregion
    protected override void Awake()
    {
        base.Awake();
        Init_Object();
    }

    
}
