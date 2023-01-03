/// <summary>
/// public class objectpooltest : ObjectPoolObject
///     public override void SetUp()
///{
///}
/// 
/// </summary>
using UnityEngine;

public abstract class ObjectPoolObject : MonoBehaviour
{
    public abstract void SetUp();

    

    protected virtual void OnDisable()
    {
        ObjectPooler.ReturnToPool(gameObject);
        StopAllCoroutines();
        CancelInvoke();
    }
}
