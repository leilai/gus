using UnityEngine;
using System.Collections;

/// <summary>
/// Intermission. 페이드 인・아웃, 슬라이드인・아웃 4종류 준비
/// </summary>
public class IntermissionEffector : MonoBehaviour {

    public enum Type
    {
        None,
        SlideIn,    
        SlideOut,
        FadeIn,
        FadeOut
    };
    [SerializeField]
    private Type type = Type.SlideIn;
    [SerializeField]
    private float slideTime = 1.0f;
    [SerializeField]
    private int fadeAreaPixel = 200;
    [SerializeField]
    private bool playOnAwake = false;

    private float currentTime = 0.0f;
    private bool valid = false;

    private float height = 0.0f;
    private float width = 0.0f;
    private Color mainColor;

    private float startValue = 0.0f;
    private float endValue = 0.0f;

    // 임의 완료 알림용 오브젝트
    GameObject notifyObj = null;

	void Start () 
    {
        width  = (float)Screen.width;
        height = (float)guiTexture.texture.height;
        Color color = guiTexture.color;
        mainColor = new Color(color.r, color.g, color.b, color.a);

        SetType(type);
        guiTexture.pixelInset = new Rect(0, endValue, width, height);

        if (playOnAwake)
        {
            switch( type )
            {
                case Type.SlideIn: OnSlideIn(null); break;
                case Type.SlideOut: OnSlideIn(null); break;
                case Type.FadeIn: OnFadeIn(null); break;
                case Type.FadeOut: OnFadeOut(null); break;
                default: break;
            }
        }
	}
	
	void Update () 
    {
        if (!valid) return;

        currentTime += Time.deltaTime;
        float timeRate = currentTime / slideTime;
        float newValue = Mathf.Lerp(startValue, endValue, timeRate);

        switch( type ) {
            case Type.SlideIn:
            case Type.SlideOut:
                guiTexture.pixelInset = new Rect(0, newValue, width, height);
                break;
            case Type.FadeIn:
            case Type.FadeOut:
                guiTexture.color = new Color(mainColor.r, mainColor.g, mainColor.b, newValue);
                break;
            default: break;
        }

        if (timeRate >= 1.0f)
        {
            Finish();
        }
	}

    private void Finish()
    {
        valid = false;
        if (notifyObj)
        {
            notifyObj.SendMessage("OnIntermissionEnd");
            notifyObj = null;
            switch (type)
            {
                case Type.SlideOut:
                case Type.FadeOut:
                    guiTexture.enabled = false; // 표시하지 않는 것이 경제적?
                    break;
                default: break;
            }
        }
    }
    
    private void StartIntermission()
    {
        Debug.Log("StartIntermission:" + type);
        valid = true;
        currentTime = 0.0f;
    }

    void OnSlideIn(GameObject notifyObj_)
    {
        if (valid) return;  // 동작하는 동안은 무효
        notifyObj = notifyObj_;
        SetType(Type.SlideIn);
        StartIntermission();
    }
    void OnSlideOut(GameObject notifyObj_)
    {
        if (valid) return;  // 동작하는 동안은 무효
        notifyObj = notifyObj_;
        SetType(Type.SlideOut);
        StartIntermission();
    }
    void OnFadeIn(GameObject notifyObj_)
    {
        if (valid) return;  // 동작하는 동안은 무효
        notifyObj = notifyObj_;
        SetType(Type.FadeIn);
        StartIntermission();
    }
    void OnFadeOut(GameObject notifyObj_)
    {
        if (valid) return;  // 동작하는 동안은 무효
        notifyObj = notifyObj_;
        SetType(Type.FadeOut);
        StartIntermission();
    }

    void SetDefault()
    {
        // Default
        guiTexture.pixelInset = new Rect(0, -(float)fadeAreaPixel, width, height);
        guiTexture.color = new Color(mainColor.r, mainColor.g, mainColor.b, mainColor.a);
        guiTexture.enabled = true;
    }

    void SetType(Type type_)
    {
        SetDefault();   // 한 번 Default로 되돌아간다.
        type = type_;
        switch (type)
        {
            case Type.SlideIn:
                startValue = height;
                endValue = -(float)fadeAreaPixel;
                break;
            case Type.SlideOut: 
                startValue = -(float)fadeAreaPixel;
                endValue = height;
                break;
            case Type.FadeIn:
                startValue = 0.0f;
                endValue = mainColor.a;
                break;
            case Type.FadeOut:
                startValue = mainColor.a;
                endValue = 0.0f;
                guiTexture.pixelInset = new Rect(0, -(float)fadeAreaPixel, width, height);
                break;
            default:    
                guiTexture.enabled = false;
                break;
        }
    }
}
