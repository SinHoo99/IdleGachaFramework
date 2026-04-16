using UnityEngine;
using System.Collections;

public class AutoReturnEffect : PoolObject
{

    public override void OnSpawn(Vector3 position, Quaternion rotation)
    {
        base.OnSpawn(position, rotation);
    }

    public void ReturnToPool()
    {
        this.gameObject.SetActive(false);
    }
}
