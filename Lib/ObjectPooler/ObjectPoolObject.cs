/*
public class objectpooltest : ObjectPoolObject
     public override void SetUp()
 
 SetUp 오버라이딩 해서 사용
 
 
*/
using UnityEngine;

public abstract class ObjectPoolObject : MonoBehaviour
{
    public abstract void SetUp();



    void BackPool()
    {
        ObjectPooler.ReturnToPool(gameObject);
        StopAllCoroutines();
        CancelInvoke();
    }
    protected virtual void OnDisable()
    {
        BackPool();
    }

    protected virtual void OnDestoryParent()
    {
        transform.SetParent(null);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Debug.LogError("오브젝트 지워버리면 어뜨케");
    }
}
