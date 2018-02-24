using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType01ChildController
//  - 「M02 적기 모델 타입01」의 동작(포메이션의 리더를 제외한 멤버)를 제어한다.
//  - 사용 방법
//    - 스크립트가 첨부된 오브젝트를 리더인 자식 오브젝트로 배치한다.
//  - 동작 방법.
//    - 리더를 추종한다. 
//    - 리더가 파괴되는 경우 회피행동을 취한다.        
// ----------------------------------------------------------------------------
public class EnemyType01ChildController : MonoBehaviour {
	
	public float speed = 2.7f;							// 적기의 이동 스피드
	public float turnSpeed = 1f;						// 선회 스피드
	public float escapeSpeed = 3f;						// 회피시의 이동 스피드
	
	public float startDistanceToShoot = 5f;				// 총을 쏘는 범위의 시작 거리.
	public float endDistanceToShoot = 10f;				// 총을 쏘는 범위의 종료 거리 

	private bool canShoot = false;						// 총 발사 조건(true: 발사 가능).
	
	private GameObject player;							// 플레이어
	private BattleSpaceController battleSpaceContoller;	// 전투 공간
	private EnemyStatus enemyStatus;					// 적기의 상황

	void Start () {
	
		// 플레이어의 인스턴스를 취득
		player = GameObject.FindGameObjectWithTag("Player");

		// 전투 공간의 인스턴스를 취득
		battleSpaceContoller = GameObject.FindGameObjectWithTag("BattleSpace").GetComponent<BattleSpaceController>();
		
		// 적기 상황 인스턴스를 취득
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// 리더를 추종한다.
		enemyStatus.SetIsFollowingLeader( true );
	}

	void Update () {
	
		if ( enemyStatus.GetIsAttack() )
		{
			// ----------------------------------------------------------------
			// 회피한다.
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
