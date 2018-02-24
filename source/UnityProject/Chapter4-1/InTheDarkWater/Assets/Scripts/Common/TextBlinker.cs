using UnityEngine;
using System.Collections;

/// <summary>
/// テキストの?滅
/// </summary>
public class TextBlinker : MonoBehaviour
{
    [SerializeField]
    private bool valid = true;
    [SerializeField]
    private float blinkTime = 0.8f;
    [SerializeField]
    private int num = 5;

    private int count = 0;

    void Start()
    {
    }

    // ?滅ス??ト
    void OnStartTextBlink()
    {
        if (!guiText || !valid) return;
        count = 0;
        guiText.enabled = true;
        StartCoroutine("Delay", blinkTime);
    }


    private IEnumerator Delay(float delaytime)
    {
        yield return new WaitForSeconds(delaytime);
        guiText.enabled = !guiText.enabled;
        count++;
        if (count < num)
        {
            StartCoroutine("Delay", blinkTime);
        }
        else
        {
            // 終了通知
            SendMessage("OnEndTextBlink", SendMessageOptions.DontRequireReceiver);
        }
    }
}
