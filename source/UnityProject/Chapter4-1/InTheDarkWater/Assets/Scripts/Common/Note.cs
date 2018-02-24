using UnityEngine;
using System.Collections;

/// <summary>
/// 정기적으로 사운드를 재생한다.
/// 충돌 후는 페이드 아웃 처리.
/// 충돌 효과 종료를 확인하고 오브젝트 폐기를 부모에게 전달한다.
/// </summary>
public class Note : MonoBehaviour {
    [SerializeField]
    private float interval = 1.0f;  // 사운드를 재생하는 감각.
    [SerializeField]
    private float offset = 0.0f;    // 처음에 있는 타이밍의 차이.
    [SerializeField]
    private bool valid   = true;    // true로 유효.

    private HitEffector hitEffector = null;
    private float counter = 0.0f;

	void Start () 
    {
        hitEffector = gameObject.GetComponentInChildren<HitEffector>();
        counter = offset;
    }

	void FixedUpdate () 
    {
        if (valid && !audio.isPlaying) 
        {
            Clock(Time.deltaTime);
        }
	}


    private void Clock(float step)
    {
        counter += step;
        if (counter >= interval)
        {
            audio.Play();
            counter = 0.0f;
        }
    }

    /// <summary>
    /// 사운드의 유효・무효.
    /// </summary>
    /// <param name="flag"></param>
    public void SetEnable(bool flag) { valid = flag; }

    void OnHit()
    {
        valid = false;
        // Stop을 사용하면 사운드가 깨지는 경우가 존재하므로, 음량을 페이드 아웃시켜 대응.
        //audio.Stop();
        StartCoroutine("Fadeout", 1.0f);
    }


    /// <summary>
    /// 페이드 아웃 과정
    /// /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator Fadeout(float duration)
    {
        // 페이드 아웃
        float currentTime = 0.0f;
        float waitTime = 0.02f;
        float firstVol = audio.volume;
        while (duration > currentTime)
        {
            audio.volume = Mathf.Lerp(firstVol, 0.0f, currentTime / duration);
            yield return new WaitForSeconds(waitTime);
            currentTime += waitTime;
        }

        // 효과가 완전히 종료하면 오브젝트 폐기
        if (hitEffector)
        {
            while (hitEffector.IsPlaying())
            {
                yield return new WaitForSeconds(waitTime);
            }
        }
        // 삭제 메세지 요구.
        transform.parent.gameObject.SendMessage("OnDestroyLicense");
    }
}
