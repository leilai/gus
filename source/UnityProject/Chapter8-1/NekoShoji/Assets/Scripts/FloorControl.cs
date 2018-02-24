using UnityEngine;
using System.Collections;

public class FloorControl : MonoBehaviour {

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		CLOSE = 0,			// 장지문이 닫혀있다.
		OPEN,				// 열려 있다.

		TO_OPEN,			// 장지문이 닫혀있는 곳을 연다.

		CLOSE_SHOJI,		// 미닫이 문이 닫혀있다.

		TO_CLOSE_SHOJI,		// 열려 있는 곳의 장지문이 닫힌다.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;
	public float		step_timer_prev = 0.0f;


	// ---------------------------------------------------------------- //

	// 바닥 폭（Z방향）.
	public static float WIDTH = 15.0f;

	// 바닥 모델 수
	public static int MODEL_NUM = 3;

	public GameObject	shojiPrefab = null;
	public GameObject	fusumaPrefab = null;

	// 장지문, 미닫이 문의 좌표
	public static float		SHUTTER_POSITION_Z       =  15.0f;		// Z좌표
	public static float		SHUTTER_POSITION_OPEN_X  =  4.1f;		// X좌표(열려 있는 경우)
	public static float		SHUTTER_POSITION_CLOSE_X =  1.35f;		// X좌표(닫혀 있는 경우)

	public static int		FUSUMA_NUM = 2;
	public static int		SHOJI_NUM = 1;

	private	GameObject[]	fusuma_objects;
	private	ShojiControl	shoji_object = null;

	// ---------------------------------------------------------------- //
	
	// 미닫이문 등장의 패턴 타입
	public enum CLOSING_PATTERN_TYPE {

		NONE = -1,

		NORMAL = 0,			// 일반적
		OVERSHOOT,			// 오른쪽에서 등장해서 왼쪽에서 멈춘다.
		SECONDTIME,			// 첫번째에 등장하지 않고, 두번째에 등장한다.
		ARCODION,			// 왼쪽에서 미닫이문과 장지문이 동시에

		DELAY,				// 왼쪽에서 장지문이 등장하고, 조금 늦은 타이밍으로 오른쪽에서 미닫이문 등장.
		FALLDOWN,			// 위에서 미닫이 문이 떨어진다.
		FLIP,				// 장지문이 2개 닫힌후, 오른쪽에서 굴러와 미닫이문이 된다.

		SLOW,				// 느리게
		SUPER_DELAY,		// 왼쪽에서 장지문이 등장하고, 매우 느리게 오른쪽에서 미닫이문 등장. 

		NUM,
	};

	public CLOSING_PATTERN_TYPE		closing_pattern_type = CLOSING_PATTERN_TYPE.NORMAL;
	public bool						is_flip_closing = false;								// 등장 패턴을 좌우 반전으로 나타낸다.

	//미닫이 문 등장 패턴 데이터
	public struct ClosingPattern {

		public float	total_time;					// 총 시간.
		public int		fusuma_num;					// 장지문의 수

		// 매 프레임 갱신

		public 	float[]	fusuma_x;					// 각 장지문의 X좌표(매 프레임 갱신).
        public float shoji_x;			     		// 장지문의 X좌표(매 프레임 갱신).
		public	float	shoji_y;
        public float shoji_z_offset;				// 장지문의 Z좌표 오프셋(매 프레임 갱신).

        public float[] fusuma_rot_x;				// 각 장지문의 X좌표(매 프레임 갱신).
        public float shoji_rot_x;				   // 장지문의 X좌표(매 프레임 갱신).

		public	bool	is_play_close_sound;		// 장지문이 닫힐 때에 SE를 재생한다. 
		public	bool	is_play_close_end_sound;	// 장지문이 닫히고 SE를 재생한다. 

		public	float	se_volume;
		public	float	se_pitch;					// SE 의 피치

		public	float	previous_distance;			// 앞 프레임에서의 RoomControl.getDistanceNekoToShutter()
		public	float	local_timer;

		public	ClosingPatternParam	param;			// 범용 파라미터
	};

	// 미닫이문 등장 패턴의 데이터의 범용 파라미터
	public struct ClosingPatternParam {

		public	float	as_float;
		public	bool	as_bool;
	}

	public ClosingPattern	closing_pattern;

	// Sound
	public AudioClip CLOSE_SOUND = null;
	public AudioClip CLOSE_END_SOUND = null;

	// ---------------------------------------------------------------- //

	void Start() 
	{
		//

		this.fusuma_objects = new GameObject[FUSUMA_NUM];

		for(int i = 0;i < FUSUMA_NUM;i++) {

			this.fusuma_objects[i] = Instantiate(this.fusumaPrefab) as GameObject;

			this.fusuma_objects[i].transform.parent = this.gameObject.transform;

			this.fusuma_objects[i].transform.localPosition = new Vector3( SHUTTER_POSITION_OPEN_X, 0.0f, SHUTTER_POSITION_Z);
		}

		//

		this.closing_pattern_type = CLOSING_PATTERN_TYPE.NORMAL;
	}

	void Update()
	{
		this.step_timer_prev = this.step_timer;
		this.step_timer += Time.deltaTime;

		const float		to_open_time = 0.5f;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크한다.

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.TO_OPEN:
				{
					if(this.step_timer > to_open_time) {

						this.next_step = STEP.OPEN;
					}
				}
				break;

				case STEP.TO_CLOSE_SHOJI:
				{
					if(this.step_timer > this.closing_pattern.total_time + Time.deltaTime) {

						this.next_step = STEP.CLOSE_SHOJI;
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환될 때의 초기화

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.CLOSE:
				{
					this.reset_shutters();

					this.fusuma_objects[0].SetActiveRecursively(true);
					this.fusuma_objects[1].SetActiveRecursively(true);

					this.fusuma_objects[0].GetComponent<ShutterControl>().setX(-SHUTTER_POSITION_CLOSE_X);
					this.fusuma_objects[1].GetComponent<ShutterControl>().setX( SHUTTER_POSITION_CLOSE_X);
				}
				break;

				case STEP.OPEN:
				{
					this.reset_shutters();

					this.fusuma_objects[0].SetActiveRecursively(true);
					this.fusuma_objects[1].SetActiveRecursively(true);

					this.fusuma_objects[0].GetComponent<ShutterControl>().setX(-SHUTTER_POSITION_OPEN_X);
					this.fusuma_objects[1].GetComponent<ShutterControl>().setX( SHUTTER_POSITION_OPEN_X);
				}
				break;

				case STEP.TO_CLOSE_SHOJI:
				{
					this.closing_pattern_init();
				}
				break;

				case STEP.CLOSE_SHOJI:
				{
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer_prev = -Time.deltaTime;
			this.step_timer      = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행처리


		switch(this.step) {

			case STEP.TO_OPEN:
			{
				float	rate;
				float	x;

				rate = Mathf.Clamp01(this.step_timer/to_open_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				x = Mathf.Lerp(SHUTTER_POSITION_CLOSE_X, SHUTTER_POSITION_OPEN_X, rate);

				this.fusuma_objects[0].GetComponent<ShutterControl>().setX(x);

				//

				x = Mathf.Lerp(-SHUTTER_POSITION_CLOSE_X, -SHUTTER_POSITION_OPEN_X, rate);

				this.fusuma_objects[1].GetComponent<ShutterControl>().setX(x);
			}
			break;

			case STEP.TO_CLOSE_SHOJI:
			{
				this.closing_pattern_execute();
			}
			break;

		}

		// ---------------------------------------------------------------- //
	}

	private void	reset_shutters()
	{
		for(int i = 0;i < this.fusuma_objects.Length;i++) {

			this.fusuma_objects[i].SetActiveRecursively(false);
		}
	}

	// 등장 패턴을 초기화한다.
	private void	closing_pattern_init()
	{
		switch(this.closing_pattern_type) {

			case CLOSING_PATTERN_TYPE.NORMAL:
			{
				this.closing_pattern.fusuma_num = 1;
				this.closing_pattern.total_time = 0.5f;
			}
			break;

			case CLOSING_PATTERN_TYPE.OVERSHOOT:
			{
				this.closing_pattern.fusuma_num = 2;
				this.closing_pattern.total_time = 1.0f;
			}
			break;

			case CLOSING_PATTERN_TYPE.SECONDTIME:
			{
				this.closing_pattern.fusuma_num = 2;
				this.closing_pattern.total_time = 1.0f;
			}
			break;

			case CLOSING_PATTERN_TYPE.ARCODION:
			{
				this.closing_pattern.fusuma_num = 1;
				this.closing_pattern.total_time = 0.5f;
			}
			break;

			case CLOSING_PATTERN_TYPE.DELAY:
			{
				this.closing_pattern.fusuma_num = 1;
				this.closing_pattern.total_time = 0.8f;
			}
			break;

			case CLOSING_PATTERN_TYPE.FALLDOWN:
			{
				this.closing_pattern.fusuma_num = 2;
				this.closing_pattern.total_time = 1.0f;
			}
			break;

			case CLOSING_PATTERN_TYPE.FLIP:
			{
				this.closing_pattern.fusuma_num = 2;
				this.closing_pattern.total_time = 1.0f;
			}
			break;

			case CLOSING_PATTERN_TYPE.SLOW:
			{
				this.closing_pattern.fusuma_num = 2;
				//this.closing_pattern.total_time = 2.0f;
				this.closing_pattern.total_time = this.closing_pattern.param.as_float;
			}
			break;

			case CLOSING_PATTERN_TYPE.SUPER_DELAY:
			{
				this.closing_pattern.fusuma_num = 1;
				this.closing_pattern.total_time = 2.5f;

			}
			break;
		}

		this.closing_pattern.fusuma_x     = new float[this.closing_pattern.fusuma_num];
		this.closing_pattern.fusuma_rot_x = new float[this.closing_pattern.fusuma_num];

		//

		this.reset_shutters();

		for(int i = 0;i < this.closing_pattern.fusuma_num;i++) {

			this.fusuma_objects[i].SetActiveRecursively(true);

			this.closing_pattern.fusuma_x[i] = -SHUTTER_POSITION_OPEN_X;

			this.closing_pattern.fusuma_rot_x[i] = 0.0f;
		}

		this.closing_pattern.shoji_x = SHUTTER_POSITION_OPEN_X;

		this.closing_pattern.shoji_rot_x = 0.0f;

		// 왼쪽에 있는 장지문을 좌우 반전한다.
		//

		Vector3	scale = new Vector3(-1.0f, 1.0f, 1.0f);

		if(this.is_flip_closing) {

			scale.x *= -1.0f;
		}

		this.fusuma_objects[0].transform.localScale = scale;

		scale.x *= -1.0f;

		for(int i = 1;i < this.closing_pattern.fusuma_num;i++) {

			this.fusuma_objects[i].transform.localScale = scale;
		}

	}

    // step_timer가 time 을 초과했는가?
	private bool	is_step_timer_reach(float time)
	{
		bool	ret = false;

		if(this.step_timer_prev < time && time <= this.step_timer) {

			ret = true;
		}

		return(ret);
	}
	
	// 등장 패턴 실행
	private void closing_pattern_execute()
	{
		float	rate;

		// 「매 프레임 갱신하는 값」을 초기화한다.  

		for(int i = 0;i < this.closing_pattern.fusuma_num;i++) {

			this.closing_pattern.fusuma_x[i]     = SHUTTER_POSITION_OPEN_X;
			this.closing_pattern.fusuma_rot_x[i] = 0.0f;
		}

		this.closing_pattern.shoji_x        = SHUTTER_POSITION_OPEN_X;
		this.closing_pattern.shoji_y        = 0.0f;
		this.closing_pattern.shoji_z_offset = 0.0f;
		this.closing_pattern.shoji_rot_x    = 0.0f;

		this.closing_pattern.is_play_close_sound     = false;
		this.closing_pattern.is_play_close_end_sound = false;

		this.closing_pattern.se_volume = 1.0f;
		this.closing_pattern.se_pitch  = 1.0f;

		// 현재의 위치, 회전 등을 갱신한다.

		switch(this.closing_pattern_type) {

			case CLOSING_PATTERN_TYPE.NORMAL:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				this.closing_pattern.shoji_x = Mathf.Lerp(SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);

				this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time)) {

					this.closing_pattern.is_play_close_end_sound = true;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.OVERSHOOT:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				this.closing_pattern.shoji_x = Mathf.Lerp(SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				if(rate < 0.5f) {

					rate = Mathf.InverseLerp(0.0f, 0.5f, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				} else {

					rate = Mathf.InverseLerp(0.5f, 1.0f, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_CLOSE_X, -SHUTTER_POSITION_OPEN_X, rate);

					this.closing_pattern.fusuma_x[1] = Mathf.Lerp( SHUTTER_POSITION_OPEN_X,  SHUTTER_POSITION_CLOSE_X, rate);
				}

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*Mathf.Asin(0.5f)/(Mathf.PI/2.0f))) {

					this.closing_pattern.is_play_close_end_sound = true;
				}
			}
			break;
			
			case CLOSING_PATTERN_TYPE.SECONDTIME:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				this.closing_pattern.fusuma_x[1] = Mathf.Lerp( SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				if(rate < 0.5f) {

					rate = Mathf.InverseLerp(0.0f, 0.5f, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				} else {

					rate = Mathf.InverseLerp(0.5f, 1.0f, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_CLOSE_X, -SHUTTER_POSITION_OPEN_X, rate);
					
					this.closing_pattern.shoji_x = Mathf.Lerp(SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);
				}

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*Mathf.Asin(0.5f)/(Mathf.PI/2.0f))) {

					this.closing_pattern.is_play_close_end_sound = true;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.ARCODION:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				this.closing_pattern.shoji_x = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);
				this.closing_pattern.shoji_z_offset = 0.01f;

				this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time)) {

					this.closing_pattern.is_play_close_end_sound = true;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.DELAY:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);

				const float	time0 = 0.3f;
				const float	time1 = 0.7f;

				if(rate < time0) {

					// 왼쪽으로 장지문이 닫힌다.

					rate = Mathf.InverseLerp(0.0f, time0, rate);
					rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);
					this.closing_pattern.shoji_x     = SHUTTER_POSITION_OPEN_X;

				} else if(rate < time1) {

					// 잠시 대기

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  SHUTTER_POSITION_OPEN_X;

				} else {

					// 왼쪽으로 미닫이문이 닫힌다.

					rate = Mathf.InverseLerp(time1, 1.0f, rate);
					rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;

					if(this.closing_pattern.param.as_bool) {

						// 미닫이 문이 왼쪽에서(장지문의 뒤에서) 등장한다.

						this.closing_pattern.shoji_x =  Mathf.Lerp(-SHUTTER_POSITION_CLOSE_X, SHUTTER_POSITION_CLOSE_X, rate);

						// 장지문 모델과 겹쳐보이지 않도록 하기 위해 조금 뒤쪽으로 이동시킨다. 
                        this.closing_pattern.shoji_z_offset = 0.01f;

					} else {

						this.closing_pattern.shoji_x =  Mathf.Lerp(SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);
					}
				}

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 2.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*time1)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 2.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time)) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_pitch = 1.5f;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.FALLDOWN:
			{
				const float		height0 = 6.0f;
				const float		height1 = height0/16.0f;

				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);

				if(rate < 0.1f) {

					// 양쪽에서 장지문이 닫힌다.(약간 틈이 있도록)

					rate = Mathf.InverseLerp(0.0f, 0.1f, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X*2.0f, rate);
					this.closing_pattern.fusuma_x[1] = Mathf.Lerp( SHUTTER_POSITION_OPEN_X,  SHUTTER_POSITION_CLOSE_X*2.0f, rate);

					this.closing_pattern.shoji_y = height0;

				} else {

					// 위에서 미닫이문이 떨어진다.  

					rate = Mathf.InverseLerp(0.1f, 1.0f, rate);

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X*2.0f;
					this.closing_pattern.fusuma_x[1] =  SHUTTER_POSITION_CLOSE_X*2.0f;

					this.closing_pattern.shoji_x = 0.0f;

					//

					const float	fall_time0 = 0.5f;
					const float	fall_time1 = 0.75f;
	
					if(rate < fall_time0) {
	
						rate = Mathf.InverseLerp(0.0f, fall_time0, rate);
	
						rate = rate*rate;

						this.closing_pattern.shoji_y = Mathf.Lerp(height0, 0.0f, rate);

					} else if(rate < fall_time1) {
	
						// 바운드

						this.closing_pattern.shoji_x = 0.0f;
	
						rate = Mathf.InverseLerp(fall_time0, fall_time1, rate);
	
						rate = Mathf.Lerp(-1.0f, 1.0f, rate);
	
						rate = 1.0f - rate*rate;

						this.closing_pattern.shoji_y = Mathf.Lerp(0.0f, height1, rate);

					} else {
	
						Vector3	position = this.shoji_object.transform.position;
		
						position.y = 0.0f;
		
						this.shoji_object.transform.position = position;
					}
				}

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 3.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*0.1f)) {

					this.closing_pattern.is_play_close_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*(0.1f + 0.9f*0.5f))) {

					this.closing_pattern.is_play_close_end_sound = true;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*(0.1f + 0.9f*0.75f))) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_volume = 0.1f;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.FLIP:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				const float	time0 = 0.3f;
				const float	time1 = 0.7f;

				if(rate < time0) {

					// 빠르게 닫힌다.（양쪽 모두 장지문）.

					rate = Mathf.InverseLerp(0.0f, time0, rate);

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);
					this.closing_pattern.fusuma_x[1] = Mathf.Lerp( SHUTTER_POSITION_OPEN_X,  SHUTTER_POSITION_CLOSE_X, rate);
					this.closing_pattern.shoji_x     = SHUTTER_POSITION_OPEN_X;

				} else if(rate < time1) {

					// 잠시 대기

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.fusuma_x[1] =  SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  SHUTTER_POSITION_OPEN_X;

				} else {

					// 왼쪽에서 회전하며 미닫이문이 된다.

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.fusuma_x[1] =  SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  SHUTTER_POSITION_OPEN_X;

					//

					rate = Mathf.InverseLerp(time1, 1.0f, rate);

					if(rate < 0.5f) {

						// ０～９０도 장지문을 표시

						rate = Mathf.InverseLerp(0.0f, 0.5f, rate);

						this.closing_pattern.fusuma_x[1] =  SHUTTER_POSITION_CLOSE_X;
						this.closing_pattern.shoji_x     =  SHUTTER_POSITION_OPEN_X;

						//

						this.closing_pattern.fusuma_rot_x[1] = Mathf.Lerp(0.0f, 90.0f, rate);
						this.closing_pattern.shoji_rot_x     = 0.0f;


					} else {

						// ９０～１８０도 미닫이문을 표시 

						rate = Mathf.InverseLerp(0.5f, 1.0f, rate);

						this.closing_pattern.fusuma_x[1] =  SHUTTER_POSITION_OPEN_X;
						this.closing_pattern.shoji_x     =  SHUTTER_POSITION_CLOSE_X;

						//

						this.closing_pattern.fusuma_rot_x[1] = 0.0f;
						this.closing_pattern.shoji_rot_x     = Mathf.Lerp(-90.0f, 0.0f, rate);

					}
				}

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 2.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*time0)) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_pitch = 1.5f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time)) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_pitch = 1.5f;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.SLOW:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);
				rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

				this.closing_pattern.shoji_x = Mathf.Lerp(SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);

				this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 0.5f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time)) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_pitch = 0.5f;
					this.closing_pattern.se_volume = 0.5f;
				}
			}
			break;

			case CLOSING_PATTERN_TYPE.SUPER_DELAY:
			{
				rate = Mathf.Clamp01(this.step_timer/this.closing_pattern.total_time);

				const float	time0 = 0.1f;
				float time1 = this.closing_pattern.param.as_float;
				float time2 = time1 + 0.1f;

				if(rate < time0) {

					// 장지문이 빠르게 닫힌다.

					rate = Mathf.InverseLerp(0.0f, time0, rate);
					rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

					this.closing_pattern.fusuma_x[0] = Mathf.Lerp(-SHUTTER_POSITION_OPEN_X, -SHUTTER_POSITION_CLOSE_X, rate);
					this.closing_pattern.shoji_x     = SHUTTER_POSITION_OPEN_X;

				} else if(rate < time1) {

					// 잠시 대기

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  SHUTTER_POSITION_OPEN_X;

				} else if(rate < time2) {

					// 왼쪽으로 미닫이문이 빠르게 닫힌다.

					rate = Mathf.InverseLerp(time1, time2, rate);
					rate = Mathf.Sin(Mathf.Lerp(0.0f, Mathf.PI/2.0f, rate));

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  Mathf.Lerp(SHUTTER_POSITION_OPEN_X, SHUTTER_POSITION_CLOSE_X, rate);

				} else {

					this.closing_pattern.fusuma_x[0] = -SHUTTER_POSITION_CLOSE_X;
					this.closing_pattern.shoji_x     =  SHUTTER_POSITION_CLOSE_X;
				}
				//

				if(this.is_step_timer_reach(0.0f)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 2.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*time1)) {

					this.closing_pattern.is_play_close_sound = true;
					this.closing_pattern.se_pitch = 2.0f;
				}
				if(this.is_step_timer_reach(this.closing_pattern.total_time*time2)) {

					this.closing_pattern.is_play_close_end_sound = true;
					this.closing_pattern.se_pitch = 1.5f;
				}
			}
			break;
		}

		// 위치, 회전등을 GameObject 에 반영시킨다.

		for(int i = 0;i < this.closing_pattern.fusuma_num;i++) {

			if(!this.is_flip_closing) {

				this.fusuma_objects[i].GetComponent<ShutterControl>().setX(this.closing_pattern.fusuma_x[i]);
				this.fusuma_objects[i].transform.rotation = Quaternion.AngleAxis(this.closing_pattern.fusuma_rot_x[i], Vector3.up);

			} else {

				this.fusuma_objects[i].GetComponent<ShutterControl>().setX(-this.closing_pattern.fusuma_x[i]);
				this.fusuma_objects[i].transform.rotation = Quaternion.AngleAxis(-this.closing_pattern.fusuma_rot_x[i], Vector3.up);
			}
		}

		if(this.shoji_object != null) {

			Vector3	position = this.shoji_object.transform.localPosition;

			if(!this.is_flip_closing) {

				position.x = this.closing_pattern.shoji_x;
				position.y = this.closing_pattern.shoji_y;

				this.shoji_object.transform.rotation = Quaternion.AngleAxis(this.closing_pattern.shoji_rot_x, Vector3.up);

			} else {

				position.x = -this.closing_pattern.shoji_x;
				position.y =  this.closing_pattern.shoji_y;

				this.shoji_object.transform.rotation = Quaternion.AngleAxis(-this.closing_pattern.shoji_rot_x, Vector3.up);
			}

			position.z = SHUTTER_POSITION_Z + this.closing_pattern.shoji_z_offset;

			this.shoji_object.transform.localPosition = position;
		}

		//사운드

		if(this.closing_pattern.is_play_close_sound) {

			this.audio.PlayOneShot(this.CLOSE_SOUND, this.closing_pattern.se_volume);
			this.audio.pitch = this.closing_pattern.se_pitch;
		}
		if(this.closing_pattern.is_play_close_end_sound) {

			this.audio.PlayOneShot(this.CLOSE_END_SOUND, this.closing_pattern.se_volume);
			this.audio.pitch = this.closing_pattern.se_pitch;
		}
	}

    //! 미닫이문을 attach한다.(이 방의 모델을 자식구조로 설정한다.)
	public void	attachShouji(ShojiControl shoji)
	{
		this.shoji_object = shoji;

		if(this.shoji_object != null) {

			this.shoji_object.transform.parent = this.gameObject.transform;

			this.shoji_object.transform.localPosition = new Vector3( SHUTTER_POSITION_OPEN_X, 0.0f, SHUTTER_POSITION_Z);
		}
	}

	// 미닫이문의 등장 패턴을 설정한다.
	public void	setClosingPatternType(CLOSING_PATTERN_TYPE type, bool is_flip)
	{
		ClosingPatternParam		param;

		param.as_float = 0.0f;
		param.as_bool = true;

		this.setClosingPatternType(type, is_flip, param);
	}
	// 미닫이문의 등장 패턴을 설정한다.
	public void	setClosingPatternType(CLOSING_PATTERN_TYPE type, bool is_flip, ClosingPatternParam param)
	{
		this.closing_pattern_type = type;

		this.is_flip_closing = is_flip;

		this.closing_pattern.param = param;
	}

	public void	setClose()
	{
		this.next_step = STEP.CLOSE;
	}
	public void	setOpen()
	{
		this.next_step = STEP.OPEN;
	}

	public void	beginOpen()
	{
		this.next_step = STEP.TO_OPEN;
	}
	public void	beginCloseShoji()
	{
		this.next_step = STEP.TO_CLOSE_SHOJI;
	}
}
