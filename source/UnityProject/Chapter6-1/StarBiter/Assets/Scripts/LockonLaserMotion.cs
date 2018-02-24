using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// LockonLaser 동작 제어
// ----------------------------------------------------------------------------
public class LockonLaserMotion : MonoBehaviour {

    public float laserSpeed = 20f;					// LockonLaser의 스피드
    public float turnRate = 5f;						// LockonLaser의 회전 비율     .
    public float turnRateAcceleration = 18.0f;		// LockonLaser회전 비율의 증가량      
	public GameObject targetEnemy;					// 타겟인 적기 
	public float power = 3;							// 공격력 

    private int lockonBonus = 1;					// LockonLaser발사시의 LockBonus                   
	
	private int targetId;							// 타겟의 인스턴스ID.
	private	GameObject scoutingLaser;				// 탐색 레이저
    private AudioMaker audioMaker;					// LockonLaser 사운드 메이커                    

    private bool isStart = false;					// LockonLaser 발사           
    private bool isClear = false;					// LockonLaser 제거    
	
	void Start () 
	{
		// 탐색 레이저의 인스턴스를 취득
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser");

        // LockonLaser 사운드 메이커의 인스턴스를 취득  
		audioMaker = GameObject.FindGameObjectWithTag("LockonLaserAudioMaker").GetComponent<AudioMaker>();
		
		// 사운드 재생
		if ( audioMaker )
		{
			audioMaker.Play( this.gameObject );
		}
	}
	
	void Update () 
	{
        // LockonLaser 발사 시작?
		if ( isStart )
		{
            // LockonLaser 전진시킨다.         
			ForwardLaser();
		
			// 타겟 소멸 확인
			IsDestroyTarget();
		}
	}
	
	// ------------------------------------------------------------------------
    // LockonLaser를 전진시킨다.ーを前進させる.
	// ------------------------------------------------------------------------
	private void ForwardLaser()
	{
		// 적기가 있을 경우에만 처리
		if ( targetEnemy )
		{
			// 敵機の方向を取得.
			Vector3 enemyPosition = targetEnemy.gameObject.transform.position;
			Vector3 relativePosition = enemyPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

            // LockonLaser의 현재 방향에서 적기의 방향으로, 지정된 비율로 회전한 후의 각도를 취득.
			float targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnRate * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
			
			// 턴 비율을 서서히 크게 한다(레이저가 적기에 닿지 않게 루프하는 상태 제어)
			turnRate += turnRateAcceleration * Time.deltaTime;
			
			// 레이저 각도를 변경
			transform.rotation = tiltedRotation;

			// 레이저를 이동한다.
			transform.Translate ( new Vector3( 0f, 0f, laserSpeed * Time.deltaTime ) );
		
		}
	}
	
	// ------------------------------------------------------------------------
	// 타겟인 적기 설정   
	// ------------------------------------------------------------------------
	private void SetTargetEnemy( GameObject targetEnemy )
	{
		this.targetEnemy = targetEnemy;
		this.targetId = targetEnemy.GetInstanceID();
		
		isStart = true;	// ロックオンレーザー射出.
	}

	// ------------------------------------------------------------------------
    // LockonLaser 발사시의 lockonBonus 설정                  
	// ------------------------------------------------------------------------
	private void SetLockonBonus( int lockonBonus )
	{
		this.lockonBonus = lockonBonus;
	}
	
	// ------------------------------------------------------------------------
	// 적기의 충돌 판정
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		
		// 적기에 닿는 경우에는 적기 파괴를 지시
		int colliderId = collider.gameObject.GetInstanceID();
		
		if ( colliderId == targetId )
		{			
			// 적기에 파괴를 지시
			isClear = true;
			collider.gameObject.SendMessage( "SetLockonBonus", lockonBonus );
			collider.gameObject.SendMessage( "SetIsBreakByLaser", power );
		}
	}
	
	// ------------------------------------------------------------------------
	// 적기가 파괴된 후의 처리.
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		
		if ( !targetEnemy || isClear )
		{
            //  LockonLaser제거           
			Destroy( this.gameObject );

            //  LockonLaser 수의 감소  
			scoutingLaser.SendMessage( "DecreaseLockonCount", targetId );
		}
		
	}

}
