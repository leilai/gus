using UnityEngine;
using System.Collections;

/// <summary>
/// 스테이지 종료 교체로 표시되는 텍스트
/// </summary>
public class StageEndText : MonoBehaviour {

    [SerializeField]
    private float backtitleDelay = 3.0f;
    [SerializeField]
    private string gameclearText = "STAGE CLEAR";
    [SerializeField]
    private string gameoverText = "GAME OVER";

    void Start()
    {
    }

    // 게임 클리어 알림
    void OnGameClear()
    {
        guiText.text = gameclearText;
        StartCoroutine("Wait");
    }
    // 게임 오버 알림
    void OnGameOver()
    {
        guiText.text = gameoverText;
        StartCoroutine("Wait");
    }
    // 스테이지 재설정
    void OnStageReset()
    {
        guiText.enabled = false;
    }
    
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(backtitleDelay);
        guiText.enabled = true;
        BroadcastMessage("OnStartSwitcher");
    }

}
