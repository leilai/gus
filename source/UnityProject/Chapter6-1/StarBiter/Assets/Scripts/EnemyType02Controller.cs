using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType02Controller
//  - 「M03 적기 모델 타입02」의 동작(포메이션의 리더를 제외한 멤버)를 제어한다.
//  - 사용 방법
//    - 스크립트가 첨부된 오브젝트를 리더인 자식 오브젝트로 배치한다.
//  - 동작 방법.
//   - 플레이어의 진행방향의 왼쪽 위 또는 오른쪽 위에서 출현한다.
//    - 직진한다.
//    - 포메이션의 경우
//    - 리더가 파괴된 경우에는 다른 멤버의 Status를 ATTACK한다.
// ----------------------------------------------------------------------------
public class EnemyType02Controller : MonoBehaviour {

	public float speed = 4f;							// 적기의 스피드.
	public float secondSpeed = 6f;						// 적기의 빠른 스피드

    private bool canShoot = false;						// 총 발사 조건(true: 발사 가능).

    private GameObject player;							//  플레이어
    private BattleSpaceController battleSpaceContoller;	// 전투 공간
    private EnemyStatus enemyStatus;					// 적기의 상황

    private float distanceFromPlayerAtStart = 10.5f;		//  시작 시의 플레이어의 거리     
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

        //적기를 움직인다.
		enemyStatus.SetIsAttack( true );
	}
	
	void Update () {
	
		if ( enemyStatus.GetIsAttack() )
		{
			// 총을 쏜다.
			Shoot();
			
			// 적기를 이동한다.
			transform.Translate ( new Vector3( 0f, 0f, speed * Time.deltaTime ) );
			
			// 전투 공간의 스크롤 방향을 추가한다.
			transform.position -= battleSpaceContoller.GetAdditionPos();
		}
	}
	
	// ------------------------------------------------------------------------
	// 총을 쏘다.
	// ------------------------------------------------------------------------
	private void Shoot()
	{
		if ( !canShoot ) { return; }
		
		//
		bool isFiring = false;
		if ( this.GetComponent<ShotMaker>() )
		{
			this.GetComponent<ShotMaker>().GetIsFiring();
		}
		
		//
		if ( !isFiring )
		{
			if ( enemyStatus.GetIsPlayerBackArea() )
			{
				if ( this.GetComponent<ShotMaker>() )
				{
					this.GetComponent<ShotMaker>().SetIsFiring();
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 스피드 업.
	// ------------------------------------------------------------------------
	public void SpeedUp()
	{
		speed = secondSpeed;
	}
	
	// ------------------------------------------------------------------------
	// 총 발사를 허락한다.
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}

}
