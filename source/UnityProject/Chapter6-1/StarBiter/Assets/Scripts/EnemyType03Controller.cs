using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType03Controller
//  - 「M04 적기 모델 타입 03」의 동작(포메이션의 리더를 제외한 멤버)를 제어한다.
//  -사용 방법
//    - 스크립트가 첨부된 오브젝트를 리더인 자식 오브젝트로 배치한다.
// - 동작 방법.
//    - 플레이어의 진행 방향과 반대 방향에서 출현한다.
//    - 플레이어의 일정거리까지 근접한다.
//    - 총 발사 후, 화면 밖으로 도망친다.
// ----------------------------------------------------------------------------
public class EnemyType03Controller : MonoBehaviour {

    public float speed = 6f;							// 적기의 스피드.
    public float speedUTurn = 6f;						// U턴 중의 스피드.
    public float turnSpeed = 5f;						// 적기의 방향 전환 스피드.

    private bool canShoot = false;						// 총 발사 조건(true: 발사 가능).

    private GameObject player;							// 플레이어
    private EnemyStatus enemyStatus;					// 적기의 상황

    private float distanceToUTurnPoint = 5.0f;			// U턴 하는 플레이어까지의 거리.
	private float distanceFromPlayerAtStart = 9.5f;		// 시작시의 플레이어부터의 거리.
	private enum State
	{
		FORWARD,	// 이동
		STAY,		// 정지
		UTURN		// U턴
	}
    private State state = State.FORWARD;				// 적기의 행동 상황
	
	void Start () {

        // 플레이어의 인스턴스를 취득
		player = GameObject.FindGameObjectWithTag("Player");

        //적기 상황 인스턴스를 취득
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// --------------------------------------------------------------------
        // 출현 위치 지정
		// --------------------------------------------------------------------

        // 발생 방향 계산.(플레이어의 진행 방향과 반대쪽)
		float playerAngleY = player.transform.rotation.eulerAngles.y + 180;
		float additionalAngle = (float)Random.Range( -90, 90 );

        // 방향을 설정
		transform.rotation = Quaternion.Euler( 0f, playerAngleY + additionalAngle, 0f );

        // 위치를 설정
		transform.position = new Vector3( 0, 0, 0 );
		transform.position = transform.forward * distanceFromPlayerAtStart;

        // 진행방향을 플레이어에게 향하게 한다.
		Vector3 playerPosition = player.transform.position;
		Vector3 relativePosition = playerPosition - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
		transform.rotation = targetRotation;

        // 적기를 움직인다.
		enemyStatus.SetIsAttack( true );
	}
	
	void Update () {
	
		if ( enemyStatus.GetIsAttack() )
		{
			if ( state == State.UTURN )
			{
				// ----------------------------------------------------------------
                //적기의 진행 방향을 정한다.
				// ----------------------------------------------------------------

                // 플레이어의 방향을 취득
				Vector3 playerPosition = player.transform.position;
				Vector3 relativePosition = playerPosition - transform.position;
				Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

                // 적기의 현재 방향에서 플레이어와 반대 방향으로 지정한 스피드로 향한 후의 각도를 취득.
				float targetRotationAngle = targetRotation.eulerAngles.y - 180;
				float currentRotationAngle = transform.eulerAngles.y;
				currentRotationAngle = Mathf.LerpAngle(
					currentRotationAngle,
					targetRotationAngle,
					turnSpeed * Time.deltaTime );
				Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );

                // 적기의 각도를 변경            
				transform.rotation = tiltedRotation;

                // 적기를 이동한다.
				transform.Translate ( new Vector3( 0f, 0f, speedUTurn * Time.deltaTime ) );
			}
			
			if ( state == State.FORWARD )
			{
                // 적기를 이동한다.
				transform.Translate ( new Vector3( 0f, 0f, speed * Time.deltaTime ) );

                // 어느 정도의 거리까지 가까운 겨우에는 일정 시간 정지한다.
				float distance = Vector3.Distance(
					player.transform.position,
					transform.position );
				if ( distance < distanceToUTurnPoint )
				{
					state = State.STAY;
					StartCoroutine( WaitAndUTurn( 3f ) );
				}
			}
			
			if ( state == State.STAY )
			{
                // 정지중에 rigidbody에 의한 충돌판정을 유효화하기 위해 동작을 되돌린다.
                // ※Project Settings->Physics 의Sleep Velocity 보다 큰 값으로 한다.
				transform.Translate ( new Vector3( 0f, 0f, 0.2f ) );
				transform.Translate ( new Vector3( 0f, 0f, -0.2f ) );
			}
		}
	}

	// ------------------------------------------------------------------------
    // U턴까지의 대기 처리.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUTurn( float waitForSeconds )
	{
		yield return new WaitForSeconds( waitForSeconds );
		state = State.UTURN;
		SetFire();
	}
	
	// ------------------------------------------------------------------------
    // 총을 발사한다.
	// ------------------------------------------------------------------------
	private void SetFire()
	{
		if ( !canShoot ) return;
		
		bool isFiring = false;
		if ( this.GetComponent<ShotMaker>() )
		{
			this.GetComponent<ShotMaker>().GetIsFiring();
		}
		if ( !isFiring )
		{
			if ( this.GetComponent<ShotMaker>() )
			{
				this.GetComponent<ShotMaker>().SetIsFiring();
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // 총의 발사를 허락한다.
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}
}
