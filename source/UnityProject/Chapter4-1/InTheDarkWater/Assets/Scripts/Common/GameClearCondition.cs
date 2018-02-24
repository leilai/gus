using UnityEngine;
using System.Collections;

/// <summary>
/// 클리어 조건.
/// </summary>
public class GameClearCondition : MonoBehaviour
{
    [SerializeField]
    private bool valid = false;
    [SerializeField]
    private int destoryNorma = 0;
    [SerializeField]
    private int hitNorma = 0;

    private GameObject field = null;


    void Start()
    {
        field = GameObject.Find("/Field");
    }

    /// <summary>
    /// 생성한 타이밍.
    /// </summary>
    /// <param name="target"></param>
    void OnInstantiatedChild(GameObject target)
    {
        // 1개라도 생성되면 허가.
    }

    /// <summary>
    /// 폐기되는 타이밍.
    /// </summary>
    /// <param name="target"></param>
    void OnDestroyObject(GameObject target)
    {
        if (!valid) return;
        if (destoryNorma == 0) return;

        // 
        destoryNorma--;
        if (destoryNorma <= 0) Clear(target.tag);
    }

    /// <summary>
    /// 충돌 타이밍.
    /// </summary>
    /// <param name="tag"></param>
    void OnHitObject(string tag)
    {
        if (!valid) return;
        if (hitNorma == 0) return;

        // 충돌한 타이밍.
        hitNorma--;
        if (hitNorma <= 0) Clear(tag);
    }

    /// <summary>
    /// Lost 타이밍.
    /// </summary>
    /// <param name="tag"></param>
    void OnLostObject(string tag)
    {
    }

    private void Clear( string tag )
    {
        if (field) field.SendMessage("OnClearCondition", tag);
        valid = false;
    }
}
