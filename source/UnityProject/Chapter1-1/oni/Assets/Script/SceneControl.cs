using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	// -------------------------------------------------------------------------------- //
	// 프리팹.

	public GameObject		OniGroupPrefab = null;
	public GameObject		OniPrefab = null;
	public GameObject		OniEmitterPrefab = null;
	public GameObject[]		OniYamaPrefab;

	// 2D용 텍스처
	public Texture	TitleTexture = null;			// 『시작』
	public Texture	StartTexture = null;			// 『처음으로』
	public Texture	ReturnButtonTexture = null;		// 『되돌아가기』버튼

	// SE
	public AudioClip	GameStart = null;
	public AudioClip	EvalSound = null;			// 평가
	public AudioClip	ReturnSound = null;			// 되돌아가기
	// -------------------------------------------------------------------------------- //

	// 플레이어.
	public PlayerControl	player = null;

	// 스코어.
	public ScoreControl		score_control = null;
	
	// 카메라
	public GameObject	main_camera = null;

	// 도깨비의 출현을 제어한다.
	public LevelControl	level_control = null;
	
	// 득점 계산을 제어한다.
	public ResultControl result_control = null;

	// 목표 지점에서의 위에서 도깨비가 떨어지도록 하기위한 오브젝트.
	public OniEmitterControl	oni_emitter = null;

	// GUI（２D 표시） 제어.
	private GUIControl	gui_control = null;
	
	// 페이드 컨트롤
	public FadeControl	fader = null;
	
	// -------------------------------------------------------------------------------- //

	// 게임 진행 상태
	public enum STEP {

		NONE = -1,

		START,					// 『시작！』문자가 표시되는 시점.
		GAME,					// 게임 중.
		ONI_VANISH_WAIT,		// 타임오버 후 화면에 존재하는 도깨비가 없어지는 것을 기다린다.
		LAST_RUN,				// 도깨비가 출현하지 않는 잠시동안 이동한다.
		PLAYER_STOP_WAIT,		// 플레이어가 멈추기를 기다린다.

		GOAL,					// 목표 지점 연출.
		ONI_FALL_WAIT,			// 『위에서 도깨비가 내려온다』연출이 종료할 때까지 기다린다.
		RESULT_DEFEAT,			// 쓰러뜨린 수에 대한 평가 표시.
		RESULT_EVALUATION,		// 쓰러뜨린 타이밍에 대한 평가 표시.
		RESULT_TOTAL,			// 종합평가.

		GAME_OVER,				// 타임오버.
		GOTO_TITLE,				// 타이틀로 이동.

		NUM,
	};

	public STEP	step      = STEP.NONE;		// 현재 게임의 진행 상태.
	public STEP	next_step = STEP.NONE;		// 변환하는 상태.
	public float	step_timer      = 0.0f;		// 상태가 변화하는 시간.
	public float	step_timer_prev = 0.0f;

	// -------------------------------------------------------------------------------- //

	// 버튼을 클릭하고 나서 공격이 시작될 때까지의 시간(평가에 사용).
	public float		attack_time = 0.0f;


	// 평가.
	// 도깨비를 가까이에서 공격할수록 고득점.
	public enum EVALUATION {

		NONE = -1,

		OKAY = 0,		// 보통.
		GOOD,			// 우수함.
		GREAT,			// 훌륭함.

		MISS,			// 실패（충돌했다）.

		NUM,
	};
	public static string[] evaluation_str = {

		"okay",
		"good",
		"great",
		"miss",
	};
	
	public EVALUATION	evaluation = EVALUATION.NONE;

	// -------------------------------------------------------------------------------- //

	// 게임 전체의 결과.
	public struct Result {

		public int		oni_defeat_num;			// 쓰러뜨린 도깨비의 수(총합)
		public int[]	eval_count;				// 각 평가의 횟수.

		public int		rank;					// 게임 전체의 결과.
		
		public float	score;					// 현재 스코어.
		public float	score_max;				// 게임 내에서 받을 수 있는 최대 득점.
		
	};

	public Result	result;

	// -------------------------------------------------------------------------------- //

	// 한 번에 출현하는 도깨비 수의 최대값.
	// 실패하지 않고 계속 성공하면 도깨비의 수가 계속적으로 증가하지만, 최대값 이상으로 증가하지는 않는다.
	public static int	ONI_APPEAR_NUM_MAX = 10;

	// 게임이 종료될 때 도깨비 그룹의 수
	public int				oni_group_appear_max = 50;
	//private int				oni_group_appear_max = 50;
	
	// 실패시에 줄어드는 출현 수.
	public static int		oni_group_penalty = 1;
	
	// 스코어를 감추는 출현수.
	public static float		SCORE_HIDE_NUM = 40;
	
	// 그룹의 출현수.
	public int				oni_group_num = 0;

	// 공격 or 충돌한 도깨비 그룹의 수.
	public int				oni_group_complite = 0;
	
	// 공격한 도깨비 그룹의 수.
	public int				oni_group_defeat_num = 0;
	
	// 충돌한 도깨비 그룹의 수.
	public int				oni_group_miss_num = 0;
	
	// 시작 연출（『시작!』의 문자가 나오는 상황）의 시간.
	private static float	START_TIME = 2.0f;

	// 목표 지점 연출시에『도깨비가 밀집해 있는 곳』에서 『플레이어가 정지하는 위치』까지의 거리.
	private static float	GOAL_STOP_DISTANCE = 8.0f;

	// 평가를 정할 때, 버튼을 클릭하고 공격이 닿을 때까지의 경과시간.
	public static float	ATTACK_TIME_GREAT = 0.05f;
	public static float	ATTACK_TIME_GOOD  = 0.10f;

	// -------------------------------------------------------------------------------- //
	// 디버그용 플래그 이모저모.
	// 적절히 변경하여 게임이 어떻게 바뀔지 태스트해 보세요! .
	// true 로 하면, 쓰러뜨린 도깨비가 카메라의 로컬 좌표계에서 이동하게 된다. 
	// 카메라 작동과 연동하기 때문에 카메라가 갑자기 정지하는 경우라도 움직임이 부자연스럽게 변화
	// 하지는 않는다.
	//
	public static bool	IS_ONI_BLOWOUT_CAMERA_LOCAL = true;

    // 도깨비 그룹의 COLLISION을 표시한다.(디버그 용).
    // 도깨비는 몇 마리가 통합되어 출현하지만, 그룹으로 공통의 COLLISION을 사용한다.
	//
	// 이것은
	//
	// ・플레이어가 도깨비에 닿는 경우의 움직임을 조정하기 쉽도록 하기 위해
	// ・공격당한 도깨비가 날아가는 효과가 좀 더 재미있기 때문에
    //
	// 등의 이유 때문입니다.
	//
	public static bool	IS_DRAW_ONI_GROUP_COLLISION = false;

	// 플레이어의 공격시에 공격판정을 표시한다.
	public static bool	IS_DRAW_PLAYER_ATTACK_COLLISION = false;

	// 디버그용 전자동기능
	// true 로 설정하면 공격판정이 나오게 된다.
	//
	public static bool	IS_AUTO_ATTACK = false;

	// AUTO_ATTACK  경우의 평가
	public EVALUATION	evaluation_auto_attack = EVALUATION.GOOD;
	
	// 공격한 도깨비의 수가 사라지는 순간의 도깨비 수.
	private int         backup_oni_defeat_num = -1;
	
	// 디버그용의 배경 모델을 표시한다.(적색, 청색, 녹색이 되도록)
	public static bool	IS_DRAW_DEBUG_FLOOR_MODEL = false;

	public	float		eval_rate_okay  = 1.0f;
	public	float		eval_rate_good  = 2.0f;
	public	float		eval_rate_great = 4.0f;
	public	int			eval_rate		= 1;
	
	// -------------------------------------------------------------------------------- //
	
	void	Start()
	{
		// 플레이어의 인스턴스를 준비한다.
		this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

		this.player.scene_control = this;

		// 스코어의 인스턴스를 준비한다.
		this.score_control = GetComponent<ScoreControl>();
		
		// 카메라의 인스턴스를 준비한다.
		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.level_control = new LevelControl();
		this.level_control.scene_control = this;
		this.level_control.player = this.player;
		this.level_control.OniGroupPrefab = this.OniGroupPrefab;
		this.level_control.create();
		
		this.result_control = new ResultControl();

		// GUI 제어 스크립트(컴포넌트.
		this.gui_control = this.GetComponent<GUIControl>();
		
		// 페이드 컨트롤 추가.
		fader = gameObject.AddComponent<FadeControl>();
		
		// 게임 결과를 클리어한다.
		this.result.oni_defeat_num = 0;
		this.result.eval_count = new int[(int)EVALUATION.NUM];
		this.result.rank = 0;
		this.result.score = 0;
		this.result.score_max = 0;
		
		for(int i = 0;i < this.result.eval_count.Length;i++) {

			this.result.eval_count[i] = 0;
		}
		
		// 페이드 인으로 시작.
		this.fader.fade( 3.0f, new Color( 0.0f, 0.0f, 0.0f, 1.0f ), new Color( 0.0f, 0.0f, 0.0f, 0.0f ) );
		
		this.step = STEP.START;
	}

	void	Update()
	{
		// 게임의 현재 상태를 관리한다.
		this.step_timer_prev = this.step_timer;
		this.step_timer += Time.deltaTime;

		// 다음 상태로 이동할지를 점검한다.
		switch(this.step) {
		
			case STEP.START:
			{
				if(this.step_timer > SceneControl.START_TIME) {
					next_step = STEP.GAME;
				}
			}
			break;

			case STEP.GAME:
			{
				// 출현 최대수를 초과하면 도깨비의 발생을 정지한다.
				if(this.oni_group_complite >= this.oni_group_appear_max )
				{
					next_step = STEP.ONI_VANISH_WAIT;
				}
			
				if(this.oni_group_complite >= SCORE_HIDE_NUM && this.backup_oni_defeat_num == -1)
				{
					this.backup_oni_defeat_num = this.result.oni_defeat_num;
				}
			}
			break;

			case STEP.ONI_VANISH_WAIT:
			{
				do {

					// 도깨비（공격하기 전）을 모두 공격할 때까지 기다린다.
					if(GameObject.FindGameObjectsWithTag("OniGroup").Length > 0) {

						break;
					}

					// 플레이어가 가속할 때까지 기다린다.
					// 도깨비 산을 화면 밖에 출현시키기 위해 가장 마지막 도깨비를 쓰러뜨리고나서 일정 간격
					// 달리도록 한다.
				if(this.player.GetSpeedRate() < 0.5f) {

						break;
					}

					//

					next_step = STEP.LAST_RUN;

				} while(false);
			}
			break;

			case STEP.LAST_RUN:
			{
				if(this.step_timer > 2.0f) {

					// 플레이어를 정지시킨다.
					next_step = STEP.PLAYER_STOP_WAIT;
				}
			}
			break;

			case STEP.PLAYER_STOP_WAIT:
			{
				// 플레이어가 정지하면 목표 지점 연출 시작.
				if(this.player.IsStopped()) {
				
					this.gui_control.score_control.setNumForce(this.backup_oni_defeat_num);
					this.gui_control.score_control.setNum(this.result.oni_defeat_num);
					next_step = STEP.GOAL;
				}
			}
			break;

			case STEP.GOAL:
			{
				// 도깨비가 전부화면에 나올 때까지 기다린다.
				if(this.oni_emitter.oni_num == 0) {

					this.next_step = STEP.ONI_FALL_WAIT;
				}
			}
			break;

			case STEP.ONI_FALL_WAIT:
			{
				if(!this.score_control.isActive() && this.step_timer > 1.5f) {
					this.next_step = STEP.RESULT_DEFEAT;
				}
			}
			break;

			case STEP.RESULT_DEFEAT:
			{
				if(this.step_timer >= 0.4f && this.step_timer_prev < 0.4f )
				{
					// SE（『뚜둥~』）.
					this.GetComponent<AudioSource>().PlayOneShot(this.EvalSound);
				}
				// 평가 표시가 종료될 때까지 기다린다.
				//
				if(this.step_timer > 0.5f) {

					this.next_step = STEP.RESULT_EVALUATION;
				}
			}
			break;
			
			case STEP.RESULT_EVALUATION:
			{
				if(this.step_timer >= 0.4f && this.step_timer_prev < 0.4f )
				{
					// SE（『뚜둥~』）.
					this.GetComponent<AudioSource>().PlayOneShot(this.EvalSound);
				}
				// 평가 표시가 종료될 때까지 기다린다.
				//
				if(this.step_timer > 2.0f) {

					this.next_step = STEP.RESULT_TOTAL;
				}
			}
			break;
			
			case STEP.RESULT_TOTAL:
			{
				if(this.step_timer >= 0.4f && this.step_timer_prev < 0.4f )
				{
					// SE（『뚜둥~』）.
					this.GetComponent<AudioSource>().PlayOneShot(this.EvalSound);
				}
                // 평가 표시가 종료될 때까지 기다린다.
				//
				if(this.step_timer > 2.0f) {

					this.next_step = STEP.GAME_OVER;
				}
			}
			break;

			case STEP.GAME_OVER:
			{
				// 마우스를 클릭하면 페이드아웃하여 타이틀 화면으로 돌아온다.
				//
				if(Input.GetMouseButtonDown(0)) {
				
					// 페이드 아웃시킨다.
					this.fader.fade( 1.0f, new Color( 0.0f, 0.0f, 0.0f, 0.0f ), new Color( 0.0f, 0.0f, 0.0f, 1.0f ) );
					this.GetComponent<AudioSource>().PlayOneShot(this.ReturnSound);
					
					this.next_step = STEP.GOTO_TITLE;
				}
			}
			break;
			
			case STEP.GOTO_TITLE:
			{
				// 페이드가 종료되면 타이틀 화면으로 돌아온다.
				//
				if(!this.fader.isActive()) { 
					Application.LoadLevel("TitleScene");
				}
			}
			break;
		}

		// 상태가 바뀌는 경우 초기화 처리.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
			
				case STEP.PLAYER_STOP_WAIT:
				{
					// 플레이어를 정지한다.
					this.player.StopRequest();

					// -------------------------------------------------------- //
					// 『도깨비가 밀집되어 있는 산』을 생성한다..
					
					if( this.result_control.getTotalRank() > 0 ) {
						GameObject	oni_yama = Instantiate(this.OniYamaPrefab[this.result_control.getTotalRank() - 1]) as GameObject;
				
						Vector3		oni_yama_position = this.player.transform.position;
				
						oni_yama_position.x += this.player.CalcDistanceToStop();
						oni_yama_position.x += SceneControl.GOAL_STOP_DISTANCE;
	
						oni_yama_position.y = 0.0f;
				
						oni_yama.transform.position = oni_yama_position;
					}
					else{
						
					}

					// -------------------------------------------------------- //
				}
				break;

				case STEP.GOAL:
				{
					// 목표 지점 연출 시작.

                    // 『도깨비가 화면 위에서 날아든다.』용의 Emitter를 생성시킨다.

					GameObject	go = Instantiate(this.OniEmitterPrefab) as GameObject;
	
					this.oni_emitter = go.GetComponent<OniEmitterControl>();

					Vector3		emitter_position = oni_emitter.transform.position;

					// 도깨비 산 위에 설정한다..

					emitter_position.x  = this.player.transform.position.x;
					emitter_position.x += this.player.CalcDistanceToStop();
					emitter_position.x += SceneControl.GOAL_STOP_DISTANCE;
	
					this.oni_emitter.transform.position = emitter_position;

					// 최종 평가에서 보통 도깨비의 수를 변경한다.

					int		oni_num = 0;

					switch(this.result_control.getTotalRank()) {
						case 0:		oni_num = Mathf.Min( this.result.oni_defeat_num, 10 );	break;
						case 1:		oni_num = 6;	break;
						case 2:		oni_num = 10;	break;
						case 3:		oni_num = 20;	break;
					}
				
					this.oni_emitter.oni_num = oni_num;
					if( oni_num == 0 )
					{
						this.oni_emitter.is_enable_hit_sound = false;
					}
				}
				break;

				case STEP.RESULT_DEFEAT:
				{
					// 평가가 나온 후에는 도깨비의 낙하 사운드를 재생하지 않도록 한다.
					this.oni_emitter.is_enable_hit_sound = false;
				}
				break;
			}

			this.step = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
			this.step_timer_prev = -1.0f;
		}

		// 각 상태에서의 실행처리.

		switch(this.step) {

			case STEP.GAME:
			{
				// 도깨비 출현 제어.
				this.level_control.oniAppearControl();
			}
			break;

			case STEP.RESULT_DEFEAT:
			{
				// 평가 문자.
				this.gui_control.updateEval(this.step_timer);
			}
			break;
			
			case STEP.RESULT_EVALUATION:
			{
                // 평가 문자.
				this.gui_control.updateEval(this.step_timer);
			}
			break;
			
			case STEP.RESULT_TOTAL:
			{
                // 평가 문자.
				this.gui_control.updateEval(this.step_timer);
			}
			break;
		}

	}

	// 플레이어가 실패했을 때의 처리.
	public void	OnPlayerMissed()
	{
		this.oni_group_miss_num++;
		this.oni_group_complite++;
		this.oni_group_appear_max -= oni_group_penalty;
		
		this.level_control.OnPlayerMissed();

		this.evaluation = EVALUATION.MISS;

		this.result.eval_count[(int)this.evaluation]++;

		// 화면상의 그룹 전부를 퇴장시킨다.

		GameObject[] oni_groups = GameObject.FindGameObjectsWithTag("OniGroup");

		foreach(var oni_group in oni_groups) {
			this.oni_group_num--;
			oni_group.GetComponent<OniGroupControl>().beginLeave();
		}
	}

	// 쓰러진 도깨비의 수를 추가.
	public void	AddDefeatNum(int num)
	{
		this.oni_group_defeat_num++;
		this.oni_group_complite++;
		this.result.oni_defeat_num += num;
		
		// 버튼을 클릭한 시간으로 평가를 정한다.
		// （클릭하여 공격이 닿기까지의 시간이 짧다=바로 앞까지 가서 공격한다.）.

		this.attack_time = this.player.GetComponent<PlayerControl>().GetAttackTimer();

		if(this.evaluation == EVALUATION.MISS) {

			// 실패한 직후에는 OKAY 만 표시.
			this.evaluation = EVALUATION.OKAY;

		} else {

			if(this.attack_time < ATTACK_TIME_GREAT) {
	
				this.evaluation = EVALUATION.GREAT;
	
			} else if(this.attack_time < ATTACK_TIME_GOOD) {
	
				this.evaluation = EVALUATION.GOOD;
	
			} else {
	
				this.evaluation = EVALUATION.OKAY;
			}
		}

		if(SceneControl.IS_AUTO_ATTACK) {

			this.evaluation = this.evaluation_auto_attack;
		}

		this.result.eval_count[(int)this.evaluation] += num;
		
		// 득점 계산.
		float[] score_list = { this.eval_rate_okay, this.eval_rate_good, this.eval_rate_great, 0 };
		this.result.score_max += num * this.eval_rate_great;
		this.result.score += num * score_list[(int)this.evaluation];
		
		this.result_control.addOniDefeatScore(num);
		this.result_control.addEvaluationScore((int)this.evaluation);
	}
	
	//스코어를 표시해도 좋을지 그렇지 않은지.
    public bool IsDrawScore()
	{
		if( this.step >= STEP.GOAL )
		{
			return true;
		}
		
		if(this.oni_group_complite >= SCORE_HIDE_NUM )
		{
			return false;
		}
		
		return true;
	}

	// -------------------------------------------------------------------------------- //

}
