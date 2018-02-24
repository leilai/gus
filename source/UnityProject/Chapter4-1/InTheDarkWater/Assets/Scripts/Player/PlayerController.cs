using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {


    [System.Serializable]
    public class SpeedValue
    {
        [SerializeField]
        private float max = 10.0f;
        [SerializeField]
        private float step = 0.5f;

        public float current = 1.0f;
        /// <summary>
        /// 스피드 변경
        /// </summary>
        /// <param name="value"></param>
        public void Change( float value )
        {
            current += value * step;
            current = Mathf.Clamp(current, 0.0f, max);
        }

        public void Stop() { current = 0.0f; }

        public float Rate() { return Mathf.InverseLerp(0.0f, max, current); }
    };
    [SerializeField]
    private SpeedValue speed = new SpeedValue();

    [System.Serializable]
    public class RotationValue
    {
        public Vector3 current = Vector3.zero;
        private float attenuationStart;
        private float attenuationTime = 0.0f;
        private float currentRot;               // 현재의 감소율（attenuationRot/slowdownRot)

        [SerializeField]
        private float max = 30.0f;
        [SerializeField]
        private float blend = 0.8f;
        [SerializeField]
        private float margin = 0.01f;
        [SerializeField]
        private float attenuationRot = 0.2f;    // 『버튼을 누르고 있지 않은』경우의 감소율
        [SerializeField]
        private float slowdownRot = 0.4f;       // 『버튼을 누르고 있지만, 마우스는 움직이지 않는』 경우의 감소율

        public void Init()
        {
            currentRot = attenuationTime;
        }

        /// <summary>
        /// 회전량 변경
        /// </summary>
        public void Change(float value)
        {
            // 마우스의 작동이 적을 때에는 갱신하지 않는다.
            // （마우스를 계속해서 움직이지 않으면 선회를 멈추게 되기 때문에）
            if (-margin < value && value < margin) return;

            // 회전량 BLEND
            current.y = Mathf.Lerp(current.y, current.y + value, blend);
            if (current.y > max) current.y = max;
            // 감소 재설정
            attenuationStart = current.y;
            attenuationTime = 0.0f;
        }
        /// <summary>
        /// 감소
        /// </summary>
        /// <param name="time">시간변위</param>
        /// <returns>감소중/ 감소하지 않음</returns>
        public bool Attenuate(float time)
        {
            if (current.y == 0.0f) return false;
            attenuationTime += time;
            current.y = Mathf.SmoothStep(attenuationStart, 0.0f, currentRot * attenuationTime);
            return true;
        }

        public void BrakeAttenuation() {    currentRot = slowdownRot;   }
        public void UsualAttenuation()
        {
            attenuationTime = (slowdownRot * attenuationTime) / attenuationRot;
            currentRot = attenuationRot;
        }

        public void Stop() { current = Vector3.zero; }
    };
    [SerializeField]
    private RotationValue rot = new RotationValue();

    private Quaternion deltaRot;

    [SerializeField]
    private bool valid = false;

    private Controller controller;
    private MarineSnow marinesnowEffect;
    private TorpedoGenerator torpedo;

	void Start () 
    {
        // MarinSnow 효과는 스피드 존재. 자주 갱신하기 때문에 참조를 가져온다.
        GameObject effect = GameObject.Find("Effect_MarineSnow");
        if (effect)  marinesnowEffect = effect.GetComponent<MarineSnow>();
        // 어뢰 발사 스크립트
        torpedo = GetComponent<TorpedoGenerator>();
    }

    void OnGameStart()
    {
        Debug.Log("OnGameStart");
        valid = true;
        // UI 컨트롤러. 자주 갱신하기 때문에 참조를 가져온다.
        GameObject uiObj = GameObject.Find("/UI/Controller");
        if (uiObj) controller = uiObj.GetComponent<Controller>();
        // 컨트롤러 표시
        if (controller) controller.Enable(true);
        rot.Init();
    }

    void OnGameClear()
    {
        InvalidPlayer();
    }

    void OnGameOver()
    {
        InvalidPlayer();

        // 침몰 연출
        // 축 고정을 해지하고 중력을 유효화한다.
        rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rigidbody.useGravity = true;
    }
	
	void FixedUpdate () 
    {
        // 회전 감소
        rot.Attenuate(Time.deltaTime);

        if (valid)
        {
            // 어뢰발사
            if (Input.GetKeyDown(KeyCode.B))
            {
                //Debug.Log("B ender : " + Time.time);
                torpedo.Generate();
            }

            // 드래그중
            if (Input.GetMouseButton(0))
            {
//                Debug.Log("MouseButton");
                // 회전
                rot.Change(Input.GetAxis("Mouse X"));
                // 가속
                speed.Change(Input.GetAxis("Mouse Y"));
            }

            // 드래그 시작
            if (Input.GetMouseButtonDown(0))
            {
//                Debug.Log("MouseDown");
                rot.BrakeAttenuation();
            }
            // 드래그 종료
            if (Input.GetMouseButtonUp(0))
            {
                rot.UsualAttenuation();
            }
        }
        // 회전한다.
        Rotate();
        // 앞으로 이동한다.
        MoveForward();
	}

    private void InvalidPlayer()
    {
        valid = false;
        speed.Stop();
        rot.Stop();
        if (controller) controller.Enable(false);
    }

    private void Rotate() 
    {
        Quaternion deltaRot = Quaternion.Euler(rot.current * Time.deltaTime);
        rigidbody.MoveRotation(rigidbody.rotation * deltaRot);
        // 회전연출
        if (controller) controller.SetAngle(transform.localEulerAngles.y);
    }

    private void MoveForward()
    {
        Vector3 vec = speed.current * transform.forward.normalized;
        rigidbody.MovePosition(rigidbody.position + vec * Time.deltaTime);
        // 스피드 변화 연출
        if (marinesnowEffect) marinesnowEffect.SetSpeed(speed.Rate());
    }

    public void AddSpeed(float value) 
    {
        speed.Change( value );
    }

    public float SpeedRate()
    {
        return speed.Rate();
    }

}
