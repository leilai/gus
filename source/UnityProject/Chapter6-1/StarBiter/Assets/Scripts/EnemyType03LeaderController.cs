using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType03LeaderController
//  - 「M04 적기 모델 타입03」의 동작(포메이션의 리더를 제외한 멤버)를 제어한다.
//  -사용 방법
//    - 스크립트를 오브젝트에 첨부한다.
//  - 동작 방법.
//    - 플레이어를 중심으로 전방에서 출현한다.
//    - 플레이어의 일정거리까지 근접한다.
//    - 총 발사 후, 화면 밖으로 도망친다.
//    - 포메이션의 경우
//    -리더가 파괴된 경우에는 다른 멤버의 Status를 ATTACK한다.
// ----------------------------------------------------------------------------
public class EnemyType03LeaderController : MonoBehaviour {

    public float speed = 6f;							// 적기의 스피드.
    public float speedUTurn = 6f;						// U턴 중의 스피드.
    public float turnSpeed = 5f;						// 적기의 방향 전환 스피드.

    public bool canShoot = false;						// 총 발사 조건(true: 발사 가능).
    private GameObject player;							// 플레이어
    private EnemyStatus enemyStatus;					// 적기의 상황

    private float distanceToUTurnPoint = 5.0f;			//U턴 하는 플레이어까지의 거리.
	private enum State
	{
		FORWARD,	// 이동
		STAY,		// 정지.
		UTURN		// U턴
	}
    private State state = State.FORWARD;				//적기의 행동 상황
		
	void Start () {

        // 플레이어의 인스턴스를 취득
		player = GameObject.FindGameObjectWithTag("Player");

        // 적기 상황 인스턴스를 취득
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// --------------------------------------------------------------------
        // 출현 위치 지정
		// --------------------------------------------------------------------

		// 부모를 취득
		GameObject tmpParent = transform.parent.gameObject;
	
		// 포메이션을 회전한다.
		float tmpAngle = Random.Range( 0f, 360f );
		tmpParent.transform.rotation = Quaternion.Euler( 0, tmpAngle, 0 );
	
		// 부모가 가진 EnemyMaker를 복사한다.
		GameObject tmpEnemyMaker = tmpParent.GetComponent<EnemyStatus>().GetEmenyMaker();
		enemyStatus.SetEmenyMaker( tmpEnemyMaker );

		// ----------------------------------------------------------------
        // 적기의 진행 방향을 정한다.
		// ----------------------------------------------------------------

        // 플레이어의 방향을 취득
		Vector3 playerPosition = player.transform.position;
		Vector3 relativePosition = playerPosition - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

        // 적기의 각도를 변경       
		transform.rotation = targetRotation;
	
		// 적기를 움직인다.
		enemyStatus.SetIsAttack( true );
		tmpParent.GetComponent<EnemyStatus>().SetIsAttack( true );
	}
	
	void Update () {
	
		if ( enemyStatus.GetIsAttack() )
		{
			if ( state == State.UTURN )
			{
				// ----------------------------------------------------------------
                // 적기의 진행 방향을 정한다.
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

                //적기의 각도를 변경            
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
    //U턴까지의 대기 처리.
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
