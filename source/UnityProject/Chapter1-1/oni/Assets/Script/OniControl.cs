using UnityEngine;
using System.Collections;

public class OniControl : MonoBehaviour {

	// 플레이어.
	public PlayerControl		player = null;

	// 카메라.
	public GameObject	main_camera = null;

	// 콜리전 박스의 크기（１변의 길이）.
	public static float collision_size = 0.5f;

	// 아직 살아있는가？.
	private bool	is_alive = true;

	// 생성되는 때의 위치.
	private Vector3	initial_position;

	// 좌우로 울퉁불퉁하는 경우, 울퉁불퉁하는 정도의 주기.
	public float	wave_angle_offset = 0.0f;

    // 좌우로 울퉁불퉁하는 경우, 울퉁불퉁하는 정도의 폭.
	public float	wave_amplitude = 0.0f;

	// 도깨비의 상태.
	enum STEP {

		NONE = -1,

		RUN = 0,			// 走って逃げてる.
		DEFEATED,			// 斬られて吹き飛び中.

		NUM,
	};

	// 현재 상태.
	private	STEP		step      = STEP.NONE;

	// 다음으로 전환되는 상태.
	private	STEP		next_step = STEP.NONE;

	// [sec]상태가 전환되고 부터 걸리는 시간.
	private float		step_time = 0.0f;

	// DEFEATED, FLY_TO_STACK 시작 시점의 속도 벡터.
	private Vector3		blowout_vector = Vector3.zero;
	private Vector3		blowout_angular_velocity = Vector3.zero;

	// -------------------------------------------------------------------------------- //

	void 	Start()
	{
		// 생성될 때의 위치.
		this.initial_position = this.transform.position;

		this.transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up);

		this.GetComponent<Collider>().enabled = false;

		// 회전속도의 제어를 제어하지 않고 그대로 둔다.
		this.GetComponent<Rigidbody>().maxAngularVelocity = float.PositiveInfinity;

		// 모델의 센터가 약간 아래에 있기 때문에 중심을 조금 벗어나도록 한다.
		this.GetComponent<Rigidbody>().centerOfMass = new Vector3(0.0f, 0.5f, 0.0f);

	}
	void	Update()
	{
		this.step_time += Time.deltaTime;

		// 상태 변화 체크.
		// （현재 외부에서 받는 영향 이외에는 변화되지 않는다.)

		switch(this.step) {

			case STEP.NONE:
			{
				this.next_step = STEP.RUN;
			}
			break;
		}

		// 초기화.
		// 상태가 변화한 경우 초기화 처리.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.DEFEATED:
				{
					this.GetComponent<Rigidbody>().velocity = this.blowout_vector;

					// 회전의 각속도.
					this.GetComponent<Rigidbody>().angularVelocity = this.blowout_angular_velocity;

					// 부모 자식 관계를 따르지 않는다.
					// 부모（OniGroup）가 삭제되면 같이 삭제되게 되므로
					this.transform.parent = null;
			
					// 카메라의 좌표계 내에서 작동하도록 한다.
					// （카메라의 작동과 연동하도록 된다.）.
					if(SceneControl.IS_ONI_BLOWOUT_CAMERA_LOCAL) {
			
						this.transform.parent = this.main_camera.transform;
					}

                    // oni_yarare 모션을 재생한다.
					
					this.transform.GetChild(0).GetComponent<Animation>().Play("oni_yarare");

					this.is_alive = false;
				}
				break;
			}

			this.step = this.next_step;

			this.next_step = STEP.NONE;

			this.step_time = 0.0f;
		}

		// 각 상태에서의 실행 처리.

		Vector3	new_position = this.transform.position;

		float low_limit = this.initial_position.y;

		switch(this.step) {

			case STEP.RUN:
			{
				// 살아있는 동안에는 지면 아래로 떨어지지 않도록한다.

				if(new_position.y < low_limit) {
		
					new_position.y = low_limit;
				}
	
				// 좌우로 울통불퉁하다.
	
				float	wave_angle = 2.0f*Mathf.PI*Mathf.Repeat(this.step_time, 1.0f) + this.wave_angle_offset;
	
				float	wave_offset = this.wave_amplitude*Mathf.Sin(wave_angle);
	
				new_position.z = this.initial_position.z + wave_offset;
	
				// 방향（Y축 회전）.
				if(this.wave_amplitude > 0.0f) {
	
					this.transform.rotation = Quaternion.AngleAxis(180.0f - 30.0f*Mathf.Sin(wave_angle + 90.0f), Vector3.up);
				}

			}
			break;

			case STEP.DEFEATED:
			{
				// 죽은 직후에 지면으로 사라지게 되는 경우가 존재하므로 속도가 향상
				// （＝죽은 직후）의 경우에는 지면 아래로 떨어지지 않도록 한다.
				if(new_position.y < low_limit) {
	
					if(this.GetComponent<Rigidbody>().velocity.y > 0.0f) {
	
						new_position.y = low_limit;
					}
				}
	
				// 약간 후방에 희미하게 보이도록 표시하고 싶다.
                if(this.transform.parent != null) {
	
					this.GetComponent<Rigidbody>().velocity += -3.0f*Vector3.right*Time.deltaTime;
				}
			}
			break;

		}

		this.transform.position = new_position;

		// 불필요하게 되는 경우 삭제한다.
		//
		// ・화면 밖에 표시되는 경우
		// ・죽은 경우
        // ・SE의 재생이 정지
		//
		// OnBecameInvisible() 는 화면 밖으로 나오는 순간에만 불러올 수 있기 때문에
		// 『화면 밖에서 잠시동안 소리가 울린 후』삭제하고자 하는 경우에는 사용할 수 없다.
		//

		do {

			// 화면 밖에서 도깨비(도깨비 그룹)을 발생시키기 때문에 발생한 순간에도
			// 불러오게 된다. 때문에 this.is_alive 를 점검하여 사망상태에서
			// 화면 밖으로 불러오는 경우에 한해, 삭제하도록 한다.
			if(this.GetComponent<Renderer>().isVisible) {

				break;
			}

			if(this.is_alive) {

				break;
			}

			// SE 를 재생하고 있는 동안은 삭제하지 않는다.
			if(this.GetComponent<AudioSource>().isPlaying) {

				if(this.GetComponent<AudioSource>().time < this.GetComponent<AudioSource>().clip.length) {

					break;
				}
			}

			//

			Destroy(this.gameObject);

		} while(false);
	}

	// 모션 재생 스피드를 설정한다.
	public void setMotionSpeed(float speed)
	{
		this.transform.GetChild(0).GetComponent<Animation>()["oni_run1"].speed = speed;
		this.transform.GetChild(0).GetComponent<Animation>()["oni_run2"].speed = speed;
	}

	// 공격을 받을 때의 처리를 시작한다.
	public void AttackedFromPlayer(Vector3 blowout, Vector3	angular_velocity)
	{
		this.blowout_vector           = blowout;
		this.blowout_angular_velocity = angular_velocity;

		// 부모 자식 관계를 따르지 않는다.
        // 부모（OniGroup）가 삭제되면 같이 삭제되기 때문에.
		this.transform.parent = null;

		this.next_step = STEP.DEFEATED;
	}

}
