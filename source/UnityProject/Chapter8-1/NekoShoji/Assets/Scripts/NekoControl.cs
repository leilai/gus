using UnityEngine;
using System.Collections;

public class NekoControl : MonoBehaviour {

	private RoomControl		room_control = null;
	private SceneControl	scene_control = null;
	public EffectControl	effect_control = null;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		STAND = 0,			// 서기
		RUN,				// 달리기
		JUMP,				// 점프
		MISS,				// 실패
		GAMEOVER,			// 게임오버

		FREE_MOVE,			// 자유이동(디버그용)

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;
	public bool			is_grounded;				// 착지 중인가?

	// ---------------------------------------------------------------- //

	// 점프 중의 다양한 처리.
	public struct ActionStand {

		public bool		is_fade_anim;				// 애니메이션을 페이드하는가?(매 프레임 true 으로 돌아간다.)
	};

    // 점프 중의 다양한 처리.
	public struct ActionJump {

		public STEP		prevoius_step;				// 점프 하기 전의 스텝(서서 점프 or 달리면서 점프).

		public bool		is_key_released;			// 점프후 스페이스키를 누르지 않으면?

		public Vector3	launch_velocity_xz;
	};

	// 실패했을 때의 다양한 처리
	public struct ActionMiss {

		public bool		is_steel;					// 철판에 부딪혔는가?
	};

	public ActionJump	action_jump;
	public ActionMiss	action_miss;
	public ActionStand	action_stand;

	public Vector3		previous_velocity;

	private	bool		is_fallover = true;
		
	private	bool		is_auto_drive = false;		// 자동 동작(클리어한 후)

	// ---------------------------------------------------------------- //

	public static float	JUMP_HEIGHT_MAX = 5.0f;								// 점프 높이
	public static float	JUMP_KEY_RELEASE_REDUCE = 0.5f;						// 점프 중에 키를 누르지 않을 때의 상승 속도의 스케일.

	public static float	RUN_SPEED_MAX   = 5.0f;								// 달리기 스피드의 최대값.
	public static float	RUN_ACCELE      = RUN_SPEED_MAX/2.0f;				// 달리기 스피드의 가속.

	public static float	SLIDE_SPEED_MAX = 2.0f;								// 좌우이동 스피드.
	public static float	SLIDE_ACCEL     = SLIDE_SPEED_MAX/0.1f;				// 좌우이동의 가속도

	public static float SLIDE_ACCEL_SCALE_JUMP = 0.1f;						// 좌우이동 가속도의 스케일(점프중)              

	public static float	RUN_SPEED_DECELE_MISS      = RUN_SPEED_MAX/2.0f;	// 실패한 경우의 감속도
	public static float	RUN_SPEED_DECELE_MISS_JUMP = RUN_SPEED_MAX/5.0f;	// 실패한 경우의 감속도(점프중)

	public static Vector3 COLLISION_OFFSET = Vector3.up*0.2f;

	// ---------------------------------------------------------------- //

	public static float SLIDE_ROTATION_MAX = 0.2f;							// 좌우이동의 로테이션 스피드.
    public static float SLIDE_ROTATION_SPEED = SLIDE_ROTATION_MAX / 0.1f;  // 좌우이동의 로테이션 가속도.          
    public static float SLIDE_ROTATION_COEFFICIENT = 2.0f;					// 좌우이동의 로테이션 가속도의 계수

	public static float JUMP_ROTATION_MAX = 0.25f;							// 상하의 로테이션 스피드(점프중) 
	public static float JUMP_ROTATION_SPEED = JUMP_ROTATION_MAX/0.1f;		// 상하의 로테이션 가속도(점프중)
	public static float JUMP_ROTATION_COEFFICIENT = 0.25f;					// 상하의 로테이션 가속도의 계수(점프중)

    public static float SLIDE_VELOCITY = 1.0f;								// 좌우이동의 로테이션 속도            
	public static float JUMP_VELOCITY = 4.0f;								// 상하의 로테이션 속도(점프중)
	
	// ---------------------------------------------------------------- //

	public AudioClip START_SOUND = null;
	public AudioClip FAILED_STEEL_SOUND = null;
	public AudioClip FAILED_FUSUMA_SOUND = null;
	public AudioClip FAILED_NEKO_SOUND = null;
	public AudioClip JUMP_SOUND = null;
	public AudioClip LANDING_SOUND = null;
	public AudioClip FALL_OVER_SOUND = null;

	// ---------------------------------------------------------------- //

	NekoColiResult	coli_result;

	// ---------------------------------------------------------------- //

	public void	onRoomProceed()
	{
		this.coli_result.shoji_hit_info_first.is_enable = false;
	}

	// Use this for initialization
	void Start ()
	{
		this.room_control   = GameObject.FindGameObjectWithTag("RoomControl").GetComponent<RoomControl>();
		this.scene_control  = GameObject.FindWithTag("MainCamera").GetComponent<SceneControl>();
		this.effect_control = GameObject.FindGameObjectWithTag("EffectControl").GetComponent<EffectControl>();


		//

		this.is_grounded = false;

		audio.clip = START_SOUND;
		audio.Play();

		this.previous_velocity = Vector3.zero;

		this.next_step = STEP.STAND;

		this.coli_result = new NekoColiResult();
		this.coli_result.neko = this;
		this.coli_result.create();
	
		this.action_stand.is_fade_anim = true;	

	}

	// Update is called once per frame
	void Update ()
	{

		Animation	animation = this.GetComponentInChildren<Animation>();

		// ---------------------------------------------------------------- //

		// 착지할 때에 지면에 박히기 때문에
		// （보기 좋지 않으므로）.

		if(this.transform.position.y < 0.0f) {

			this.is_grounded = true;

			Vector3	pos = this.transform.position;

			pos.y = 0.0f;

			this.transform.position = pos;
		}
		
		// ---------------------------------------------------------------- //
		// 스텝 내의 경과 시간을 진행한다.

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지를 체크한다.       

		// 앞의 프레임 콜리전 결과를 점검한다.

		if(this.step != STEP.MISS) {

			this.coli_result.resolveCollision();
		}

		//

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.NONE:
				{
					this.next_step = STEP.STAND;
				}
				break;
	
				case STEP.STAND:
				{
					// 시프트 키로 달리기시작한다.
					if(Input.GetKeyDown(KeyCode.LeftShift)) {
	
						this.next_step = STEP.RUN;
					}
					// 스페이스 키로 점프
					if(Input.GetKeyDown(KeyCode.Space)) {
	
						this.next_step = STEP.JUMP;
					}
				}
				break;
	
				case STEP.RUN:
				{
					if(!this.is_auto_drive) {

						if(Input.GetKeyDown(KeyCode.Space)) {
		
							this.next_step = STEP.JUMP;
						}
					}
				}
				break;

				case STEP.JUMP:
				{
					// 착지하여 서기 또는 달리기로.
					if(this.is_grounded) {
					
						audio.clip = LANDING_SOUND;
						audio.Play();
						this.next_step = this.action_jump.prevoius_step;
					}
				}
				break;

				case STEP.MISS:
				{
					if(this.step_timer > 3.0f) {
					
						GameObject.FindWithTag("MainCamera").transform.SendMessage("applyDamage", 1);

						if(this.scene_control.getLifeCount() > 0) {

							this.transform.position = this.room_control.getRestartPosition();

							this.room_control.onRestart();

							// 애니메이션은 수정하지 않는다.
							this.action_stand.is_fade_anim = false;
						
							this.next_step = STEP.STAND;

						} else {

							this.next_step = STEP.GAMEOVER;
						}
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환될 때의 초기화

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.STAND:
				{
					Vector3 v = this.rigidbody.velocity;

					v.x = 0.0f;
					v.z = 0.0f;

					this.rigidbody.velocity = v;

					// 서기 애니메이션 재생

					if(this.action_stand.is_fade_anim) {

						animation.CrossFade("M01_nekostanding", 0.2f);

					} else {

						animation.CrossFade("M01_nekostanding", 0.0f);
					}

					this.action_stand.is_fade_anim = true;	
				}
				break;

				case STEP.RUN:
				{
					animation.CrossFade("M02_nekodash", 0.2f);
				}
				break;

				case STEP.JUMP:
				{
					Vector3	v = this.rigidbody.velocity;

					v.y = Mathf.Sqrt(2.0f*9.8f*JUMP_HEIGHT_MAX);

					this.rigidbody.velocity = v;

					//

					this.action_jump.is_key_released = false;
					this.action_jump.prevoius_step   = this.step;

					this.action_jump.launch_velocity_xz = this.rigidbody.velocity;
					this.action_jump.launch_velocity_xz.y = 0.0f;

					//

					animation.CrossFade("M03_nekojump", 0.2f);
					audio.clip = JUMP_SOUND;
					audio.Play();
				}
				break;

				case STEP.MISS:
				{				
					// 뒤로 뛰어오른다.

					Vector3 v = this.rigidbody.velocity;

					v.z *= -0.5f;

					this.rigidbody.velocity = v;
						
					// 효과   
					this.effect_control.createMissEffect(this);

					// 철판에 부딪히는 소리 or 장지문에 부딪히는 소리.
					//
					if(this.action_miss.is_steel) {

						audio.PlayOneShot(FAILED_STEEL_SOUND);

					} else {

						audio.PlayOneShot(FAILED_FUSUMA_SOUND);
					}

					// 소리 재생
					//
					audio.PlayOneShot(FAILED_NEKO_SOUND);
					
					animation.CrossFade("M03_nekofailed01", 0.2f);

					this.coli_result.lock_target.enable = false;

					this.is_fallover = false;
				}
				break;

				case STEP.FREE_MOVE:
				{
					this.rigidbody.useGravity = false;

					this.rigidbody.velocity = Vector3.zero;
				}
				break;

			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리

		// 좌우이동, 점프에 따른 로테이션
		this.rotation_control();

		switch(this.step) {

			case STEP.STAND:
			{
			}
			break;

			case STEP.RUN:
			{
				// 앞으로 가속

				Vector3	v = this.rigidbody.velocity;

				v.z += (RUN_ACCELE)*Time.deltaTime;

				v.z = Mathf.Clamp(v.z, 0.0f, RUN_SPEED_MAX);

				// 좌우로 평행이동

				if(this.is_auto_drive) {

					v = this.side_move_auto_drive(v, 1.0f);

				} else {

					v = this.side_move(v, 1.0f);
				}

				//

				this.rigidbody.velocity = v;
			}
			break;

			case STEP.JUMP:
			{
				Vector3 v = this.rigidbody.velocity;

				// 점프중에 키를 누르지 않으면, 상승속도를 줄인다.
				// （키를 누르는 길이로 점프의 높이를 제어할 수 있도록).

				do {

					if(!Input.GetKeyUp(KeyCode.Space)) {
					
						break;
					}

					// 한 번 버튼을 뗀 후에는 동작이 연속되지 않는다. (연속 타자 대책).
					if(this.action_jump.is_key_released) {

						break;
					}

					// 하강 중에는 점프할 수 없다.
					if(this.rigidbody.velocity.y <= 0.0f) {

						break;
					}

					//

					v.y *= JUMP_KEY_RELEASE_REDUCE;

					this.rigidbody.velocity = v;

					this.action_jump.is_key_released = true;

				} while(false);

				// 좌우로 평행이동.
				// （점프 중에도 약간 제어할 수 있도록 하기 위해)
				//
				if(this.is_auto_drive) {

					this.rigidbody.velocity = this.side_move_auto_drive(this.rigidbody.velocity, SLIDE_ACCEL_SCALE_JUMP);

				} else {

					this.rigidbody.velocity = this.side_move(this.rigidbody.velocity, SLIDE_ACCEL_SCALE_JUMP);
				}

				//

				// 격자에 부딪힌 경우에는 구멍의 중심으로 유도한다.
				if(this.coli_result.shoji_hit_info.is_enable) {
	
					//
	
					v = this.rigidbody.velocity;
			
					if(this.coli_result.lock_target.enable) {

						v = this.coli_result.lock_target.position  - this.transform.position;
					}

					v.z = this.action_jump.launch_velocity_xz.z;
						
					this.rigidbody.velocity = v;
				}
			}
			break;


			case STEP.MISS:
			{
				GameObject.FindWithTag("MainCamera").transform.SendMessage("nekoFailed");

				// 서서히 감속한다.

				Vector3 v = this.rigidbody.velocity;

				v.y = 0.0f;

				float	speed_xz = v.magnitude;

				if(this.is_grounded) {	

					speed_xz -= RUN_SPEED_DECELE_MISS*Time.deltaTime;

				} else {

					speed_xz -= RUN_SPEED_DECELE_MISS_JUMP*Time.deltaTime;
				}

				speed_xz = Mathf.Max(0.0f, speed_xz);

				v.Normalize();

				v *= speed_xz;

				v.y = this.rigidbody.velocity.y;

				this.rigidbody.velocity = v;

				do {

					if(this.is_fallover) {

						break;
					}

					if(!this.is_grounded) {

						break;
					}

					if(animation["M03_nekofailed01"].normalizedTime < 1.0f) {

						break;
					}

					animation.CrossFade("M03_nekofailed02", 0.2f);
					audio.clip = FALL_OVER_SOUND;
					audio.Play();

					this.is_fallover = true;

				} while(false);
			}
			break;

			case STEP.FREE_MOVE:
			{
				float	speed = 400.0f;

				Vector3	v = Vector3.zero;
				
				if(Input.GetKey(KeyCode.RightArrow)) {

					v.x = +speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.LeftArrow)) {

					v.x = -speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.UpArrow)) {

					v.y = +speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.DownArrow)) {

					v.y = -speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.LeftShift)) {

					v.z = +speed*Time.deltaTime;
				}
				if(Input.GetKey(KeyCode.RightShift)) {

					v.z = -speed*Time.deltaTime;
				}

				this.rigidbody.velocity = v;
			}
			break;

		}

		// ---------------------------------------------------------------- //

		this.is_grounded = false;

		this.coli_result.shoji_hit_info.is_enable = false;

		this.coli_result.hole_hit_infos.clear();

		this.coli_result.obstacle_hit_info.is_enable = false;

		this.previous_velocity = this.rigidbody.velocity;

		animation["M02_nekodash"].speed = 4.0f;
	}

	void	OnGUI()
	{
		/*if(this.coli_result.lock_target.enable) {

			GUI.Label(new Rect(10, 10, 100, 20), this.coli_result.lock_target.hole_index.x.ToString() + " " + this.coli_result.lock_target.hole_index.y.ToString());

		} else {

			GUI.Label(new Rect(10, 10, 100, 20), "disable");
		}*/
	}

	// ---------------------------------------------------------------- //
	// 콜리전 관련

	void 	OnCollisionStay(Collision other)
	{
		this.on_collision_common(other);
	}
	void 	OnCollisionEnter(Collision other)
	{
		this.on_collision_common(other);
	}
	private void	on_collision_common(Collision other)
	{
		// 미닫이문의 콜리전에 닿았는지를 점검한다.
		//
		do {

			if(other.gameObject.tag != "Syouji") {

				break;
			}

			ShojiControl	shoji_control = other.gameObject.GetComponent<ShojiControl>();

			if(shoji_control == null) {

				break;
			}

			// 미닫이문의 콜리전에 부딪힌 것을 기록한다.


			Vector3		position = this.transform.TransformPoint(NekoControl.COLLISION_OFFSET);

			ShojiControl.HoleIndex	hole_index = shoji_control.getClosetHole(position);

			this.coli_result.shoji_hit_info.is_enable = true;
			this.coli_result.shoji_hit_info.hole_index = hole_index;
			this.coli_result.shoji_hit_info.shoji_control = shoji_control;

		} while(false);

		// 장지문에 부딪혔는가?
		
		do {
		
			if(other.gameObject.tag != "Obstacle") {
		
				break;
			}
		
			this.coli_result.obstacle_hit_info.is_enable = true;
			this.coli_result.obstacle_hit_info.go        = other.gameObject;
			this.coli_result.obstacle_hit_info.is_steel  = false;

		} while(false);
	}
	
	void 	OnTriggerEnter(Collider other)
	{
		this.on_trigger_common(other);
	}
	
	private void	on_trigger_common(Collider other)
	{
		// 구멍을 통과하였는가?

		do {

			if(other.gameObject.tag != "Hole") {

				break;
			}


			SyoujiPaperControl	paper_control = other.GetComponent<SyoujiPaperControl>();

			if(paper_control == null) {

				break;
			}

			// 격자의 trigger를 통과한 것을 기록한다.

			if(paper_control.step == SyoujiPaperControl.STEP.STEEL) {

				// 철판의 경우, 장애물에 닿은 것으로 간주한다.

				this.coli_result.obstacle_hit_info.is_enable = true;
				this.coli_result.obstacle_hit_info.go        = other.gameObject;
				this.coli_result.obstacle_hit_info.is_steel  = true;

			} else {

				// 종이인 경우
				if(!this.coli_result.hole_hit_infos.full()) {
	
					NekoColiResult.HoleHitInfo		hole_hit_info;
			
					hole_hit_info.paper_control = paper_control;
			
					this.coli_result.hole_hit_infos.push_back(hole_hit_info);
				}
			}

		} while(false);
	}

	// ---------------------------------------------------------------- //

	public void	beginMissAction(bool is_steel)
	{
		this.rigidbody.velocity = this.previous_velocity;
		this.action_miss.is_steel = is_steel;

		this.next_step = STEP.MISS;
	}

	// ---------------------------------------------------------------- //

	// 좌우이동, 점프에 따른 로테이션 
	private void rotation_control()
	{

		// ---------------------------------------------------------------- //
		// 상하 로테이션      
		Quaternion	current = this.transform.GetChild(0).transform.localRotation;
		Quaternion	rot     = current;

		if(this.transform.position.y > 0.0f || this.step == STEP.JUMP) {		
			// ↑처리의 순서상 점프의 첫 1프레임은 y == 0.0f 이기 때문에
			//   step 을 고려하여 점프의 첫 1프레임도 여기로 가져오도록 한다.
	
			rot.x = -this.rigidbody.velocity.y/20.0f;
		
			float	rot_x_diff = rot.x - current.x;
			float	rot_x_diff_limit = 2.0f;

			rot_x_diff = Mathf.Clamp(rot_x_diff, -rot_x_diff_limit*Time.deltaTime, rot_x_diff_limit*Time.deltaTime);

			rot.x = current.x + rot_x_diff;

		} else {
		
			rot.x = current.x;
			rot.x *= 0.9f;
		}

		if(this.step == STEP.MISS) {

			rot.x = current.x;

			if(this.is_grounded) {

				rot.x *= 0.9f;
			}
		}

		// ---------------------------------------------------------------- //
		// 좌우 로테이션  

		rot.y = 0.0f;	
		
		rot.y = this.rigidbody.velocity.x/10.0f;
		
		float	rot_y_diff = rot.y - current.y;
		
		rot_y_diff = Mathf.Clamp(rot_y_diff, -0.015f, 0.015f);
		
		rot.y = current.y + rot_y_diff;

	
		rot.z = 0.0f;

		// ---------------------------------------------------------------- //

		// 자식(모델)만 회전한다.

		this.transform.GetChild(0).transform.localRotation = Quaternion.identity;
		this.transform.GetChild(0).transform.localPosition = Vector3.zero;

		this.transform.GetChild(0).transform.Translate(COLLISION_OFFSET);
		this.transform.GetChild(0).transform.localRotation *= rot;
		this.transform.GetChild(0).transform.Translate(-COLLISION_OFFSET);
	}

	// 좌우로 평행이동
	private	Vector3	side_move(Vector3 velocity, float slide_speed_scale)
	{

		if(Input.GetKey(KeyCode.LeftArrow)) {

			velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else if(Input.GetKey(KeyCode.RightArrow)) {

			velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else {

			// 좌우키를 누르지 않은 경우에 속도는 0으로 돌아간다.

			if(velocity.x > 0.0f) {

				velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Max(velocity.x, 0.0f);

			} else {

				velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Min(velocity.x, 0.0f);
			}
		}

		velocity.x = Mathf.Clamp(velocity.x, -SLIDE_SPEED_MAX, SLIDE_SPEED_MAX);

		return(velocity);
	}

	// 좌우로 평행이동(자동동작)
	private	Vector3	side_move_auto_drive(Vector3 velocity, float slide_speed_scale)
	{
		const float		center_x = 0.0001f;

		if(this.transform.position.x > center_x) {

			velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else if(this.transform.position.x < -center_x) {

			velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

		} else {

            // 좌우키를 누르지 않은 경우에 속도는 0으로 돌아간다.

			if(velocity.x > 0.0f) {

				velocity.x -= SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Max(velocity.x, 0.0f);

			} else {

				velocity.x += SLIDE_ACCEL*slide_speed_scale*Time.deltaTime;

				velocity.x = Mathf.Min(velocity.x, 0.0f);
			}
		}

		// 정가운데에 가까워지면, 서서히 옆으로의 이동이 적어지게 되도록(직진에 가까워진다.)
		velocity.x = Mathf.Clamp(velocity.x, -Mathf.Abs(this.transform.position.x), Mathf.Abs(this.transform.position.x));


		velocity.x = Mathf.Clamp(velocity.x, -SLIDE_SPEED_MAX, SLIDE_SPEED_MAX);

		return(velocity);
	}	
	
	// 자동 동작 시작(클리어 후)
	public void	beginAutoDrive()
	{
		this.is_auto_drive = true;
	}

}
