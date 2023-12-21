/*
public class objectpooltest : ObjectPoolObject
     public override void SetUp()
 
 SetUp 오버라이딩 해서 사용
 
 
*/
using UnityEngine;

public abstract class ObjectPoolObject : MonoBehaviour
{
    public abstract void SetUp();

    

    protected virtual void OnDisable()
    {
        Debug.Log("a");
        ObjectPooler.ReturnToPool(gameObject);
        StopAllCoroutines();
        CancelInvoke();
    }
}
