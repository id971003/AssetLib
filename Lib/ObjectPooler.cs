using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using OBJECTPOOL;




public class ObjectPooler : SINGLETON<ObjectPooler,SINGLETONE.SINGLETONEType.DontDestroy>
{
    private string filePath=@"Assets\Scripte\Lib\ObjectPoller\OBJECTPOOL.cs";
    private string message = "namespace OBJECTPOOL { public enum ObjectPool {";
    private string message2 = "}}\n";

    private string PrefabPath = "ObjectPool";

    [SerializeField] Dictionary<ObjectPool, List<GameObject>> Dic_NameToQueueGameObject = new Dictionary<ObjectPool, List<GameObject>>();

    [Button]
    void Init()
    {
        Init_1();
        Init_2();
    }
    void Init_1()
    {        FileStream fileStream
            = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);

        StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.Unicode);

        writer.WriteLine(message);
        
        GameObject[] poolArray=Resources.LoadAll<GameObject>(PrefabPath);
        for (int i = 0; i < poolArray.Length; i++)
        {
            //enum 생성
            var prefabName = poolArray[i].name.Substring(0, poolArray[i].name.Length - 1);
            if (i != 0)
            {
                Debug.Log(prefabName);
                writer.WriteLine(","+ prefabName);
            }
            else
            {
                Debug.Log(prefabName);
                writer.WriteLine(prefabName);    
            }
            
        }
        writer.WriteLine(message2);
        writer.Close();
        fileStream.Close();
    }
    void Init_2()
    {
        //초기화
        for (int i = transform.childCount-1; i >=0 ; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        Dic_NameToQueueGameObject.Clear();
        


        GameObject[] poolArray=Resources.LoadAll<GameObject>(PrefabPath);
        
        for (int i = 0; i < poolArray.Length; i++)
        {
            //enum 생성
            var prefabName = poolArray[i].name.Substring(0, poolArray[i].name.Length - 1);
            //이름 갯수설정
            if(!int.TryParse(poolArray[i].name.Substring(poolArray[i].name.Length-1),out int count))
                Debug.Log("ObjectPool_Error : "+prefabName+"LastName Is Not Int");
            
            if(count==0)
                Debug.LogError("ObjectPooler Init Error "+prefabName +"count 0");
            if(Dic_NameToQueueGameObject.ContainsKey((ObjectPool)Enum.Parse(typeof(ObjectPool),prefabName)))
                Debug.LogError("ObjectPooler Init Error "+prefabName +"already in dic");

            List<GameObject> targetList = new List<GameObject>();
            for (int j = 0; j < count; j++)
            {
                var A= Instantiate(poolArray[i], gameObject.transform);
                A.name = prefabName;
                targetList.Add(A);
                A.SetActive(false);
            }


            Dic_NameToQueueGameObject.Add((ObjectPool)Enum.Parse(typeof(ObjectPool),prefabName),targetList);
            
        }

    }




    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    
}
