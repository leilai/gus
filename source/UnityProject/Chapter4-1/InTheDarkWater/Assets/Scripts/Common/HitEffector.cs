using UnityEngine;
using System.Collections;

/// <summary>
/// 충돌 효과 전용.
/// OnHit가 전달되는 경우, 파티클과 사운드를 재생한다.
/// </summary>
public class HitEffector : MonoBehaviour {

    [SerializeField]
    private bool valid = true;

    void Start()
    { 
    }

    // 무효라면 사전에 불러온다.
    void OnInvalidEffect()
    {
        Debug.Log("HitEffector.OnInvalid");
        valid = false;
    }

    // 충돌 시의 작동 관리와 종료 타이밍.
    void OnHit()
    {

        Debug.Log("HitEffector.OnHit:" + transform.parent.gameObject.transform.parent.tag);
        if (valid)
        {
            if (particleSystem)
            {
                Debug.Log("HitEffector => particle.Play");
                particleSystem.Play();
            }
            if (audio)
            {
                Debug.Log("HitEffector => audio.Play");
                audio.Play();
            }
        }
        else Debug.Log("HitEffector.OnHit: Invalid");
    }

    public bool IsFinished()
    {
        bool result = true;
        if (particleSystem) result = result && !particleSystem.isPlaying;
        if (audio) result = result && !audio.isPlaying;
        return result;
    }
    public bool IsPlaying()
    {
        bool result = false;
        if (particleSystem) result = result || particleSystem.isPlaying;
        if (audio) result = result || audio.isPlaying;
        return result;
    }
}
