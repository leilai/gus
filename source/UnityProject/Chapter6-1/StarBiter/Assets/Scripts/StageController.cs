using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// StageController
//  - 스테이지를 제어
//  - 사용 방법
//    - 스크립트가 첨부된 게임 오브젝트를 배치한다.
//  - 동작 방법
//    - 스테이지는 아래와 같이 진행한다.
//      1.  시작
//      2.  탐색병 적과 만남
//          - 적기 종류: TYPE 01 + TYPE 04
//      3.  제 1 공격
//          - 적기 종류: TYPE 01 + TYPE 01 배틀 스테이션
//      4.  탐색병 적과 만남
//          - 적기 종류: TYPE 01 + TYPE 02 + TYPE 04
//      5.  제 2 공격
//          - 적기 종류: TYPE 01 + TYPE 02 + TYPE 02 バトルステーション
//      6.  탐색병인 적과 만남
//          - 적기 종류: TYPE 01 + TYPE 02 + TYPE 03 + TYPE 04
//      7.  제 3 공격
//          - 적기 종류: TYPE 01 + TYPE 02 + TYPE 03 + TYPE 03 バトルステーション
//      8.  일시적 정적
//      9.  BOSS등장
//      10. BOSS전투
//      11. 게임오버
//    - ※배틀 스테이션: 적기의 포메이션 공격
// ---------------------------------------------------------------------------
public class StageController : MonoBehaviour {

	public GameObject EnemyMakerType01Formation;	// ENEMY MAKER TYPE 01 FORMATION
	public GameObject EnemyMakerType02Formation;	// ENEMY MAKER TYPE 02 FORMATION
	public GameObject EnemyMakerType03Formation;	// ENEMY MAKER TYPE 03 FORMATION
	public GameObject EnemyMakerType01;		// ENEMY MAKER TYPE 01
	public GameObject EnemyMakerType02;		// ENEMY MAKER TYPE 02
	public GameObject EnemyMakerType03;		// ENEMY MAKER TYPE 03
	public GameObject EnemyMakerType04;		// ENEMY MAKER TYPE 04
	public GameObject Boss;					// BOSS
	public GameObject GameOver;
	
	private enum Level
	{
		DEBUG,
		EASY,
		NORMAL,
		HARD
	}
	private Level level = Level.NORMAL;
	
	// 레벨별 작성 수
	//  - 0: TYPE01.
	//  - 1: TYPE01 FORMATION.
	//  - 2: TYPE02.
	//  - 3: TYPE02 FORMATION.
	//  - 4: TYPE03.
	//  - 5: TYPE03 FORMATION.
	private int[,] maxEnemyInSceneByLevel =
	{
		{ 0, 0, 0, 0, 0, 0 },
		{ 1, 1, 1, 1, 1, 1 },
		{ 3, 4, 3, 2, 1, 2 },
		{ 6, 6, 6, 4, 6, 4 },
	};

	private enum EnemyType
	{
		TYPE01,
		TYPE01FORMATION,
		TYPE02,
		TYPE02FORMATION,
		TYPE03,
		TYPE03FORMATION,
	}
	
	// 각 스테이지
	private enum Stage
	{
		START,								// 시작
		PATROL1,							// 탐색병1
        ATTACK1,							// 제 1 공격
        PATROL2,							// 탐색병2
        ATTACK2,							// 제 2 공격
        PATROL3,							// 탐색병3
        ATTACK3,							// 제 3 공격
		SILENCE,							// 정적.
		BOSS,								// 정지.
		GAMECLEAR,							// 게임 클리어
		END
	}
	
	// 스테이지의 각 상태テージの各状態.
	private enum State
	{
		INITIALIZE,							// 준비
		NOWPLAYING,							// 플레이중
		END									// 종료
	}
	
	private Stage stage;					// 스테이지
	private State state;					// 스테이지 상태
	
	private GameObject enemyMakerType01Formation;	// ENEMY MAKER TYPE 01 FORMATION 인스턴스  
    private GameObject enemyMakerType02Formation;	// ENEMY MAKER TYPE 02 FORMATION 인스턴스  
    private GameObject enemyMakerType03Formation;	// ENEMY MAKER TYPE 03 FORMATION 인스턴스  
    private GameObject enemyMakerType01;	// ENEMY MAKER TYPE 01 인스턴스  
    private GameObject enemyMakerType02;	// ENEMY MAKER TYPE 02 인스턴스  
    private GameObject enemyMakerType03;	// ENEMY MAKER TYPE 03 인스턴스  
    private GameObject enemyMakerType04;	// ENEMY MAKER TYPE 04 인스턴스  
    private GameObject boss;				// BOSS 인스턴스  
    private PlayerStatus playerStatus;		// playerStatus의 인스턴스             
	private GameObject subScreenMessage;	// SubScreen의 메세지 영역
	
	private GameObject txtStageController;	// DEBUG
	
	void Start () 
	{
		// --------------------------------------------------------------------
		// 각 인스턴스를 취득
		// --------------------------------------------------------------------
		
		// SubScreenMessage의 인스턴스를 취득
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");

        // playerStatus의 인스턴스를 취득.
		playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();
		
		// --------------------------------------------------------------------
		// 초기값
		// --------------------------------------------------------------------
		
		// 스테이지를 설정
		stage = Stage.START;
		state = State.INITIALIZE;
		
		// --------------------------------------------------------------------
		// 각 인스턴스를 생성.
		// --------------------------------------------------------------------

		if ( EnemyMakerType01Formation )
		{
			enemyMakerType01Formation = Instantiate(
				EnemyMakerType01Formation,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType02Formation )
		{
			enemyMakerType02Formation = Instantiate(
				EnemyMakerType02Formation,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType03Formation )
		{
			enemyMakerType03Formation = Instantiate( 
				EnemyMakerType03Formation,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType01 )
		{
			enemyMakerType01 = Instantiate( 
				EnemyMakerType01,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType02 )
		{
			enemyMakerType02 = Instantiate(
				EnemyMakerType02,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType03 )
		{
			enemyMakerType03 = Instantiate( 
				EnemyMakerType03, 
				Vector3.zero, 
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType04 )
		{
			enemyMakerType04 = Instantiate( 
				EnemyMakerType04, 
				Vector3.zero, 
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( Boss )
		{
			boss = Instantiate(
				Boss, 
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		
	}
	
	void Update () 
	{
		
		// 스테이지를 갱신
		UpdateStage();

	}
	
	// ------------------------------------------------------------------------
	// 각 스테이지 처리
	// ------------------------------------------------------------------------
	private void UpdateStage()
	{
		// 플레이어를 조작가능한 경우에만 스테이지를 진행한다                
		if ( playerStatus.GetIsNOWPLAYING() )
		{			
			// 시작
			StageStart();
			
			// 시작 종료
			StageStartEnd();
			
			// 탐색병(1).
			StagePatrol1Start();
			
			// 탐색병(1) 종료.
			StagePatrol1End();

            // 제 1 공격
			StageAttack1Start();

            // 제 1 공격 종료
			StageAttack1End();

            // 탐색병(2).
			StagePatrol2Start();

            // 탐색병(2) 종료.
			StagePatrol2End();

            // 제 2 공격
			StageAttack2Start();

            // 제 2 공격 종료
			StageAttack2End();

            // 탐색병(3).
			StagePatrol3Start();

            // 탐색병(3) 종료.
			StagePatrol3End();

            // 제 3 공격
			StageAttack3Start();

            // 제 3 공격 종료
			StageAttack3End();
	
			// 잠시 정적
			SilenceStart();
			
			// 잠시 정적 종료
			SilenceEnd();
			
			// BOSS전 시작
			BossAttackStart();
			
			// BOSS전 종료
			BossAttackEnd();
			
			// GameClear
			GameClearWait();
			
			// GameClear
			GameClearMessage();
		}
	}

	// ------------------------------------------------------------------------
	// 스테이지 - 시작.
	// ------------------------------------------------------------------------
	private void StageStart()
	{
		if ( stage == Stage.START && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// TYPE 01 작성을 시작
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
			}
			
			// 스테이지 종료 시간을 설정
			StartCoroutine( WaitAndUpdateState( 3f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 스테이지 - 시작의 종료
	// ------------------------------------------------------------------------
	private void StageStartEnd()
	{
		if ( stage == Stage.START && state == State.END )
		{
			// 다음 스테이지로
			stage = Stage.PATROL1;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 스테이지 - 탐색병(1)
	// ------------------------------------------------------------------------
	private void StagePatrol1Start()
	{
		if ( stage == Stage.PATROL1 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;	

			// TYPE 04 의 작성을 시작
			if ( enemyMakerType04 )
			{
				enemyMakerType04.GetComponent<EnemyMaker>().
					SetMaxEnemysInScene( 1 );
				enemyMakerType04.GetComponent<EnemyMaker>().
					SetStage( (int)stage );
			
				// 서브 스크린에 메세지 출력
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"PATROL SHIP IS COMING AHEAD." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 탐색병(1) 종료
	// ------------------------------------------------------------------------
	private void StagePatrol1End()
	{
		if ( stage == Stage.PATROL1 && state == State.END )
		{
            // 다음 스테이지로
			stage = Stage.ATTACK1;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 제 1 공격
	// ------------------------------------------------------------------------
	private void StageAttack1Start()
	{
		if ( stage == Stage.ATTACK1 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
					
			// TYPE 01 FORMATION 작성 시작
			if ( enemyMakerType01Formation )
			{
				enemyMakerType01Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01FORMATION
					] );
				enemyMakerType01Formation.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );

                // 서브 스크린에 메세지 출력
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage", 
					"BATTLE STATIONS HAS APPROACHED." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}

            // 스테이지 종료 시간을 설정
			StartCoroutine( WaitAndUpdateState( 20f, State.END ) );

		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 제 1 공격 종료
	// ------------------------------------------------------------------------
	private void StageAttack1End()
	{
		if ( stage == Stage.ATTACK1 && state == State.END )
		{
			// TYPE 01 FORMATION 의 작성을 중지
			if ( enemyMakerType01Formation )
			{
				enemyMakerType01Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}

            // 다음 스테이지로
			stage = Stage.PATROL2;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 탐색병(2)
	// ------------------------------------------------------------------------
	private void StagePatrol2Start()
	{
		if ( stage == Stage.PATROL2 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// TYPE 02  작성 시작
			if ( enemyMakerType02 )
			{
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE02
					] );
			}
			
			// TYPE 04  작성 시작
			if ( enemyMakerType04 )
			{
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 1 );
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );

                // 서브 스크린에 메세지 출력
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"PATROL SHIP IS COMING AHEAD." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 탐색병(2) 종료
	// ------------------------------------------------------------------------
	private void StagePatrol2End()
	{
		if ( stage == Stage.PATROL2 && state == State.END )
		{
            // 다음 스테이지로
			stage = Stage.ATTACK2;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 제 2 공격 
	// ------------------------------------------------------------------------
	private void StageAttack2Start()
	{
		if ( stage == Stage.ATTACK2 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// TYPE 01 의 작성량을 늘린다.
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
			}
			
			// TYPE 02 FORMATION  작성 시작
			if ( enemyMakerType02Formation )
			{
				enemyMakerType02Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE02FORMATION
					] );
				enemyMakerType02Formation.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );

                // 서브 스크린에 메세지 출력
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"BATTLE STATIONS HAS APPROACHED." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}

            // 스테이지 종료 시간을 설정
			StartCoroutine( WaitAndUpdateState( 20f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 제 2 공격 종료
	// ------------------------------------------------------------------------
	private void StageAttack2End()
	{
		if ( stage == Stage.ATTACK2 && state == State.END )
		{
			// TYPE 02 FORMATION 작성 중지
			if ( enemyMakerType02Formation )
			{
				enemyMakerType02Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// TYPE 01 의 작성량을 되돌린다.
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
				// 총을 쏠 수 있도록 한다.
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetCanShoot( true );
			}

            // 다음 스테이지로
			stage = Stage.PATROL3;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 탐색병(3)
	// ------------------------------------------------------------------------
	private void StagePatrol3Start()
	{
		if ( stage == Stage.PATROL3 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// TYPE 03 작성 시작
			if ( enemyMakerType03 )
			{
				enemyMakerType03.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE03
					] );
			}
			
			// TYPE 04 작성 시작 
			if ( enemyMakerType04 )
			{
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 1 );
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );

                //서브 스크린에 메세지 출력
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"PATROL SHIP IS COMING AHEAD." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 탐색병(3) 종료
	// ------------------------------------------------------------------------
	private void StagePatrol3End()
	{
		if ( stage == Stage.PATROL3 && state == State.END )
		{
			다음 스테이지로
			stage = Stage.ATTACK3;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 제 3 공격 
	// ------------------------------------------------------------------------
	private void StageAttack3Start()
	{
		if ( stage == Stage.ATTACK3 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// TYPE 01 의 작성량을 늘린다.
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
			}

            // TYPE 02 의 작성량을 늘린다.
			if ( enemyMakerType02 )
			{
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE02
					] );
				// 스피드를 올린다.
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetAddToSpeed( true );
			}
			
			// TYPE 03 FORMATION 작성 시작
			if ( enemyMakerType03Formation )
			{
				enemyMakerType03Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE03FORMATION
					] );
				enemyMakerType03Formation.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );

                // 서브 스크린에 메세지 출력
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"BATTLE STATIONS HAS APPROACHED." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}

            // 스테이지 종료 시간을 설정
			StartCoroutine( WaitAndUpdateState( 20f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 제 3 공격 종료
	// ------------------------------------------------------------------------
	private void StageAttack3End()
	{
		if ( stage == Stage.ATTACK3 && state == State.END )
		{
			// TYPE 03 FORMATION 작성 중지
			if ( enemyMakerType03Formation )
			{
				enemyMakerType03Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// TYPE 01 작성 중지
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}

            // TYPE 02 작성 중지
			if ( enemyMakerType02 )
			{
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}

            // TYPE 03 작성 중지
			if ( enemyMakerType03 )
			{
				enemyMakerType03.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}

            // 다음 스테이지로
			stage = Stage.SILENCE;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 스테이지 - 잠시 정적.
	// ------------------------------------------------------------------------
	private void SilenceStart()
	{
		if ( stage == Stage.SILENCE && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;

            // 서브 스크린에 메세지 출력
			subScreenMessage.SendMessage("SetMessage"," ");
			subScreenMessage.SendMessage( "SetMessage", "CAUTION!" );
			subScreenMessage.SendMessage(
				"SetMessage",
				"CAUGHT HIGH ENERGY REACTION AHEAD." );
			subScreenMessage.SendMessage("SetMessage"," ");

            // 스테이지 종료 시간을 설정
			StartCoroutine( WaitAndUpdateState( 10f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - 잠시 정적 종료
	// ------------------------------------------------------------------------
	private void SilenceEnd()
	{
		if ( stage == Stage.SILENCE && state == State.END )
		{
            // 다음 스테이지로
			stage = Stage.BOSS;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 스테이지 - BOSS전 시작.
	// ------------------------------------------------------------------------
	private void BossAttackStart()
	{
		if ( stage == Stage.BOSS && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// BOSS 작성
			if ( boss )
			{
				boss.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 1 );
				boss.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );

                // 서브 스크린에 메세지 출력
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"ACKNOWKEDGED SPIDER-TYPE" );
				subScreenMessage.SendMessage(
					"SetMessage",
					"THE LIMITED-WARFARE ATTACK WEAPON." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // 스테이지 - BOSS전 종료
	// ------------------------------------------------------------------------
	private void BossAttackEnd()
	{
		if ( stage == Stage.BOSS && state == State.END )
		{
            // 다음 스테이지로
			stage = Stage.GAMECLEAR;
			state = State.INITIALIZE;
		}
	}

	// ------------------------------------------------------------------------
	// 스테이지 - GameClear.
	// ------------------------------------------------------------------------
	private void GameClearWait()
	{
		if ( stage == Stage.GAMECLEAR && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// BOSS전 후 대기 시간을 설정
			StartCoroutine( WaitAndUpdateState( 2f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// SCORE가 HISCORE를 초과한 경우 저장한다.
	// ------------------------------------------------------------------------
	private bool SetHiscore()
	{
		int hiScore = int.Parse(
			GameObject.FindGameObjectWithTag("HighScore").GetComponent<GUIText>().text );
		int score = int.Parse(
			GameObject.FindGameObjectWithTag("Score").GetComponent<GUIText>().text );
		
		if ( score > hiScore )
		{
			// 하이 스코어 갱신    
			GameObject.FindGameObjectWithTag("HighScore").GetComponent<GUIText>().text = score.ToString();
			
			// 글로벌 영역에 저장한다.
			GlobalParam.GetInstance().SetHiScore( score );
			
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 스테이지 - GameClear.
	// ------------------------------------------------------------------------
	private void GameClearMessage()
	{
		if ( stage == Stage.GAMECLEAR && state == State.END )
		{
			stage = Stage.END;
				
			// 하이 스코어 갱신
			bool isHiScore = SetHiscore();
			
			// 게임오버 문자를 표시
			GameObject gameOver = Instantiate( GameOver, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
			gameOver.SendMessage( "SetIsHiScore", isHiScore );
			gameOver.SendMessage( "SetDefeatedBoss", true );
			gameOver.SendMessage( "Show" );
			
			// 오프닝의 클릭 이벤트를 유효화한다.
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ClickToOpening>().enabled = true;
			
			// 일정시간 후에 오프닝 화면으로 돌아온다.
			StartCoroutine( WaitAndCallScene( 5f, "ending" ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 지정한 시간 경과 후에 씬을 불러온다.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndCallScene( float waitForSeconds, string sceneName )
	{
		// 지정한 시간을 기다린다.
		yield return new WaitForSeconds( waitForSeconds );
		
		// 게임 씬을 불러온다.
		Application.LoadLevel( sceneName );
	}
	
	// ------------------------------------------------------------------------
	// 스테이지 상태를 갱신
	// ------------------------------------------------------------------------
	public void SetStateEnd( int stageIndex )
	{
		// 스테이지가 다른처리로 갱신되어 있는 않는 경우에만 갱신한다.
		if ( stageIndex == (int)stage )
		{
			state = State.END;
		}
	}
	
	// ------------------------------------------------------------------------
	// 스테이지 종료를 기다리는 처리
	//  일정 시간 경과 후에 스테이지 상태를 갱신
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateState( float waitForSeconds, State state )
	{
		Stage tmpStage = stage;

        // 지정한 시간을 기다린다.
		yield return new WaitForSeconds( waitForSeconds );
		
		// 스테이지가 바뀌지 않은 경우에만 처리한다.
		if ( tmpStage == stage )
		{
			// 스테이지 상태를 갱신
			this.state = state;
		}
	}
	
	// ------------------------------------------------------------------------
	// DEBUG
	// ------------------------------------------------------------------------
	
	// 스테이지 제목을 전송한다.
	public string GetStage()
	{
		return stage.ToString();
	}
	
	// 스테이지를 강제적으로 변경한다.
	public void SetStage( string stage )
	{
		
		// 스테이지를 설정
		if ( stage == "START" )   {	this.stage = Stage.START; 	}
		if ( stage == "PATROL1" ) {	this.stage = Stage.PATROL1; }
		if ( stage == "ATTACK1" ) {	this.stage = Stage.ATTACK1; }
		if ( stage == "PATROL2" ) {	this.stage = Stage.PATROL2; }
		if ( stage == "ATTACK2" ) {	this.stage = Stage.ATTACK2; }
		if ( stage == "PATROL3" ) {	this.stage = Stage.PATROL3; }
		if ( stage == "ATTACK3" ) {	this.stage = Stage.ATTACK3; }
		if ( stage == "SILENCE" ) {	this.stage = Stage.SILENCE; }
		if ( stage == "BOSS" ) 	  {	this.stage = Stage.BOSS; 	}
		if ( stage == "GAMECLEAR" ){ this.stage = Stage.GAMECLEAR;}
		
		// 스테이지 상태는 초기
		this.state = State.INITIALIZE;
	}
	
	// 레벨 문자를 전송한다.
	public string GetLevelText()
	{
		return level.ToString();
	}
	
	// 레벨을 전송한다.
	public int GetLevel()
	{
		return (int)level;
	}
	
	// 레벨을 저장한다.
	public void SetLevel( int level )
	{
		this.level = (Level)level;
	}
}
