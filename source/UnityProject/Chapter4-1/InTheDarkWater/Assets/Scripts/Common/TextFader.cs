using UnityEngine;
using System.Collections;

/// <summary>
/// 텍스트의 페이드
/// </summary>
public class TextFader : MonoBehaviour {

    [SerializeField]
    private float waitTime = 0.05f;
    [SerializeField]
    private float fadeTime = 3.0f;
    [SerializeField]
    private float maxAlpha = 1.0f;
    [SerializeField]
    private float minAlpha = 0.0f;

    private float fromValue = 0.0f;
    private float toValue   = 1.0f;
    private Color baseColor;

	void Start () 
    {
        if (guiText) baseColor = new Color(guiText.material.color.r, guiText.material.color.g, guiText.material.color.b, guiText.material.color.a);
	}

    // 페이드 아웃 시작
    void OnTextFadeOut()
    {
        if (!guiText) return;
        Debug.Log("OnTextFadeOut");
        fromValue = maxAlpha;
        toValue = minAlpha;
        //Coroutine으로 페이드 처리.
        StartCoroutine("Fade", fadeTime);
    }

    // 페이드 인 시작.
    void OnTextFadeIn()
    {
        if (!guiText) return;
        Debug.Log("OnTextFadeIn");
        fromValue = minAlpha;
        toValue = maxAlpha;
        //Coroutine으로 페이드 처리.
        StartCoroutine("Fade", fadeTime);
    }

    private IEnumerator Fade(float duration)
    {
        // 페이드
        float currentTime = 0.0f;
        while (duration > currentTime)
        {
            float alpha = Mathf.Lerp(fromValue, toValue, currentTime/duration);
            guiText.material.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            // 시간 갱신.
            yield return new WaitForSeconds(waitTime);
            currentTime += waitTime;
        }
        // 페이드 종료 알림.
        SendMessage("OnEndTextFade", SendMessageOptions.DontRequireReceiver);
    }
}
