using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType01Controller
//  - 「M02 적기 모델 타입01」의 동작(단독or포메이션의 리더)를 제어한다.
//  - 사용 방법.
//    - 스크립트를 오브젝트에 첨부한다.
//  - 동작 방법
//    - 플레이어의 진행방향의 왼쪽 위 또는 오른쪽 위에서 출현한다.
//    - 플레이어의 중심을 향해 간다.
//    - 포메이션의 경우   
//      - 리더가 파괴된 경우에는 다른 멤버의 Status를 ATTACK한다.
// ----------------------------------------------------------------------------
public class EnemyType01Controller : MonoBehaviour {

    public float speed = 2.7f;							// 적기의 이동 스피드
    public float turnSpeed = 1f;						// 선회 스피드

    public float startDistanceToShoot = 5f;				// 총을 쏘는 범위의 시작 거리.
    public float endDistanceToShoot = 8f;				// 총을 쏘는 범위의 종료 거리 

    private bool canShoot = false;						// 총 발사 조건(true: 발사 가능).

    private GameObject player;							// 플레이어
    private BattleSpaceController battleSpaceContoller;	// 전투 공간
    private EnemyStatus enemyStatus;					// 적기의 상황
	
	private float distanceFromPlayerAtStart = 9.5f;		// 시작 시의 플레이어의 거리     
	
	void Start () {
	
		// 플레이어의 인스턴스를 취득
		player = GameObject.FindGameObjectWithTag("Player");

		// 전투 공간의 인스턴스를 취득
		battleSpaceContoller = GameObject.FindGameObjectWithTag("BattleSpace").GetComponent<BattleSpaceController>();

        // 적기 상황 인스턴스를 취득
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// --------------------------------------------------------------------
		// 출현 위치 지정
		// --------------------------------------------------------------------
		
		// 발생 방향 계산.(플레이어의 각도에서 플러스, 마이너스 45도).
		float playerAngleY = player.transform.rotation.eulerAngles.y;
		float additionalAngle = (float)Random.Range( -45, 45 );
		
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
            // 플레이어의 방향을 취득
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

            // 적기의 현재 방향에서 플레이어와 반대 방향으로 지정한 스피드로 향한 후의 각도를 취득.
			float targetRotationAngle;
			targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnSpeed * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );

            // 적기의 각도를 변경            
			transform.rotation = tiltedRotation;

            // 적기를 이동한다.
			transform.Translate ( new Vector3( 0f, 0f, speed * Time.deltaTime ) );

            // 전투 공간의 스크롤 방향을 추가한다.
			transform.position -= battleSpaceContoller.GetAdditionPos();

            // 총 발사 확인.
			if ( canShoot )
			{
				IsFireDistance();
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // 공격 대상의 거리
	// ------------------------------------------------------------------------
	private void IsFireDistance()
	{
		bool isFiring = false;
		if ( this.GetComponent<ShotMaker>() )
		{
			isFiring = this.GetComponent<ShotMaker>().GetIsFiring();
			if ( !isFiring )
			{
				if ( IsInRange( startDistanceToShoot, endDistanceToShoot ) )
				{
					this.GetComponent<ShotMaker>().SetIsFiring();
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // 범위 내인지 아닌지
	// ------------------------------------------------------------------------
	private bool IsInRange( float fromDistance, float toDisRance )
	{
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		
		if ( distance >= fromDistance && distance <= toDisRance )
		{
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
    // 총 발사를 허락한다.
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}
}
