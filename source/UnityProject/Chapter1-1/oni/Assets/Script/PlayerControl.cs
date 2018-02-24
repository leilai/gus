using UnityEngine;
using System.Collections;

// 메모
//
// 회전하지 않도록 하기 위해서는
// rigidbody -> constraint -> freeze rotation
// 에 체크를 추가한다.
//
// 프리팹 복사
// × Ctrl-C Ctrl-V
// ○ Ctrl-D
//
// 적의 콜리전을 통합한다.
//
// 무한히 반복되는 배경 제작 방법
//
// GameObject 에서 스크립트의 변수나 메소드를 사용하고자 하는 경우에는
// GetComponent<클래스명>() 을 사용한다.
//
// 불필요한 인스턴스가 제대로 삭제되어 있는지
// 게임을 정지하여 Hierarchy 뷰를 보면 체크가 용이하다.
//
// 생성한 인스턴스를 GameObject 형식으로 적용하고 싶은 경우에는
// as GameObject 으로 한다.
//
// 인스턴스를 삭제할 때에는 Destory(this) 가 아닌 Destory(this.gameObject)
// 으로 한다.
//
// OnBecameVisible/Invisible() 를 불러오지 않고
// MeshRender 가 무효（Inspector의 체크 박스가 표시되어 있지 않음）
// 인 경우 불러올 수 없다.
//
// On*() 를 불러오지 않고
// 메소드의 이름이 존재하더라도 인수의 형태가 다르면 불러올 수 없다.
// × void OnCollisionEnter(Collider other)
// ○ void OnCollisionEnter(Collision other)
//.

public class PlayerControl : MonoBehaviour {

	// -------------------------------------------------------------------------------- //

	// 사운드
	public AudioClip[]	AttackSound;				// 공격할 때의 사운드.
	public AudioClip	SwordSound;					// 검을 휘두를 때의 사운드.
	public AudioClip	SwordHitSound;				// 부딪히는 사운드（검이 도깨비에 닿았을 때에 사운드）.
	public AudioClip	MissSound;					// 실패한 경우의 사운드.
	public AudioClip	runSound;
	
	public AudioSource	attack_voice_audio;			// 공격사운드
	public AudioSource	sword_audio;				// 검 사운드（휘두르는 사운드, 충돌 사운드）.
	public AudioSource	miss_audio;					// 실패한 경우의 사운드.
	public AudioSource	run_audio;
	
	public int			attack_sound_index = 0;		// 다음에 울리는 AttakSound.

	// -------------------------------------------------------------------------------- //

	// 이동 스피드.
	private	float	run_speed = 5.0f;

	// 이동 스피드의 최대값 [m/sec].
	public static float	RUN_SPEED_MAX = 20.0f;

	// 이동 스피드의 가속치 [m/sec^2].
	private const float	run_speed_add = 5.0f;

	// 이동 스피드의 감속치 [m/sec^2].
	private const float	run_speed_sub = 5.0f*4.0f;

    // 공격판정용 Collider.
	private	AttackColliderControl	attack_collider = null;

	public SceneControl				scene_control = null;

	// 공격 판정이 발생중인 타이머.
	// attack_timer > 0.0f 라면 공격중.
	private float	attack_timer = 0.0f;

	// 헛스윙 후 공격할 수 없는 타이머.
	// attack_disable_timer > 0.0f 라면 공격할 수 없다.
	private float	attack_disable_timer = 0.0f;

	// 공격판정이 지속되는 시간 [sec].
	private static float	ATTACK_TIME = 0.3f;

	// 공격판정이 지속되는 시간 [sec].
	private static float	ATTACK_DISABLE_TIME = 1.0f;

	private bool	is_running = true;

	private bool	is_contact_floor = false;

	private bool	is_playable		= true;
	
	// 정지 목표 위치.
	// （SceneControl.cs が決めた、ここで止まってほしいという位置）.
	public float	stop_position = -1.0f;

	// 공격 모션의 종류.
	public enum ATTACK_MOTION {

		NONE = -1,

		RIGHT = 0,
		LEFT,

		NUM,
	};

	public ATTACK_MOTION	attack_motion = ATTACK_MOTION.LEFT;


	// 검의 궤도 효과.
	public AnimatedTextureExtendedUV	kiseki_left = null;
	public AnimatedTextureExtendedUV	kiseki_right = null;

	// 충돌 효과.
	public ParticleSystem				fx_hit = null;
	
	// 달릴 때의 효과.
	public ParticleSystem				fx_run = null;

	// 
	public	float	min_rate = 0.0f;
	public	float	max_rate = 3.0f;
	
	// -------------------------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		RUN = 0,		// 달린다　　게임중.
		STOP,			// 정지한다 목표점 연출시.
		MISS,			// 실패 도깨비와 충돌하였을 경우
		NUM,
	};

	public STEP		step			= STEP.NONE;
	public STEP		next_step    	= STEP.NONE;

	// -------------------------------------------------------------------------------- //

	void	Start()
	{
        // 공격판정용 Collider를 준비한다.
		this.attack_collider = GameObject.FindGameObjectWithTag("AttackCollider").GetComponent<AttackColliderControl>();

        // 공격판정용 Collider에 플레이어의 인스턴스를 세팅한다.
		this.attack_collider.player = this;

		// 검의 궤도 효과

		this.kiseki_left = GameObject.FindGameObjectWithTag("FX_Kiseki_L").GetComponent<AnimatedTextureExtendedUV>();
		this.kiseki_left.stopPlay();

		this.kiseki_right = GameObject.FindGameObjectWithTag("FX_Kiseki_R").GetComponent<AnimatedTextureExtendedUV>();
		this.kiseki_right.stopPlay();

		// 충돌 효과.

		this.fx_hit = GameObject.FindGameObjectWithTag("FX_Hit").GetComponent<ParticleSystem>();
		
		this.fx_run = GameObject.FindGameObjectWithTag("FX_Run").GetComponent<ParticleSystem>();
		//

		this.run_speed = 0.0f;

		this.next_step = STEP.RUN;

		this.attack_voice_audio = this.gameObject.AddComponent<AudioSource>();
		this.sword_audio        = this.gameObject.AddComponent<AudioSource>();
		this.miss_audio         = this.gameObject.AddComponent<AudioSource>();
		
		this.run_audio         	= this.gameObject.AddComponent<AudioSource>();
		this.run_audio.clip		= this.runSound;
		this.run_audio.loop		= true;
		this.run_audio.Play();
	}

	void	Update()
	{
#if false
		if(Input.GetKey(KeyCode.Keypad1)) {
			min_rate -= 0.1f;
		}
		if(Input.GetKey(KeyCode.Keypad2)) {
			min_rate += 0.1f;
		}
		if(Input.GetKey(KeyCode.Keypad4)) {
			max_rate -= 0.1f;
		}
		if(Input.GetKey(KeyCode.Keypad5)) {
			max_rate += 0.1f;
		}
#endif		
		min_rate = Mathf.Clamp( min_rate, 0.0f, max_rate );
		max_rate = Mathf.Clamp( max_rate, min_rate, 5.0f );
		
		// 다음 상태로 이동할지 하지 않을지를 체크한다.
		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.RUN:
				{
					if(!this.is_running) {
	
						if(this.run_speed <= 0.0f) {
						
							// 주행 사운드와 주행 효과를 정지한다.
							this.fx_run.Stop();
						
							this.next_step = STEP.STOP;
						}
					}
				}
				break;

				case STEP.MISS:
				{
					if(this.GetComponent<Rigidbody>().velocity.y < 0.0f) {

						if(this.is_contact_floor) {
						
							// 주행 효과를 재시작.
							this.fx_run.Play();
						
							this.GetComponent<Rigidbody>().useGravity = true;
							this.next_step = STEP.RUN;
						}
					}
				}
				break;
			}
		}
			
		// 상태 변화시 초기화.
		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.STOP:
				{
					Animation	animation = this.transform.GetComponentInChildren<Animation>();

					animation.Play("P_stop");
				}
				break;

				case STEP.MISS:
				{
					// 비스듬하게 날아간다.

					Vector3	velocity = this.GetComponent<Rigidbody>().velocity;

					float	jump_height = 1.0f;

					velocity.x = -2.5f;
					velocity.y = Mathf.Sqrt(2.0f*9.8f*jump_height);
					velocity.z = 0.0f;

					this.GetComponent<Rigidbody>().velocity = velocity;
					this.GetComponent<Rigidbody>().useGravity = false;

					this.run_speed = 0.0f;

					Animation	animation = this.transform.GetComponentInChildren<Animation>();

					animation.Play("P_yarare");				
					animation.CrossFadeQueued("P_run");

					//

					this.miss_audio.PlayOneShot(this.MissSound);
				
					// 주행 효과를 정지한다.
					this.fx_run.Stop();
				}
				break;
			}

			this.step = this.next_step;

			this.next_step = STEP.NONE;
		}
		
		// 주생 사운드 볼륨 제어
		if( this.is_running ){
			this.run_audio.volume = 1.0f;
		}else{
			this.run_audio.volume = Mathf.Max(0.0f, this.run_audio.volume - 0.05f );
		}
		
		// 각 상태 실행.

		// ---------------------------------------------------- //
		// 포지션.

		switch(this.step) {

			case STEP.RUN:
			{
				// ---------------------------------------------------- //
				// 속도
		
				if(this.is_running) {
		
					this.run_speed += PlayerControl.run_speed_add*Time.deltaTime;
		
				} else {
		
					this.run_speed -= PlayerControl.run_speed_sub*Time.deltaTime;
				}
		
				this.run_speed = Mathf.Clamp(this.run_speed, 0.0f, PlayerControl.RUN_SPEED_MAX);
		
				Vector3	new_velocity = this.GetComponent<Rigidbody>().velocity;
		
				new_velocity.x = run_speed;
		
				if(new_velocity.y > 0.0f) {
		
					new_velocity.y = 0.0f;
				}
		
				this.GetComponent<Rigidbody>().velocity = new_velocity;
		
				float	rate;
			
				rate	= this.run_speed / PlayerControl.RUN_SPEED_MAX;
				this.run_audio.pitch	= Mathf.Lerp( min_rate, max_rate, rate);

				// ---------------------------------------------------- //
				// 공격
		
				this.attack_control();

				this.sword_fx_control();

				// ---------------------------------------------------- //
				// 공격 가능 여부에 따라 색을 바꾼다.(디버그 용).
		
				if(this.attack_disable_timer > 0.0f) {
		
					this.GetComponent<Renderer>().material.color = Color.gray;
		
				} else {
		
					this.GetComponent<Renderer>().material.color = Color.Lerp(Color.white, Color.blue, 0.5f);
				}
		
				// ---------------------------------------------------- //
				// "W" 키로 전방으로 크게 이동(디버그 용).
#if UNITY_EDITOR
				if(Input.GetKeyDown(KeyCode.W)) {
		
					Vector3		position = this.transform.position;
		
					position.x += 100.0f*FloorControl.WIDTH*FloorControl.MODEL_NUM;
		
					this.transform.position = position;
				}
#endif
			}
			break;

			case STEP.MISS:
			{
				this.GetComponent<Rigidbody>().velocity += Vector3.down*9.8f*2.0f*Time.deltaTime;
			}
			break;

		}

		//

		this.is_contact_floor = false;
	}


	void OnCollisionStay(Collision other)
	{
		// 도깨비와 충돌한다면 감속한다.
		//

		if(other.gameObject.tag == "OniGroup") {

			if(this.step != STEP.MISS) {

				this.next_step = STEP.MISS;

				// 플레이어가 도깨비와 충돌했을 때의 처리.

				this.scene_control.OnPlayerMissed();

				// 도깨비 그룹에게 플레이어가 충돌한 것을 기억하게 한다.

				OniGroupControl	oni_group = other.gameObject.GetComponent<OniGroupControl>();
				
				oni_group.onPlayerHitted();
			}
		}

		// 착지하였는가?.
		if(other.gameObject.tag == "Floor") {

			this.is_contact_floor = true;
		}
	}

	// CollisionStay 를 불러오지 않는 경우도 있으므로 아래와 같은 작업을 준비한다.
	void OnCollisionEnter(Collision other)
	{
		this.OnCollisionStay(other);
	}


	// -------------------------------------------------------------------------------- //

	// 공격 효과를 재생한다.
	public void		playHitEffect(Vector3 position)
	{
		this.fx_hit.transform.position = position;

		this.fx_hit.Play();
	}

	// 공격 사운드를 재생한다.
	public void		playHitSound()
	{
		this.sword_audio.PlayOneShot(this.SwordHitSound);
	}

	// 『공격할 수 없는 사이』타이머를 리셋한다.（바로 공격 가능 상태가 된다.）.
	public void 	resetAttackDisableTimer()
	{
		this.attack_disable_timer = 0.0f;
	}

	// 공격을 시작하고 나서 (마우스 버튼을 클릭하고)의 경과시간을 구한다.
	public float	GetAttackTimer()
	{
		return(PlayerControl.ATTACK_TIME - this.attack_timer);
	}

	// 플레이어의 스피드율（0.0f ～ 1.0f）을 구한다.
	public float	GetSpeedRate()
	{
		float	player_speed_rate = Mathf.InverseLerp(0.0f, PlayerControl.RUN_SPEED_MAX, this.GetComponent<Rigidbody>().velocity.magnitude);

		return(player_speed_rate);
	}

	// 멈추고
	public void 	StopRequest()
	{
		this.is_running = false;
	}
	
	// 플레이어 조작 가능
	public void		Playable()
	{
		this.is_playable = true;
	}
	
	// 플레이어 조작 정지
	public void		UnPlayable()
	{
		this.is_playable = false;
	}
	
	// 플레이어가 정지했다？.
	public bool 	IsStopped()
	{
		bool	is_stopped = false;

		do {

			if(this.is_running) {

				break;
			}

			if(this.run_speed > 0.0f) {

				break;
			}

			//

			is_stopped = true;

		} while(false);

		return(is_stopped);
	}

	// 계속해서 감속을 하는 경우의 예상 정지 위치를 구한다.
	public float CalcDistanceToStop()
	{
		float distance = this.GetComponent<Rigidbody>().velocity.sqrMagnitude/(2.0f*PlayerControl.run_speed_sub);

		return(distance);
	}

	// -------------------------------------------------------------------------------- //

	// 공격 입력을 했는가？.
	private bool	is_attack_input()
	{
		bool	is_attacking = false;

		// 마우스 왼쪽 버튼을 클릭하면 공격.
		//
		// OnMouseDown() 은 자기 위에서 클릭하는 경우에만 불러올 수 있다.
		// 이번에는 화면의 어느 부분에서든지 클릭하더라도 반응하도록 하기 위해
        // Input.GetMouseButtonDown() 를 사용한다.
		//
		if(Input.GetMouseButtonDown(0)) {

			is_attacking = true;
		}

		// 디버그용 자동공격.
		if(SceneControl.IS_AUTO_ATTACK) {

			GameObject[] oni_groups = GameObject.FindGameObjectsWithTag("OniGroup");

			foreach(GameObject oni_group in oni_groups) {

				float	distance = oni_group.transform.position.x - this.transform.position.x;
				
				distance -= 1.0f/2.0f;
				distance -= OniGroupControl.collision_size/2.0f;

				// 뒤쪽에 위치해 있는 것은 무시.
				// （이번 게임에서는 일어날 수 있는 상황은 아니지만, 만일에 대비하여）.
				//
				if(distance < 0.0f) {

					continue;
				}

				// 충돌까지의 예상시간.

				float	time_left = distance/(this.GetComponent<Rigidbody>().velocity.x - oni_group.GetComponent<OniGroupControl>().run_speed);

				// 멀리 떨어져 있는 것은 무시.
				//
				if(time_left < 0.0f) {

					continue;
				}

				if(time_left < 0.1f) {
				//if(time_left < 0.05f) {

					is_attacking = true;
				}
			}
		}

		return(is_attacking);
	}

	// 공격 컨트롤.
	private void	attack_control()
	{
		if(!this.is_playable) {
			return;	
		}
		
		if(this.attack_timer > 0.0f) {

			// 공격 판정 발생중.

			this.attack_timer -= Time.deltaTime;

			// 공격 판정 종료 체크.
			if(this.attack_timer <= 0.0f) {

                // collider（공격의 충돌 판정）의 충돌 판정을 무효화한다.
                //
				attack_collider.SetPowered(false);
			}

		} else {

			this.attack_disable_timer -= Time.deltaTime;

			if(this.attack_disable_timer > 0.0f) {

				// 아직 공격할 수 없는 시점.

			} else {

				this.attack_disable_timer = 0.0f;

				if(this.is_attack_input()) {

                    // collider（공격의 충돌 판정）의 충돌 판정을 유효화한다.
					//
					attack_collider.SetPowered(true);
		
					this.attack_timer = PlayerControl.ATTACK_TIME;
	
					this.attack_disable_timer = PlayerControl.ATTACK_DISABLE_TIME;

					// 공격 모션을 재생한다.

					Animation	animation = this.transform.GetComponentInChildren<Animation>();

					// 같은 모션을 초기부터 재생하고자 하는 경우에는 한 번 stop() 하지 않으면 안된다.
					//animation.Stop();

					// 다음으로 재생할 모션을 선택한다.
					//
					// 『도깨비』가 날아가는 방향을 정할 때에 『직전의 공격 모션』을 파악하기 위해
					// 재생 후가 아닌 재생 전에 모션을 선택한다.
					//
					switch(this.attack_motion) {

						default:
						case ATTACK_MOTION.RIGHT:	this.attack_motion = ATTACK_MOTION.LEFT;	break;
						case ATTACK_MOTION.LEFT:	this.attack_motion = ATTACK_MOTION.RIGHT;	break;
					}

					switch(this.attack_motion) {

						default:
						case ATTACK_MOTION.RIGHT:	animation.CrossFade("P_attack_R", 0.2f);	break;
						case ATTACK_MOTION.LEFT:	animation.CrossFade("P_attack_L", 0.2f);	break;
					}

					// 공격 모션이 종료되면 달리는 모션으로 되돌아온다.
					animation.CrossFadeQueued("P_run");

					this.attack_voice_audio.PlayOneShot(this.AttackSound[this.attack_sound_index]);

					this.attack_sound_index = (this.attack_sound_index + 1)%this.AttackSound.Length;

					this.sword_audio.PlayOneShot(this.SwordSound);

				}
			}
		}
	}

	// 검의 궤도 효과.
	private	void	sword_fx_control()
	{

		do {
		
			if(this.attack_timer <= 0.0f) {
		
				break;
			}
		
			if(this.kiseki_left.isPlaying()) {
		
				break;
			}
		
			Animation					animation = this.transform.GetComponentInChildren<Animation>();
			AnimationState				state;
			AnimatedTextureExtendedUV	anim_player;
		
			switch(this.attack_motion) {
		
				default:
				case ATTACK_MOTION.RIGHT:
				{
					state = animation["P_attack_R"];
					anim_player = this.kiseki_right;
				}
				break;
		
				case ATTACK_MOTION.LEFT:
				{
					state = animation["P_attack_L"];
					anim_player = this.kiseki_left;
				}
				break;
			}
		
			float	start_time    = 2.5f;
			float	current_frame = state.time*state.clip.frameRate;
			
			if(current_frame < start_time) {
			
				break;
			}
		
			anim_player.startPlay(state.time - start_time/state.clip.frameRate);
		
		} while(false);
	}
}
