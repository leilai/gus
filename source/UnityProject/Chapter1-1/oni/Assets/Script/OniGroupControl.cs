using UnityEngine;
using System.Collections;

public class OniGroupControl : MonoBehaviour {

	// 플레이어.
	public PlayerControl	player = null;

	// 카메라.
	public GameObject	main_camera = null;

    // 	scene_control.
	public SceneControl	scene_control = null;

	// 도깨비 프리팹
	public GameObject[]	OniPrefab;
	
	// 그림자 프리팹
	public GameObject	ShadowPrefab;
	
	public AudioClip[]	YarareLevel1;
	public AudioClip[]	YarareLevel2;
	public AudioClip[]	YarareLevel3;

	// 그룹에 속하는 OniPrefab 의 인스턴스
	public OniControl[]	onis;

	// -------------------------------------------------------------------------------- //

	// 콜리전 박스의 크기（１변의 길이）.
	public static float collision_size = 2.0f;

	// 그룹에 속하는 도깨비의 수.
	private	int		oni_num;
	
	// 현재까지의 도깨비의 최대수.
	static private int	oni_num_max = 0;

	// 그룹 전체가 이동하는 속도.
	public float	run_speed = SPEED_MIN;

	// 플레이어와 충돌하였나？.
	public bool	is_player_hitted = false;

	// -------------------------------------------------------------------------------- //

	// 타입.

	public enum TYPE {

		NONE = -1,

		NORMAL = 0,			// 보통.

		DECELERATE,			// 도중에 감속.
		LEAVE,				// 화면 오른쪽으로 서둘러 퇴장(플레이어가 미스한 직후).
		NUM,
	};

	public TYPE		type = TYPE.NORMAL;

	// 스피드 제어의 정보（TYPE = DECELERATE のとき）.
	public struct Decelerate {

		public bool		is_active;			// 감속동작중？.
		public float	speed_base;			// 감속동작을 시작하기 전의 스피드.
		public float	timer;
	};

	public Decelerate	decelerate;

	// -------------------------------------------------------------------------------- //

	public static float		SPEED_MIN = 2.0f;			// 이동 스피드의 최소값.
	public static float		SPEED_MAX = 10.0f;			// 이동 스피드의 최대값.
	public static float		LEAVE_SPEED = 10.0f;		// 퇴장할 때의 스피드.

	// -------------------------------------------------------------------------------- //

	void	Start()
	{
		// 콜리전을 표시한다.(디버그 용).
		this.gameObject.GetComponent<Renderer>().enabled = SceneControl.IS_DRAW_ONI_GROUP_COLLISION;

		this.decelerate.is_active = false;
		this.decelerate.timer     = 0.0f;
	}

	void	Update()
	{
		this.speed_control();

		this.transform.rotation = Quaternion.identity;

		// 퇴장 모드의 경우에 화면 밖에 나오면 삭제한다.
		// （renderer 를 disable 로 하기 때문에、OnBecameInvisible
		// 　는 사용할 수 없다）.
		//
		if(this.type == TYPE.LEAVE) {

			// 그룹의 도깨비 전부가 화면 밖에 존재한다면 각 그룹을 삭제한다. 

			bool	is_visible = false;

			foreach(var oni in this.onis) {

				if(oni.GetComponent<Renderer>().isVisible) {

					is_visible = true;
					break;
				}
			}

			if(!is_visible) {

				Destroy(this.gameObject);
			}
		}
	}

	void FixedUpdate()
	{
		Vector3	new_position = this.transform.position;

		new_position.x += this.run_speed*Time.deltaTime;

		this.transform.position = new_position;
	}

	// 달리는 스피드 제어.
	private void	speed_control()
	{
		switch(this.type) {

			case TYPE.DECELERATE:
			{
				// 플레이어와의 거리가 기준 이하가 되면 감속동작을 시작한다.
				//
				const float	decelerate_start = 8.0f;

				if(this.decelerate.is_active) {

					// １．가속으로 도망친다.
					// ２．플레이어와 같은 속도로 잠시동안 따라간다.
					// ３．역시 무리~와 같은 느낌으로 한 번에 감속.

					float	rate;

					const float		time0 = 0.7f;
					const float		time1 = 0.4f;
					const float		time2 = 2.0f;

					const float		speed_max = 30.0f;
					      float		speed_min = OniGroupControl.SPEED_MIN;

					float	time = this.decelerate.timer;

					do {

						// 가속한다.

						if(time < time0) {

							rate = Mathf.Clamp01(time/time0);
	
							rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, rate)) + 1.0f)/2.0f;

							this.run_speed = Mathf.Lerp(this.decelerate.speed_base, speed_max, rate);

							this.set_oni_motion_speed(2.0f);

							break;
						}
						time -= time0;

						// 플레이어와 같은 속도까지 감속.

						if(time < time1) {

							rate = Mathf.Clamp01(time/time1);
	
							rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, rate)) + 1.0f)/2.0f;

							this.run_speed = Mathf.Lerp(speed_max, PlayerControl.RUN_SPEED_MAX, rate);

							break;
						}
						time -= time1;

						// 좀 더 느린 속도로 감속.

						if(time < time2) {

							rate = Mathf.Clamp01(time/time2);
	
							rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, rate)) + 1.0f)/2.0f;

							this.run_speed = Mathf.Lerp(PlayerControl.RUN_SPEED_MAX, speed_min, rate);

							this.set_oni_motion_speed(1.0f);

							break;
						}
						time -= time2;

						//

						this.run_speed = speed_min;

					} while(false);

					this.decelerate.timer += Time.deltaTime;

				} else {

					float	distance = this.transform.position.x - this.player.transform.position.x;

					if(distance < decelerate_start) {

						this.decelerate.is_active  = true;
						this.decelerate.speed_base = this.run_speed;
						this.decelerate.timer      = 0.0f;
					}
				}
			}
			break;

			case TYPE.LEAVE:
			{
				this.run_speed = LEAVE_SPEED;
			}
			break;

		}

	}

	// 도깨비 그룹을 생성한다.
	public void	CreateOnis(int oni_num, Vector3 base_position)
	{
		this.oni_num = oni_num;
		oni_num_max = Mathf.Max( oni_num_max, oni_num );
		
		this.onis = new OniControl[this.oni_num];
		
		Vector3		average		= new Vector3( 0.0f, 0.0f, 0.0f );
		Vector3		position;

		for(int i = 0;i < this.oni_num;i++) {

			GameObject	go = Instantiate(this.OniPrefab[i%this.OniPrefab.Length]) as GameObject;

			this.onis[i] = go.GetComponent<OniControl>();

			// 도깨비의 위치를 제각각이 되도록 한다.

			position = base_position;

			if(i == 0) {

				// 반드시 한 개 정도의 도깨비는 플레이어와 정면에서 부딪히도록 하고 싶으므로
				// ０번째는 오프셋을 설정한다.		
			
			} else {

				// 난수를 설정하여 위치를 제각각이 되도록한다.

				Vector3	splat_range;
				
				// 그룹 내의 도개비의 수(한 번에 출현하는 수)가 많을수록 제각각으로 되어 있는 범위가 넓어지도록.
				splat_range.x = OniControl.collision_size*(float)(oni_num - 1);
				splat_range.z = OniControl.collision_size*(float)(oni_num - 1)/2.0f;

				// 제각각이 되는 범위가 너무 넓어지지 않도록
				// 플레이어가 칼을 휘둘러 벨 수 있는 범위로 제한한다.
				splat_range.x = Mathf.Min(splat_range.x, OniGroupControl.collision_size);
				splat_range.z = Mathf.Min(splat_range.z, OniGroupControl.collision_size/2.0f);
			
				position.x += Random.Range(0.0f, splat_range.x);
				position.z += Random.Range(-splat_range.z, splat_range.z);
			}

			position.y = 0.0f;
			
			
			this.onis[i].transform.position = position;
			this.onis[i].transform.parent = this.transform;

			this.onis[i].player      = this.player;
			this.onis[i].main_camera = this.main_camera;

			this.onis[i].wave_amplitude    = (i + 1)*0.1f;
			this.onis[i].wave_angle_offset = (i + 1)*Mathf.PI/4.0f;
			
			average		+= this.onis[i].transform.localPosition;
		}
		
		
		GameObject	shadow = Instantiate(this.ShadowPrefab) as GameObject;
		
		// 평균 위치에 그림자를 설정한다.
		average		/= this.oni_num;
		// 높이는 고정
		average.y	= 15.0f;
		
		shadow.transform.parent			= this.transform;
		shadow.transform.localPosition	= average;
	}

	private static int	count = 0;

	// 플레이어의 공격을 받은 경우.
	public void OnAttackedFromPlayer()
	{

		// 쓰러진 도깨비의 수를 늘린다.
		// （↓ 중에도 평가 계산을 실행하고 있기 때문에 먼저 실행해 둔다.）.
		this.scene_control.AddDefeatNum(this.oni_num);

		// 도깨비를 제각각 날아가게 한다.
		//
		// 원뿔의 표면에 각각의 도깨비를 날아가는 방향을 정한다.
		// 평가가 높을 수록 원뿔의 범위가 커져, 보다 넓은 범위로 날아가게 할 수 있다.
		// 플레이어의 속도가 빠르면 원뿔이 앞쪽으로 쏠리게 된다.

		Vector3			blowout;				// 도개비가 날아가는 방향(속도 벡터)
		Vector3			blowout_up;				// ↑의 수직성분
		Vector3			blowout_xz;				// ↑의 수평성분

		float			y_angle;
		float 			blowout_speed;
		float			blowout_speed_base;

		float			forward_back_angle;		// 원뿔의 전후 방향.

		float			base_radius;			// 원뿔 바닥의 반지름.

		float			y_angle_center;
		float			y_angle_swing;			// 원불의 중심（모션의 좌우에 따라 결정되는 값）.

		float			arc_length;				// 원불의 길이（원주）.

		switch(this.scene_control.evaluation) {

			default:
			case SceneControl.EVALUATION.OKAY:
			{
				base_radius = 0.3f;

				blowout_speed_base = 10.0f;

				forward_back_angle = 40.0f;

				y_angle_center = 180.0f;
				y_angle_swing  = 10.0f;
			}
			break;

			case SceneControl.EVALUATION.GOOD:
			{
				base_radius = 0.3f;

				blowout_speed_base = 10.0f;

				forward_back_angle = 0.0f;

				y_angle_center = 0.0f;
				y_angle_swing = 60.0f;
			}
			break;

			case SceneControl.EVALUATION.GREAT:
			{
				base_radius = 0.5f;

				blowout_speed_base = 15.0f;

				forward_back_angle = -20.0f;

				y_angle_center = 0.0f;
				y_angle_swing = 30.0f;
			}
			break;
		}

		forward_back_angle += Random.Range(-5.0f, 5.0f);

		arc_length = (this.onis.Length - 1)*30.0f;

		arc_length = Mathf.Min(arc_length, 120.0f);

		// 플레이어의 모션(오른쪽으로 베기, 왼쪽으로 베기)으로 날아가는 방향을 좌우로 바꾼다.

		y_angle = y_angle_center;

		y_angle += -arc_length/2.0f;

		if(this.player.attack_motion == PlayerControl.ATTACK_MOTION.RIGHT) {

			y_angle += y_angle_swing;

		} else {

			y_angle -= y_angle_swing;
		}

		y_angle += ((OniGroupControl.count*7)%11)*3.0f;

		// 그룹에 속하는 도깨비 전부를 공격당한 것으로 설정한다.
		foreach(OniControl oni in this.onis) {

			//

			blowout_up = Vector3.up;

			blowout_xz = Vector3.right*base_radius;

			blowout_xz = Quaternion.AngleAxis(y_angle, Vector3.up)*blowout_xz;

			blowout = blowout_up + blowout_xz;

			blowout.Normalize();

			// 원뿔을 전후로 향햐게 한다.

			blowout = Quaternion.AngleAxis(forward_back_angle, Vector3.forward)*blowout;

			// 날아가는 속도.

			blowout_speed = blowout_speed_base*Random.Range(0.8f, 1.2f);

			blowout *= blowout_speed;

			if(!SceneControl.IS_ONI_BLOWOUT_CAMERA_LOCAL) {

				// 글로벌하게 날아가는 때에는(카메라의 작동과 연동하지 않는 경우)
				// 플레이어의 속도를 더한다.
				blowout += this.player.GetComponent<Rigidbody>().velocity;
			}

			// 회전.

			Vector3	angular_velocity = Vector3.Cross(Vector3.up, blowout);

			angular_velocity.Normalize();
			angular_velocity *= 3.14f*8.0f*blowout_speed/15.0f*Random.Range(0.5f, 1.5f);

			//angular_velocity = Quaternion.AngleAxis(Random.Range(-30.0f, 30.0f), Vector3.up)*angular_velocity;

			//

			oni.AttackedFromPlayer(blowout, angular_velocity);

			//Debug.DrawRay(this.transform.position, blowout*2.0f, Color.white, 1000.0f);

			//

			y_angle += arc_length/(this.onis.Length - 1);

		}

		// 공격당하는 소리의 SE를 재생한다.
		// 소리가 많이 울리면 혼란스러울 수 있으므로 한 개 정도.
		//
		if(this.onis.Length > 0)
		{
			AudioClip[]	yarareSE = null;
			
			if( this.onis.Length >= 1 && this.onis.Length < 3 )
			{
				yarareSE = this.YarareLevel1;
			}
			else if( this.onis.Length >= 3 && this.onis.Length < 8 )
			{
				yarareSE = this.YarareLevel2;
			}
			else if( this.onis.Length >= 8 )
			{
				yarareSE = this.YarareLevel3;
			}
			
			if( yarareSE != null )
			{
				int index = Random.Range( 0, yarareSE.Length );
				
				this.onis[0].GetComponent<AudioSource>().clip = yarareSE[index];
				this.onis[0].GetComponent<AudioSource>().Play();
			}
		}

		OniGroupControl.count++;

		// 인스턴스를 삭제한다.
		//
		// Destroy(this) 를 하고자 한다면　OniGroupPrefab 의 인스턴스가 아닌 스크립트（OniGroupControl）
		// 를 삭제하게 되기 때문에 주의할 것.
		//
		Destroy(this.gameObject);

	}

	// -------------------------------------------------------------------------------- //

	// 플레이어가 부딪혔을 때의 처리.
	public void	onPlayerHitted()
	{
		this.scene_control.result.score_max += this.scene_control.eval_rate_okay * oni_num_max * this.scene_control.eval_rate;
		this.is_player_hitted = true;
	}

	// 퇴장을 시작한다.
	public void	beginLeave()
	{
		this.GetComponent<Collider>().enabled = false;
		this.type = TYPE.LEAVE;
	}

	// 도깨비의 모션 재생 스피드를 세팅한다.
	private void	set_oni_motion_speed(float speed)
	{
		foreach(OniControl oni in this.onis) {

			oni.setMotionSpeed(speed);
		}
	}

}
