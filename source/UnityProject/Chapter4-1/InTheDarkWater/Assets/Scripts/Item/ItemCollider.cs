using UnityEngine;
using System.Collections;

/// <summary>
/// 아이템 충돌
/// </summary>
public class ItemCollider : MonoBehaviour
{
    [SerializeField]
    private bool valid = true;  // 만일을 대비해 플래그 관리를 한다.

    void Start()
    {
    }

    /// <summary>
    /// Note에서 삭제허용
    /// </summary>
//    void OnDestroyObject()
    void OnDestroyLicense()
    {
        // 부모가 삭제하도록 한다.
        transform.parent.gameObject.SendMessage("OnDestroyObject", gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckPlayer(collision.gameObject);
    }
    void OnCollisionStay(Collision collider)
    {
        CheckPlayer(collider.gameObject);
    }

    void CheckPlayer(GameObject target)
    {
        if (! valid) return;
        if (!target.CompareTag("Player")) return;
        // Collider를 끈다.
        collider.enabled = false;
        valid = false;
        // 플레이어와 접촉했을 때의 효과
        SendMessage("OnRecovery");
    }

}
