/*
public class objectpooltest : ObjectPoolObject
     public override void SetUp()
 
 SetUp 오버라이딩 해서 사용
 
 
 
*/

using System;
using UnityEngine;

public abstract class ObjectPoolObject : MonoBehaviour
{
    private bool FirstSetUp;
    private void Awake()
    {
        FirstSetUp = true;
    }

    public abstract void SetUp();


    
    protected void BackPool()
    {
        gameObject.SetActive(false);
        ObjectPooler.ReturnToPool(gameObject);
        StopAllCoroutines();
        CancelInvoke();
    }
    protected virtual void OnDisable()
    {
        if (FirstSetUp)
        {
            FirstSetUp = false;
        }
        else
        {
            Invoke("BackPool", 0);
        }
    }
    
}