using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// BossController
//  - 「BOSS」의 동작을 제어한다.
//  - 사용 방법
//    - BOSS에 설정한다.
//  - 동작 방법
//    - 화면 위쪽에서 등장.
//    - 피하기->공격 전 이동->공격 순서대로 동작(반봅)
// ----------------------------------------------------------------------------
public class BossController : MonoBehaviour {
	
	public float startSpeed = 5f;						// BOSS 등장시의 스피드.
	public float turnRate = 5f;							// BOSS의 등장 타이밍의 방향 전환 비율.
	public float escapeRate = 5f;						// BOSS가 좌우로 피할 때, 현재 위치에서 이동 지점으로 이동할 경우의 비율.
	public float jumpSpeed = 30f;						// BOSS가 화면 밖으로 도망갈 때의 스피드.
	public float escapeSpeed = 5f;						// BOSS가 좌우로 피할 때의 현재 위치에서 이동 지점으로 움직이는 경우의 스피드.
	public float escapeTime = 5f;						// 피하는 시간.
	public float waitTimeBeforeAttack = 3f;				// 공격 전 대기 시간.
	public float waitTimeJustBeforeAttack = 0.7f;		// 공격 직전 대기 시간.
	public float escapeSpeedJustBeforeAttack = 4f;		// 공격 직전 거리를 둘 때의 스피드.
	public float Attack1Time = 7f;						// 공격 1의 시간.
	public float Attack2Time = 4f;						// 공격 2의 시간.
	public float Attack3Time = 3f;						// 공격 3의 시간.
	
	private GameObject player;							// 플레이어.
    private PlayerStatus playerStatus;					// PlayerStatus의 인스턴스.
	private BattleSpaceController battleSpaceContoller;	// 전투 공간.
	private float startPositionZ = -12.0f;				// 출현 위치(플레이어ー(Z=0)에서의 거리).
	private float distanceToPlayer = 7.0f;				// 플레이어와의 거리.
	private float distanceToPlayerJustBeforeAttack = 8.0f;	// 공격 직전의 플레이어와의 거리.
    private float distanceToPlayerMove1 = 4.0f;			// 플레이어와의 거리.
    private float distanceToPlayerMove2 = 5.0f;			// 플레이어와의 거리.
    private float distanceToPlayerMove3 = 4.0f;			// 플레이어와의 거리.
	
	private BulletMaker bulletMakerLeft;
	private BulletMaker bulletMakerRight;
	private LaserMaker laserMakerLeft;
	private LaserMaker laserMakerRight;
	private ShotMaker shotMaker;
	private EnemyStatusBoss enemyStatusBoss;
	private Animation bossAnimation;
	
	private Vector3 destinationPosition = Vector3.zero;	// 좌우로 피하는 장소     
	private bool isEscape = false;						// 좌우로 피하는 중
	
	private enum State
	{
		START,				// BOSS전투 시작.
		TOPLAYER,			// 플레이어를 향해 간다.
		ESCAPE1START,		// 회피 시작.
		ESCAPE1END,			// 회피중~종료.
		ESCAPE2START,
		ESCAPE2END,
		ESCAPE3START,
		ESCAPE3END,
		MOVE1START,			// 공격위치로 이동 시작.
		MOVE1END,			// 이동중~종료.
		MOVE2START,
		MOVE2END,
		MOVE3START,
		MOVE3END,
		JUSTBEFORE1START,	// 공격 직전 동작 시작.
		JUSTBEFORE1END,		// 공격 직전 동작 중~종료.
		JUSTBEFORE2START,
		JUSTBEFORE2END,
		JUSTBEFORE3START,
		JUSTBEFORE3END,
		ATTACK1START,		// 공격 시작.
		ATTACK1END,			// 공격중~종료.
		ATTACK2START,
		ATTACK2END,
		ATTACK3START,
		ATTACK3END
	}
	private State state = State.TOPLAYER;
	
	void Start () {
	
		// 플레이어의 인스턴스를 취득
		player = GameObject.FindGameObjectWithTag("Player");

        //  PlayerStatus의 인스턴스를 취득 
		playerStatus = player.GetComponent<PlayerStatus>();
		
		// BOSS의 각 파츠 취득.
		bulletMakerLeft = GameObject.Find("BossVulcanLeft").GetComponent<BulletMaker>();
		bulletMakerRight = GameObject.Find("BossVulcanRight").GetComponent<BulletMaker>();
		laserMakerLeft = GameObject.Find("BossLaserLeft").GetComponent<LaserMaker>();
		laserMakerRight = GameObject.Find("BossLaserRight").GetComponent<LaserMaker>();
		shotMaker = GameObject.Find("BossCore").GetComponent<ShotMaker>();
		enemyStatusBoss = GetComponent<EnemyStatusBoss>();
		
		// 애니메이션 취득.
		bossAnimation = GetComponent<Animation>();
		
		// 전투 공간 취득.
		battleSpaceContoller = GameObject.FindGameObjectWithTag("BattleSpace").GetComponent<BattleSpaceController>();
		
		// --------------------------------------------------------------------
		// 출현 위치 지정.
		// --------------------------------------------------------------------
		
		// 먼저 플레이어의 위치로.
		transform.position = player.transform.position;
		transform.rotation = Quaternion.Euler( 0, 180, 0 );
		
		// 위치를 조정.
		transform.Translate ( new Vector3( 0f, 0f, startPositionZ ) );
		
	}
	
	void Update () {
		
		// 플레이어와의 거리를 확인.
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		if ( state == State.TOPLAYER && distance < distanceToPlayer )
		{
			state = State.ESCAPE1START;
			destinationPosition = transform.position;
		}
		
		// 화면 위쪽에서 등장하는 처리.
		if ( state == State.TOPLAYER )
		{
			transform.position += transform.forward * startSpeed * Time.deltaTime;
		}
		
		// 플레이어가 조작 가능한 때에만 처리한다.
		if ( playerStatus.GetIsNOWPLAYING() ) {
		
			// 공격.
			Attack1();
		
			// 공격.
			Attack2();
		
			// 공격.
			Attack3();
			
			// 공격중 처리
			Attacking();
		
			// 공격중에 플레이어와의 간격을 정한다.
			SetDistanceToPlayerAtAttack();
			
			// 회피1
			Escape1();

			// 회피2
			Escape2();
			
			// 회피.
			Escape3();
	
			// 공격위치로 이동1
			Move1();

            // 공격위치로 이동2.
			Move2();

            // 공격위치로 이동3
			Move3();
			
			// 공격 진적 동작1
			MotionJustBeforeAttack1();

            // 공격 진적 동작2       
			MotionJustBeforeAttack2();

            // 공격 진적 동작3            
			MotionJustBeforeAttack3();
			
			// 공격 직전에 플레이어와의 간격을 정한다.
			SetDistanceToPlayerJustBefortAttack();
			
			// 플레이어와의 간격을 정한다.
			SetDistanceToPlayer();
			
			// 회피 행동.
			EscapeFromPlayer();
		
		}
	
		// 전투 공간의 스크롤 방향을 추가한다.
		transform.position -= battleSpaceContoller.GetAdditionPos();
		
	}

	// ------------------------------------------------------------------------
	// BOSS - 공격.
	// ------------------------------------------------------------------------
	private void Attack1()
	{
		// 공격.
		if ( state == State.ATTACK1START )
		{
            // Status를 진행한다.
			state = State.ATTACK1END;
			
			// 기체가 존재할 때에만 공격가능.
			if ( bulletMakerLeft || bulletMakerRight )
			{
				// 애니메이션 재생
				bossAnimation.Play();

                //  Vulcan 공격.
				if ( bulletMakerLeft ) bulletMakerLeft.SetIsFiring();
				if ( bulletMakerRight ) bulletMakerRight.SetIsFiring();					
				
				StartCoroutine( WaitAndUpdateState( Attack1Time, State.ESCAPE2START ) );
			}
			else
			{
				// 기체가 없는 경우 다음 공격으로 이동.
				state = State.ATTACK2START;
			}
			
		}
	}

	// ------------------------------------------------------------------------
	// BOSS - 공격.
	// ------------------------------------------------------------------------
	private void Attack2()
	{
		// 공격.
		if ( state == State.ATTACK2START )
		{
            // Status를 진행한다.  
			state = State.ATTACK2END;

            // 기체가 존재할 때에만 공격가능.
			if ( laserMakerLeft || laserMakerRight )
			{
				// 레이저 공격.
				if ( laserMakerLeft ) laserMakerLeft.SetIsFiring();
				if ( laserMakerRight ) laserMakerRight.SetIsFiring();
				
				StartCoroutine( WaitAndUpdateState( Attack2Time, State.ESCAPE3START ) );
			}
			else
			{
                // 기체가 없는 경우 다음 공격으로 이동.
				state = State.ATTACK3START;
			}
		}
	}

	// ------------------------------------------------------------------------
	// BOSS - 공격3.
	// ------------------------------------------------------------------------
	private void Attack3()
	{
		// 공격3.
		if ( state == State.ATTACK3START )
		{
			state = State.ATTACK3END;

            // shot 공격.
			if ( shotMaker ) shotMaker.SetIsFiring();
					
			StartCoroutine( WaitAndUpdateState( Attack3Time, State.ESCAPE1START ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 공격 중 처리.
	// ------------------------------------------------------------------------
	private void Attacking()
	{
		// 공격 종료 직후   
		if ( state == State.ATTACK1END ||
			 state == State.ATTACK2END ||
			 state == State.ATTACK3END )
		{
			// 플레이어의 방향을 취득.
			Vector3 relativePosition = player.transform.position - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

			// BOSS의 현재 방향에서 목적지 방향으로 지정한 스피드로 향한 후 각도를 정한다.
			float targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnRate * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
				
			// BOSS의 방향을 플레이어의 방향으로 향하게 한다.
			transform.rotation = tiltedRotation;
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 회피.
	// ------------------------------------------------------------------------
	private void Escape1()
	{
		// 회피.
		if ( state == State.ESCAPE1START )
		{
			state = State.ESCAPE1END;
			
			// 기체가 존재할 때에만 회피1을 한다.
			if ( bulletMakerLeft || bulletMakerRight )
			{
				SetEscapeTime();
				StartCoroutine( WaitAndUpdateState( escapeTime, State.MOVE1START ) );
			}
			else
			{
				// 기체가 없는 경우에는 다음 회피로 이동한다.
				state = State.ESCAPE2START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 회피.
	// ------------------------------------------------------------------------
	private void Escape2()
	{
		// 회피2
		if ( state == State.ESCAPE2START )
		{
			state = State.ESCAPE2END;

            // 기체가 존재할 때에만 회피2를 한다.
			if ( laserMakerLeft || laserMakerRight )
			{
				SetEscapeTime();
				StartCoroutine( WaitAndUpdateState( escapeTime, State.MOVE2START ) );
			}
			else
			{
                // 기체가 없는 경우에는 다음 회피로 이동한다.
				state = State.ESCAPE3START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 회피3
	// ------------------------------------------------------------------------
	private void Escape3()
	{
		// 회피3
		if ( state == State.ESCAPE3START )
		{
			state = State.ESCAPE3END;
			SetEscapeTime();
			StartCoroutine( WaitAndUpdateState( escapeTime, State.MOVE3START ) );			
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 공격 위치로 이동1
	// ------------------------------------------------------------------------
	private void Move1()
	{
		// 이동.
		if ( state == State.MOVE1START )
		{
			state = State.MOVE1END;
			
			// 기체가 존재할  때에만 이동1을 한다.
			if ( bulletMakerLeft || bulletMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeBeforeAttack, State.JUSTBEFORE1START ) );
			}
			else
			{
				// 기체가 존재하지 않는 경우에는 다음 이동으로 진행한다.
				state = State.MOVE2START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // BOSS - 공격 위치로 이동2
	// ------------------------------------------------------------------------
	private void Move2()
	{
		// 이동.
		if ( state == State.MOVE2START )
		{
			state = State.MOVE2END;

            // 기체가 존재할  때에만 이동2를 한다.
			if ( laserMakerLeft || laserMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeBeforeAttack, State.JUSTBEFORE2START ) );
			}
			else
			{
                // 기체가 존재하지 않는 경우에는 다음 이동으로 진행한다.
				state = State.MOVE3START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // BOSS - 공격 위치로 이동3
	// ------------------------------------------------------------------------
	private void Move3()
	{
		// 이동3.
		if ( state == State.MOVE3START )
		{
			state = State.MOVE3END;
			isEscape = false;
			StartCoroutine( WaitAndUpdateState( waitTimeBeforeAttack, State.JUSTBEFORE3START ) );			
		}
	}
	
	
	// ------------------------------------------------------------------------
	// BOSS - 공격 직전 동작1
	// ------------------------------------------------------------------------
	private void MotionJustBeforeAttack1()
	{
        // 공격 직전 동작1
		if ( state == State.JUSTBEFORE1START )
		{
			state = State.JUSTBEFORE1END;
			
			// 기체가 존재할 때에만 공격 직전 동작 1을 한다.          
			if ( bulletMakerLeft || bulletMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeJustBeforeAttack, State.ATTACK1START ) );
			}
			else
			{
				// 기체가 존재하지 않은 경우에는 다음 공격 직전 동작으로 이동한다.
				state = State.JUSTBEFORE2START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // BOSS - 공격 직전 동작2
	// ------------------------------------------------------------------------
	private void MotionJustBeforeAttack2()
	{
        // 공격 직전 동작2
		if ( state == State.JUSTBEFORE2START )
		{
			state = State.JUSTBEFORE2END;

            // 기체가 존재할 때에만 공격 직전 동작 2를 한다.          
			if ( laserMakerLeft || laserMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeJustBeforeAttack, State.ATTACK2START ) );
			}
			else
			{
                //  기체가 존재하지 않은 경우에는 다음 공격 직전 동작으로 이동한다.
				state = State.JUSTBEFORE3START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // BOSS - 공격 직전 동작3
	// ------------------------------------------------------------------------
	private void MotionJustBeforeAttack3()
	{
        // 공격 직전 동작3
		if ( state == State.JUSTBEFORE3START )
		{
			isEscape = false;
			state = State.ATTACK3START;
		}
	}
	
	// ------------------------------------------------------------------------
	// 플레이어와의 간격을 갖는다.
	// ------------------------------------------------------------------------
	private void SetDistanceToPlayer()
	{
        // 플레이어와의 간격을 갖는다.
		if (
			state == State.MOVE1END ||
			state == State.MOVE2END ||
			state == State.MOVE3END )
		{
			// 플레이어의 방향 취득.
			Vector3 relativePosition = player.transform.position - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

			// BOSS의 현재 방향에서 목적지의 방향으로, 지정한 스피드로 향한 후의 각도를 취득.
			float targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnRate * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
				
			// BOSS의 방향을 플레이어를 향하게 한다.
			transform.rotation = tiltedRotation;
			
			// 플레이어와의 간격을 갖도록 한다.
			float tmpDistanceToPlayer = 0;
			if ( state == State.MOVE1END ) { tmpDistanceToPlayer = distanceToPlayerMove1; }
			if ( state == State.MOVE2END ) { tmpDistanceToPlayer = distanceToPlayerMove2; }
			if ( state == State.MOVE3END ) { tmpDistanceToPlayer = distanceToPlayerMove3; }
			float distance5 = Vector3.Distance(
				player.transform.position,
				transform.position );

			if ( distance5 < tmpDistanceToPlayer )
			{
				transform.position -= transform.forward * Time.deltaTime * ( tmpDistanceToPlayer - distance5 ) * 2;
			}
			else
			{
				transform.position += transform.forward * Time.deltaTime * ( distance5 - tmpDistanceToPlayer ) * 2;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 공격 직전 플레이어와의 거리를 갖는다.
	// ------------------------------------------------------------------------
	private void SetDistanceToPlayerJustBefortAttack()
	{
		// 공격 중에 플레이어와의 거리를 갖는다.
		if (
			state == State.JUSTBEFORE1END ||
			state == State.JUSTBEFORE2END ||
			state == State.JUSTBEFORE3END )
		{
			
			// 플레이어와 BOSS의 거리를 갖는다.
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			// BOSS를 플레이어에 향하게 한다.
			Vector3 playerRelativePositionByBoss = player.transform.position - transform.position;
			Quaternion playerTargetRotationByBoss = Quaternion.LookRotation( playerRelativePositionByBoss );
			transform.rotation = playerTargetRotationByBoss;

			// 플레이어와의 거리를 갖는다.
			if ( distance < distanceToPlayerJustBeforeAttack )
			{
				transform.position -=
					transform.forward
						* Time.deltaTime
						* ( distanceToPlayerJustBeforeAttack - distance )
						* escapeSpeedJustBeforeAttack;
			}
			else
			{
				transform.position +=
					transform.forward
						* Time.deltaTime
						* ( distance - distanceToPlayerJustBeforeAttack )
						* escapeSpeedJustBeforeAttack;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 공격 중에 플레이어와의 거리를 갖는다.
	// ------------------------------------------------------------------------
	private void SetDistanceToPlayerAtAttack()
	{
        // 공격 중에 플레이어와의 거리를 갖는다.
		if (
			state == State.ATTACK1END ||
			state == State.ATTACK3END )
		{
			
			// 플레이어와 BOSS의 거리를 취득.
            float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			// BOSS를 플레이어에게 향하게 한다                      
			Vector3 playerRelativePositionByBoss = player.transform.position - transform.position;
			Quaternion playerTargetRotationByBoss = Quaternion.LookRotation( playerRelativePositionByBoss );
			transform.rotation = playerTargetRotationByBoss;
	
			// 플페이어와의 거리를 갖는다.
			if ( distance < distanceToPlayer )
			{
				transform.position -= transform.forward * Time.deltaTime * ( distanceToPlayer - distance ) * 2;
			}
			else
			{
				transform.position += transform.forward * Time.deltaTime * ( distance - distanceToPlayer ) * 2;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 회피 동작.
	// ------------------------------------------------------------------------
	private void EscapeFromPlayer()
	{
		// 록온레이저에 닿지 않도록 움직인다.
		if (
			state == State.ESCAPE1START ||
			state == State.ESCAPE1END ||
			state == State.ESCAPE2START ||
			state == State.ESCAPE2END ||
			state == State.ESCAPE3START ||
			state == State.ESCAPE3END )
		{
			// BOSS가 회피중인가 아닌가.
			if ( !isEscape )
			{
				// 플레이어와의 거리를 확인한다.
				GetDestinationPosition();
			}
			else
			{
				// BOSS가 바라본 이동지점의 각도를 취득.
				Vector3 destinationRelativePositionByBoss =
					destinationPosition - transform.position;
				Quaternion destinationTargetRotationByBoss =
					Quaternion.LookRotation( destinationRelativePositionByBoss );

				// BOSS의 진행 방향을 이동지점을 향하게 한다.
				transform.rotation = destinationTargetRotationByBoss;

				// BOSS를 진행 방향으로 이동시킨다.
				float distanceToDestination = Vector3.Distance(
					destinationPosition,
					transform.position );
				transform.Translate ( new Vector3( 0f, 0f, distanceToDestination * escapeSpeed * Time.deltaTime ) );

				// 이동 후의 위치에서 이동 지점까지의 거리를 구한다.
				distanceToDestination = Vector3.Distance(
					destinationPosition,
					transform.position );
				
				// 이동 지점에 도착한 때에 회피 동작을 종료한다.
				if ( distanceToDestination < 1f )
				{
					isEscape = false;
				}
				
				// BOSSをプレイヤーの方向に向ける.
				Vector3 relativePosition = player.transform.position - transform.position;
				Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
				transform.rotation = targetRotation;
			}
		}
	}

	// ------------------------------------------------------------------------
	// 회피시 이동지점을 취득.
	// ------------------------------------------------------------------------
	public void GetDestinationPosition()
	{
		// 플레이어의 각도.
		float playerAngle = player.transform.eulerAngles.y;
		
		// 플레이어가 바라보는 BOSS의 각도.
		Vector3 bossRelativePositionByPlayer = transform.position - player.transform.position;
		float bossAngleByPlayer = Quaternion.LookRotation( bossRelativePositionByPlayer ).eulerAngles.y;

		// 각도 수정.
		if ( Mathf.Abs ( playerAngle - bossAngleByPlayer ) > 180f )
		{
			// 0도 <-> 359도를 넘는다고 간주하여 +360도 한다.
			if ( playerAngle < 180f )
			{
				playerAngle += 360f;
			}
			if ( bossAngleByPlayer < 180f )
			{
				bossAngleByPlayer += 360f;
			}
		}
		
		// 플레이어의 진행 방향의 각도에서 일정 이상 떨어져 잇는 경우에는 피히지 않는다.
		if ( Mathf.Abs ( playerAngle - bossAngleByPlayer ) > 45f )
		{
			// ----------------------------------------------------------------
			//  플레이어와의 거리를 갖는다.
			// ----------------------------------------------------------------
			
			// 플레이어와 BOSS의 거리를 취득.
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			// BOSS를 플레이어에게 향하게 한다.
			Vector3 playerRelativePositionByBoss = player.transform.position - transform.position;
			Quaternion playerTargetRotationByBoss = Quaternion.LookRotation( playerRelativePositionByBoss );
			transform.rotation = playerTargetRotationByBoss;
	
			// 플레이어와의 간격을 갖는다.
			if ( distance < distanceToPlayer )
			{
				transform.position -= transform.forward * Time.deltaTime * ( distanceToPlayer - distance ) * 2;
			}
			else
			{
				transform.position += transform.forward * Time.deltaTime * ( distance - distanceToPlayer ) * 2;
			}

			return;
		}
		
		// ----------------------------------------------------------------
		// 회피 지점을 구한다.
		// ----------------------------------------------------------------
		
		// 플레이어를 중심으로 BOSS의 회피 지점의 각도를 구한다.
		float transformAngle = Random.Range( -45f, 45f );
		if ( transformAngle > 0 )
		{
			transformAngle += 50f;
		}
		else
		{
			transformAngle -= 50f;
		}
		float targetRotationAngle = bossAngleByPlayer;
		targetRotationAngle += transformAngle;
		
		// 회피 지점의 위치를 설정.
		destinationPosition =
			Quaternion.AngleAxis( targetRotationAngle, Vector3.up )
				* Vector3.forward * distanceToPlayer;
		
		// 회피 시작.
		isEscape = true;
		
	}
	
	// ------------------------------------------------------------------------
	// BOSS의 상태를 갱신.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateState( float waitForSeconds, State state )
	{
		//대기
		yield return new WaitForSeconds( waitForSeconds );

        // state 갱신
		this.state = state;
	}
	
	// ------------------------------------------------------------------------
	// life잔량에 맞추어 피하는 시간을 짧게 한다.
	// ------------------------------------------------------------------------
	private void SetEscapeTime()
	{
		int life = enemyStatusBoss.GetLife();
		if ( life > 5 )
		{
			return;
		}
		switch ( life )
		{
		    case 1:
		        escapeTime = 0.5f;
				waitTimeBeforeAttack = 0.5f;
		        break;
		    case 2:
			case 3: 
		        escapeTime = 1f;
				waitTimeBeforeAttack = 1f;
		        break;
		    case 4:
			case 5: 
		        escapeTime = 2f;
				waitTimeBeforeAttack = 2f;
		        break;
		}
	}
}