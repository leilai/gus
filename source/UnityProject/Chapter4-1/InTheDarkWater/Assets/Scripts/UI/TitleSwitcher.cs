using UnityEngine;
using System.Collections;

/// <summary>
/// 사용자 클릭후, Adapter에 씬 종료를 전한다.
/// </summary>
public class TitleSwitcher : MonoBehaviour {

    [SerializeField]
    private float waitTime = 3.0f;

    private bool pushed = false;
    private bool fade = false;
   
    void Start()
    {
        guiText.enabled = false;
        Color basecolor = guiText.material.color;
        guiText.material.color = new Color(basecolor.r, basecolor.g, basecolor.b, 0.0f);
    }

    void Update()
    {
        if (!guiText.enabled) return;

        if ( !pushed && Input.GetMouseButtonDown(0))
        {
            pushed = true;
            audio.Play();
            // 씬 종료 전달한다.
            GameObject adapter = GameObject.Find("/Adapter");
            if (adapter) adapter.SendMessage("OnSceneEnd");
            else Debug.Log("adapter is not exist...");
        }
	}

    /// <summary>
    /// 페이드 종료시에 불러올 수 있다.
    /// </summary>
    void OnEndTextFade()
    {
        if (!guiText.enabled) return;
        StartCoroutine("Delay");
    }

    /// <summary>
    /// Switch 시작
    /// </summary>
    void OnStartSwitcher()
    {
        Debug.Log("OnStartSwitcher");
        guiText.enabled = true;
        fade = true;
        SendMessage("OnTextFadeIn");
    }
    /// <summary>
    /// 스테이지 재설정
    /// </summary>
    void OnStageReset()
    {
        guiText.enabled = false;
        Color basecolor = guiText.material.color;
        guiText.material.color = new Color(basecolor.r, basecolor.g, basecolor.b, 0.0f);
        pushed = false;
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(waitTime);
        // FadeIn과 FadeOut을 교체하여 실행
        fade = !fade;
        if (fade) SendMessage("OnTextFadeIn");
        else SendMessage("OnTextFadeOut");
    }

}
