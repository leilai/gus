using UnityEngine;
using System.Collections;

/// <summary>
///적에 따라Caution값 설정.
/// </summary>
public class EnemyCaution : MonoBehaviour {

    [SerializeField]
    private float waitForce = 0.02f;

    [SerializeField]
    private int step = 1;

    [SerializeField]   // Debug열람용
    private EnemyParameter param = null;
    
    [SerializeField]    // Debug열람용
    private int cautionValue = 0;

    private int currentStep = 1;

    [SerializeField]    // Debug열람용
    private float waitTime = 1.0f;

    private float count = 0.0f;

    private bool counting = false;
    private bool emergency = false;
    private bool countup = true;
    private CautionUpdater updater = null;
    private PlayerController controller = null;

	void Start () 
    {
        GameObject enemy = GameObject.Find("/Field/Enemies");
        if (enemy) updater = enemy.GetComponent<CautionUpdater>();
        GameObject player = GameObject.Find("/Field/Player");
        if (player) controller = player.GetComponent<PlayerController>();
	}

    void Update()
    {
        if (param == null || !counting) return;

        count += Time.deltaTime;
        if (count >= waitTime) {
            count = 0.0f;
            counting = UpdateCaution();
        }
    }

    void OnStayPlayer( float distRate )
    {
        if (param==null) return;

        if (countup)
        {
            // Player와의 거리가 가까울수록 Caution값은 상승한다.
            if (!emergency)
            {
                waitTime = Mathf.Lerp(param.cautionWaitMin, param.cautionWaitMax, distRate);
                // Player의 속도가 느릴수록 Caution값이 떨어진다.
                float speedRate = controller.SpeedRate();
                // 보통은 waitTime을 Lerp시킨다.
                float sneakingRate = (1.0f - speedRate) * param.sneaking;
                waitTime = Mathf.Lerp(waitTime, param.cautionWaitLimit, sneakingRate);
            }
        }
        else 
        {
            // 카운트다운하는 경우에는 카운트업으로 변경
            StartCount(true);
        }
    }

      // 멀어지는 경우에 무엇인가를 추가하려며 이것
    void OnExitPlayer()
    {
        if (param == null || counting) return;
        // 카운트 하지 않고 멀어진다.
        waitTime = waitForce;
        emergency = true;
        StartCount(false);
    }

    void OnStartCautionCount(EnemyParameter param_)
    {
        // 파라미터 설정과 카운트 시작.
        Debug.Log("OnStartCautionCount");
        param = param_;
        waitTime = param.cautionWaitMax;
        StartCount(true);
    }

    void OnAddScore()
    {
        if (param == null) return;

        // 스코어 값을 보낸다.
        GameObject ui = GameObject.Find("/UI");
        if (ui)
        {
            // 오브젝트 충돌
            ui.BroadcastMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);
            // 득점추가
            float time = 1.0f - Mathf.InverseLerp(0, 100, cautionValue);
            int scoreValue = (int)Mathf.Lerp(param.scoreMin, param.scoreMax, time);
            ui.BroadcastMessage("OnAddScore", scoreValue);
        }
        GameObject parent = gameObject.transform.parent.gameObject;
        if (parent)
        {
            // 부모에게도 충돌 알림
            parent.SendMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);
        }
        // 스스로 충돌 판정
        BroadcastMessage("OnHit");
    }

    void OnActiveSonar()
    {
        if (param == null) return;
        Debug.Log("EnemyCaution.OnActiveSonar");
        // 탐지기가 충돌할 때마다 Caution 상승
        cautionValue = Mathf.Clamp(cautionValue + param.sonarHitAddCaution, 0, 100);
        updater.DisplayValue(gameObject, cautionValue);
    }

    private void StartCount(bool countup_)
    {
        count = 0;
        counting = true;
        countup = countup_;
        currentStep = (countup) ? step : (-step);
    }

    private bool UpdateCaution()
    {
        cautionValue = Mathf.Clamp(cautionValue + currentStep, 0, 100);
        // 표시 갱신
        updater.DisplayValue(gameObject, cautionValue);
        // 조건 체크
        if (cautionValue >= 100)
        {
            // Player를 발견
            SendMessage("OnEmergency");
            return false;
        }
        else if (cautionValue <= 0)
        {
            // Player를 잃는다.
            SendMessage("OnUsual");
            emergency = false;
            StartCount(true);   // 카운트 수정
        }

        return true;
    }

    void OnEmergency()
    {
        emergency = true;
        if (cautionValue < 100)
        {
            // 아직 cuation값이 차지 않은 경우.
            waitTime = waitForce;
        }
    }

    public int Value(){ return cautionValue; }
}
