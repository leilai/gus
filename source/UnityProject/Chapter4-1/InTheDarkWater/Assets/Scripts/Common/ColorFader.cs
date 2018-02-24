using UnityEngine;
using System.Collections;

/// <summary>
/// 탐지기 포인트의 페이드.
/// 표시・비표시 플래그 관리.
/// </summary>
public class ColorFader : MonoBehaviour {

    [SerializeField]
    private float duration = 2.0f;
    [SerializeField]
    private float delay = 1.0f;
    [SerializeField]
    private float minAlpha = 0.1f;

    [SerializeField]
    private bool sonarHit = false;
    [SerializeField]
    private bool sonarInside = false;

    private bool wait = false;
    private float max = 0.0f;
    private float currentTime = 0.0f;
    private Color startColor;

	void Start () 
    {
        max = 1.0f - minAlpha;
        startColor = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, renderer.material.color.a);

        // 생성된 단계에서 탐지기 내에 존재하는지 체크.
        GameObject player = GameObject.Find("/Field/Player");
        if (player)
        {
            player.BroadcastMessage("OnInstantiatedSonarPoint", gameObject);
        }
	}

	void Update () 
    {
        if (!renderer.enabled) return;

        if (!wait)
        {
            float time = currentTime / duration;
            if (time <= (2.0f*max))
            {
                float alpha = 1.0f - Mathf.PingPong(time, max);
                renderer.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                // 시간 갱신.
                currentTime += Time.deltaTime;
            }
            else
            {
                wait = true;
                StartCoroutine("Delay", delay);
            }
        }
	}
    
    void OnTriggerEnter(Collider other)
    {
//        Debug.Log("OnTriggerEnter:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarCamera_Enter(other.gameObject);
    }
    void OnTriggerStay(Collider other)  // Enter로 누락이 발생할 가능성이 존재하므로, Stay로도 확인.
    {
//        Debug.Log("OnTriggerStay:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarCamera_Enter(other.gameObject);
    }
    void OnTriggerExit(Collider other)
    {
//        Debug.Log("OnTriggerExit:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarCamera_Exit(other.gameObject);
    }

    void CheckSonarCamera_Enter(GameObject target)
    {
        if (sonarInside) return;
        if (!target.CompareTag("SonarCamera")) return;

        Debug.Log("CheckSonarCamera_Enter");
        OnSonarInside();
    }

    void CheckSonarCamera_Exit(GameObject target)
    {
        if (!sonarInside) return;
        if (!target.CompareTag("SonarCamera")) return;

        Debug.Log("CheckSonarCamera_Exit");
        OnSonarOutside();
    }


    void OnHit()
    {
        // 충돌한 순간 탐지기를 볼 수 없게 된다.
        Debug.Log(gameObject.transform.parent.gameObject.name + " -> OnHit");
        sonarHit = false;
        Enable();
    }

    void OnActiveSonar()
    {
        // 탐지기로 보는 것을 허락한다.
        Debug.Log(gameObject.transform.parent.gameObject.name + " -> ActiveSonar");
        sonarHit = true;
        Enable();
    }

    void OnSonarInside()
    {
        // 탐지기 표시 영역의 내부.
        Debug.Log(gameObject.transform.parent.gameObject.name + " -> SonarInside");
        sonarInside = true;
        Enable();
    }

    void OnSonarOutside()
    {
        // 탐지기 표시 영역의 외부.
        Debug.Log(gameObject.transform.parent.gameObject.name + " -> SonarOutside");
        sonarInside = false;
        Enable();
    }

    private void Enable()
    {
        Debug.Log(gameObject.transform.parent.gameObject.name + ": sonarInside=" + sonarInside + ", sonarHit=" + sonarHit);
        bool result = (sonarInside && sonarHit)?true:false;
        renderer.enabled = result;
        if (result)
        {
            wait = false;
            currentTime = 0.0f;
        }
    }

    private IEnumerator Delay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        wait = false;
        currentTime = 0.0f;
    }

    public bool SonarInside() {
        return sonarInside;
    }
}
