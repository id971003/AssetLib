/*
public class objectpooltest : ObjectPoolObject
     public override void SetUp()
 
 SetUp 오버라이딩 해서 사용
 
 
 
*/

using System;
using UnityEngine;

public abstract class ObjectPoolObject : MonoBehaviour
{
    [SerializeField] private Transform Instance;

    private void Awake()
    {
        Instance = ObjectPooler.Instance.transform;
    }

    public abstract void SetUp();


    
    protected void BackPool()
    {
        ObjectPooler.ReturnToPool(gameObject);
        StopAllCoroutines();
        CancelInvoke();
    }
    protected virtual void OnDisable()
    {
        Invoke("BackPool",0);
    }
    

    public virtual void OnDestoryParent()
    {
        gameObject.SetActive(false);
    }
    
}
