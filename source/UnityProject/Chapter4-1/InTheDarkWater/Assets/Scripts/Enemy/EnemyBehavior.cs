using UnityEngine;
using System.Collections;

/// <summary>
/// 적의 동작.
/// </summary>
public class EnemyBehavior : MonoBehaviour
{
    /// <summary>
    /// 스피드 조정.
    /// </summary>
    [System.Serializable]
    public class SpeedValue
    {
        [SerializeField]
        private float usualMax = 5.0f;
        [SerializeField]
        private float emergencyMax = 0.0f;
        [SerializeField]
        private float backwardMax = 10.0f;
        [SerializeField]
        private float current = 1.0f;
        private float max;
        public float Value
        {
            set
            {
                if (max < value) current = max;
                else current = value;
            }
            get { return current; }
        }

        public void Usual() { max = usualMax; }
        public void Emergency(){   max = emergencyMax; }

        public void Stop() { current = 0.0f; }

        /// <summary>
        /// 속도 변경
        /// </summary>
        public void Change()
        {
            current += Random.Range(-max, max);
            current = Mathf.Clamp(current, 0.0f, max);
        }
        public void GoBackward()
        {
            current = -backwardMax;
        }
        public void GoFroward()
        {
            current = 0.0f;
            Change();
        }
    };
    [SerializeField]
    private SpeedValue speed = new SpeedValue();

    /// <summary>
    /// 회전 조정
    /// </summary>
    [System.Serializable]
    public class RotationValue
    {
        [SerializeField]
        private float usualMax = 20.0f;
        [SerializeField]
        private float emergencyMax = 10.0f;
        [SerializeField]
        private float swingStep = 20.0f;
        [SerializeField]
        private float blending = 0.8f;
        [SerializeField]
        private float attenuation = 0.2f;

        private Vector3 current = Vector3.zero;
        private float max;
        private float attenuationStart;
        private float attenuationTime;

        public Vector3 Value
        {
            set
            {
                if (value.y < -max) current.y = -max;
                if (value.y > max) current.y = max;
                else current = value;
            }
            get { return current; }
        }

        public void Usual() { max = usualMax; }
        public void Emergency() { max = emergencyMax; }

        public void Stop() { current = Vector3.zero; }

        public void Swing( float value )
        {
            if (value < -swingStep) value = -swingStep;
            if (value > swingStep) value = swingStep;
            current.y = value;
        }

        /// <summary>
        /// 회전양 변경
        /// </summary>
        public void Change()
        {
            float value = Random.Range(-max, max);
            // 회전양 블랜드.
            current.y = Mathf.Lerp(current.y, current.y + value, blending);
            // 감소 재설정
            attenuationStart = current.y;
            attenuationTime = 0.0f;
        }

        /// <summary>
        /// 감소
        /// </summary>
        /// <param name="time">시간 변이</param>
        /// <returns>감소중/감소하지 않음</returns>
        public bool Attenuate(float time)
        {
            if (current.y == 0.0f) return false;

            attenuationTime += time;
            current.y = Mathf.SmoothStep(attenuationStart, 0.0f, attenuation * attenuationTime);
            return true;
        }
    };
    [SerializeField]
    private RotationValue rot = new RotationValue();

    [SerializeField]
    private Rect runningArea;   // 이동 범위
    [SerializeField]
    private float waitTime = 10.0f;

    [SerializeField]
    private float attackWait = 5.0f;
    [SerializeField]
    private float attackDistance = 100.0f;
        
    enum Mode
    {
        Usual,
        Caution,
        Emergency
    };
    private Mode mode = Mode.Usual;

    private float currentTime;
    private bool valid;

    private bool autoAttack = false;
    private float currentAttackTime;
    private TorpedoGenerator torpedo = null;
    private GameObject player = null;


    void Start()
    {
        // 시작시에는 외부에 있기 때문에 중심을 향하게 해 둔다.
        transform.LookAt(Vector3.zero);

        player = GameObject.Find("/Field/Player");
        torpedo = GetComponent<TorpedoGenerator>();
        currentTime = 0.0f;
        valid = true;
        speed.Usual();
        rot.Usual();
    }

    void Update()
    {
        // 회전 감소
        if (! rot.Attenuate(Time.deltaTime))
        {
            if (valid)
            {
                // 감소 종료후, 카운트하여 다시 이동.
                currentTime += Time.deltaTime;
                if (currentTime > waitTime) AutoController();
            }
        }
        // 회전한다.
        Rotate();
        // 앞으로 이동.
        MoveForward();
    }

    /// <summary>
    /// 게임 오버시.
    /// </summary>
    void OnGameOver()
    {
        speed.Stop();
        rot.Stop();
        valid = false;
    }
    /// <summary>
    /// 공격이 충돌한 경우
    /// </summary>
    void OnHit()
    {
        //Debug.Log("EnemyBehaviour.OnHit:" + name);
        // 무효화
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider) collider.enabled = false;
        // 만일에 대비하여
        StopAllCoroutines();
    }
    /// <summary>
    /// Note에서 삭제 허가를 받는다.
    /// </summary>
    void OnDestroyLicense()
    {
        //Debug.Log("EnemyBehaviour.OnDestroyObject:" + name);
        // 부모에게 전달한다. 부모가 삭제한다.
        transform.parent.gameObject.SendMessage("OnDestroyObject", gameObject, SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Player발견, 공격상태.
    /// </summary>
    void OnEmergency()
    {
        Debug.Log("OnEmergency");
        mode = Mode.Emergency;
        rot.Emergency();
        speed.Emergency();

        if (!autoAttack)
        {
            autoAttack = true;
            Debug.Log("AutoAttack");
            StartCoroutine("AutoAttack");
        }
    }

    /// <summary>
    /// 경계 상태
    /// </summary>
    void OnCaution()
    {
        Debug.Log("OnCaution");
        mode = Mode.Caution;
        rot.Emergency();
        speed.Emergency();
        if (autoAttack)
        {
            autoAttack = false;
            StopCoroutine("AutoAttack");
        }
    }

    /// <summary>
    /// 보통 상태.
    /// </summary>
    void OnUsual()
    {
        Debug.Log("OnUsual");
        mode = Mode.Usual;
        rot.Usual();
        speed.Usual();
        if(autoAttack) 
        {
            autoAttack = false;
            StopCoroutine("AutoAttack");
        }
    }

    private IEnumerator AutoAttack()
    {
        yield return new WaitForSeconds(attackWait);

        transform.LookAt(player.transform);
        torpedo.Generate();

        StartCoroutine("AutoAttack");
    }


    /// <summary>
    /// 이동 자동 갱신
    /// </summary>
    private void AutoController()
    {
        currentTime = 0.0f;
        rot.Change();
        speed.Change();
    }

    /// <summary>
    /// 회전 갱신
    /// </summary>
    private void Rotate()
    {
        if (!runningArea.Contains(new Vector2(transform.position.x, transform.position.z)))
        {
            // 이동 위치 밖이라면 선회.
            Vector3 aimVec = -transform.position.normalized;
            float angle = Vector3.Angle(transform.forward, aimVec);
            Debug.Log("angle=" + angle + ": (" + transform.position.x + "," +  transform.position.z + ")");
            if (!Mathf.Approximately(angle, 0.0f)) rot.Swing(-angle);
        }

        if (mode == Mode.Emergency)
        {
            // 플레이어 쪽을 향한다.
//            Vector3 aimVec = player.transform.position - transform.position;
//            float angle = Vector3.Angle(transform.forward, aimVec);
//            Debug.Log("angle=" + angle + ": (" + transform.position.x + "," + transform.position.z + ")");
//            if (!Mathf.Approximately(angle, 0.0f)) rot.Swing(-angle);
        }

        Quaternion deltaRot = Quaternion.Euler(rot.Value * Time.deltaTime);
        rigidbody.MoveRotation(rigidbody.rotation * deltaRot);
    }

    /// <summary>
    /// 이동 갱신
    /// </summary>
    private void MoveForward()
    {
        CheckPlayer();

        Vector3 deltaVec = speed.Value * transform.forward.normalized;
        rigidbody.MovePosition(rigidbody.position + deltaVec * Time.deltaTime);
    }

    private void CheckPlayer()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        switch (mode)
        {
            case Mode.Emergency:
                if (dist <= attackDistance) speed.GoBackward();
                else speed.GoFroward();
                break;
            case Mode.Usual:
                // 플레이어에게 발각
                if (dist <= attackDistance)
                {
                    SendMessage("OnEmergency");
                }
                break;
            default: break;
        }
    }
}
