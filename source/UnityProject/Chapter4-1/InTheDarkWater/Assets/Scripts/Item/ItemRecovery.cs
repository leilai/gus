using UnityEngine;
using System.Collections;

/// <summary>
/// 아이템에 따른 회복량 설정.
/// </summary>
public class ItemRecovery : MonoBehaviour
{
    [SerializeField]
    private ItemParameter param;

    private float timeStamp = 0.0f; // 경과 시간을 측정하기 위한 타임스탬프

    void Start()
    {
    }

    void OnStartLifeTimer(ItemParameter param_)
    {
        Debug.Log("OnStartLifeTimer");
        param = param_;
        // 타임스탬프를 준비한다.
        timeStamp = Time.time;
        StartCoroutine("WaitLifeTimeEnd");
    }

    void OnDestroyObject()
    {
        // 만일에 대비하여 Coroutine은 끈다.
        StopAllCoroutines();
    }

    private IEnumerator WaitLifeTimeEnd()
    {
        yield return new WaitForSeconds(param.lifeTime);
        // 수명까지 기다리고 소실한다.
        Disappear();
    }

    // 수명이 다했기 때문에 스스로 Destory한다.
    private void Disappear()
    {
        Debug.Log("Disappear");

        GameObject ui = GameObject.Find("/UI");
        if (ui)
        {
            // Lost알림
            ui.BroadcastMessage("OnLostObject", tag, SendMessageOptions.DontRequireReceiver);
        }
        GameObject parent = gameObject.transform.parent.gameObject;
        if (parent)
        {
            // 부모에게도 Lost 알림
            parent.SendMessage("OnLostObject", tag, SendMessageOptions.DontRequireReceiver);
        }

        // 강제 삭제
        BroadcastMessage("OnInvalidEffect"); // 충돌 효과만 무효화
        BroadcastMessage("OnHit");  // 충돌 알림
    }

    /// <summary>
    /// 플레이어가 준비되면 회복
    /// </summary>
    void OnRecovery()
    {
        GameObject ui = GameObject.Find("/UI");
        if (ui)
        {
            // 충돌 알림
            ui.BroadcastMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);

            float t = (Time.time - timeStamp) / param.lifeTime;
            // 스코어 알림
            int score = (int)Mathf.Lerp(param.scoreMax, param.scoreMin, t);
            Debug.Log(t + ":ItemScore=" + score);
            ui.BroadcastMessage("OnAddScore",  score );
            // Air 회복
            int recoveryValue = (int)Mathf.Lerp(param.recoveryMax, param.recoveryMin, t);
            Debug.Log(t + ":ItemAir=" + recoveryValue);
            ui.BroadcastMessage("OnAddAir", recoveryValue);
        }

        GameObject parent = gameObject.transform.parent.gameObject;
        if (parent)
        {
            // 부모에게도 충돌 알림
            parent.SendMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);
        }

        // 충돌 후 스스로 처리
        BroadcastMessage("OnHit");  // 충돌 알림
    }

}
