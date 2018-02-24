using UnityEngine;
using System.Collections;

/// <summary>
/// 아이템 접속 판정.
/// </summary>
public class ItemCollider : MonoBehaviour
{

    // [SerializeField]
    // private int type;

    private bool isFinished;

    void Start()
    {
        isFinished = false;
        collider.isTrigger = true;  // トリガーをたてておく
    }

    void OnTriggerEnter(Collider collider)
    {
        if (isFinished) return; // 1회만 충돌을 테스트하기 위한 감시용.
                                // isTrigge=false하더라도 여러 번 실행된다.
        GameObject obj = collider.gameObject;
        if (obj.tag.Equals("Player"))   // 플레이어가 판정.
        {
            isFinished = true;
            // HitItem알림
            //obj.SendMessage("OnHitItem");
            GameObject ui = GameObject.Find("/UI");
            if (ui) ui.SendMessage("OnHitItem", gameObject.name);
            Note note = GetComponent<Note>();
            if (note) note.SendMessage("OnHitItem");
        }
    }
}
