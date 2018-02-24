using UnityEngine;
using System.Collections;

/// <summary>
/// 스테이지 시작에서 표시되는 텍스트
/// </summary>
public class StageStartText : MonoBehaviour {

    [SerializeField]
    private float delayTime = 2.0f;
    [SerializeField]
    private string[] startText = new string[] { 
        "Stage 1", "Stage 2", "Final Stage" 
    };
    [SerializeField]
    private string[] missionText = new string[] { 
        "Kill the Enemy!", 
        "Get the Recovery Item!", 
        "Stay alive as long as possible!" 
    };

    private GUIText missionGUIText = null;
    private Color startColor;

    void Start()
    {
        GameObject missionObj = GameObject.Find("MissionText");
        missionGUIText = missionObj.guiText;
        startColor = new Color(guiText.material.color.r, guiText.material.color.g, guiText.material.color.b, guiText.material.color.a);
    }

    void OnAwakeStage(int index)
    {
        // 게임 시작 전에 문자를 준비한다.
        if (index >= startText.Length) return;
        guiText.text = startText[index];
        guiText.enabled = true;
        guiText.material.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a);
        missionGUIText.text = missionText[index];
        missionGUIText.enabled = true;
        missionGUIText.material.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a);
    }

    void OnGameStart()
    {
        enabled = true;
        StartCoroutine("Delay");
    }

    // 만일을 대비해 비표시
    void OnGameClear()
    {
        StopAllCoroutines();
        OnEndTextFade();
    }
    // 만일을 대비해 비표시
    void OnGameOver()
    {
        StopAllCoroutines();
        OnEndTextFade();
    }

    void OnEndTextFade()
    {
        enabled = false;
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(delayTime);
        // TextFader
        BroadcastMessage("OnTextFadeOut");
    }

}
