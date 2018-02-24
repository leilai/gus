using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 탐색 레이저 제어
// ----------------------------------------------------------------------------
public class ScoutingLaser : MonoBehaviour {
	
	public bool isScanOn = false;				// 탐색 시작
	
	public GameObject lockonLaser;

    public GameObject lockonSight1;				//  lockonSight1.
    public GameObject lockonSight2;				//  lockonSight2.
    public GameObject lockonSight3;				//  lockonSight3.
    public GameObject lockonSight4;				//  lockonSight4.
    public GameObject lockonSight5;				//  lockonSight5.
    public GameObject lockonSight6;				//  lockonSight6.
	
	public float ScoutingLaserTurnRate = 15f;	// 플레이어의 방향에 맞추어 탐색 레이저가 회전하는 비율
	public float ScoutingLaserFowardPosition = 5.5f;	// 탐색 레이저의 시작위치(플레이에가 바라보는 전방)

    private GameObject[] lockonSights;			//  lockonSight1～6.
	
	private GameObject player;					// 플레이어
	private GameObject scoutingLaserMesh;		// 탐색 레이저의 콜리전 보완
	private MeshRenderer scoutingLaserLine;		// 플레이어의 전방 방향에 표시할 탐색 레이저
	private GameObject lockBonus;				// LockBonus.
	private GameObject lockSlot;				// LockSlot.
	private GameObject subScreenMessage;		// SubScreen의 메세지 영역

    private int lockonCount = 0;				// lockon 수

    private static int MAX_LOCKON_COUNT = 6;	// lockon 최대수    

    private GameObject[] lockedOnEnemys;		// lockon한 적기        
    private int[] lockedOnEnemyIds;				// lockon한 적기의 인스턴스 ID.
    private int[] lockonLaserIds;				// lockon한 레이저의 인스턴스ID.
    private GameObject[] lockedOnSights;		// 적기를 lockon한 lockedOnSights.

    private float[] lockonLaserStartRotation =	// locked 레이저 발사시의 각도  
	{
		-40f, 40f, -70f, 70f, -100f, 100f
	};
	
	private int	invalidInstanceId = -1;		// lockedOnEnemyIds[] 가 미사용인 경우를 나타내는 값
	
	void Start () 
	{
        // lockedOn 적기
		lockedOnEnemys = new GameObject[MAX_LOCKON_COUNT];
		lockedOnEnemyIds = new int[MAX_LOCKON_COUNT];
		lockonLaserIds = new int[MAX_LOCKON_COUNT];
		lockedOnSights = new GameObject[MAX_LOCKON_COUNT];

        // lockedOnSights의 인스턴스를 취득           
		lockonSights = new GameObject[MAX_LOCKON_COUNT];
		lockonSights[0] = lockonSight1;
		lockonSights[1] = lockonSight2;
		lockonSights[2] = lockonSight3;
		lockonSights[3] = lockonSight4;
		lockonSights[4] = lockonSight5;
		lockonSights[5] = lockonSight6;
		
		// player의 인스턴스를 취득
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 탐색 레이저의 콜리전 보완을 취득
		scoutingLaserMesh = GameObject.FindGameObjectWithTag("ScoutingLaserMesh");

		// 탐색 레이저의 전방 선의 인스턴스를 취득    
		scoutingLaserLine = GameObject.FindGameObjectWithTag("ScoutingLaserLine").GetComponent<MeshRenderer>();

        // lockedOnSights의 초기화         
		this.GetComponent<TrailRenderer>().enabled = isScanOn;
		scoutingLaserLine.enabled = isScanOn;
		
		// LockBonus의 인스턴스를 취득
		lockBonus = GameObject.FindGameObjectWithTag("LockBonus");
		
		// LockSlot의 인스턴스를 취득
		lockSlot = GameObject.FindGameObjectWithTag("LockSlot");
		
		// SubScreenMessage의 인스턴스를 취득
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
		// lockedOnEnemyIds[] 가 미사용인 경우를 나타내는 값
		// 자기자신이 록온되는 경우는 존재하지 않으므로
		invalidInstanceId = this.gameObject.GetInstanceID();

		for(int i = 0;i < lockedOnEnemyIds.Length;i++) {

			lockedOnEnemyIds[i] = invalidInstanceId;
			lockonLaserIds[i] = invalidInstanceId;
		}
		
	}
	
	void Update ()
	{
	
		// 탐색 레이저 정지시 록온한 적기에 록온 레이저 발사
        if ( isScanOn == false ) 
		{
			StartLockonLaser();
		}
		
		// 탐색 레이저의 위치를 설정              
		UpdateTransformMesh();

	}

	// ------------------------------------------------------------------------
	// 탐색 레이저의 유효무효 교체
	//  - 탐색 레이저의 시작 시간 대기가 없는 버젼.
	// ------------------------------------------------------------------------	
	public void SetIsScanOn( bool isScanOn )
	{		
		// 탐색 레이저의 (유효/ 무효) 교체
		this.isScanOn = isScanOn;
		
		// 탐색 레이저의 (표시/ 비표시) 교체
		this.GetComponent<TrailRenderer>().enabled = isScanOn;
		scoutingLaserLine.enabled = isScanOn;
		
		// 메세지 표시
		if ( isScanOn == true )
		{
			StartCoroutine( "SetSearchingMessage" );
		}
		else
		{
			StopCoroutine( "SetSearchingMessage" );
		}
		
		// 탐색 레이저 사운드
		if ( isScanOn == true )
		{
			this.audio.Play();
		} 
		else
		{
			this.audio.Stop();
		}
	}
	IEnumerator SetSearchingMessage()
	{
		yield return new WaitForSeconds( 0.5f );
		subScreenMessage.SendMessage( "SetMessage", "SEARCHING ENEMY..." );
	}
	
	// ------------------------------------------------------------------------
	// 탐색 레이저의 위치 설정 MeshCollider 버전
	//  - 항상 플레이어의 전방에 표시.
	//  - 탐색 레이저의 스피드 조정(느리게)을 한다. 
	//    - 플레이어의 방향을 바꾸는 스피드가 빠른 경우에 발생하는 아래의 문제점을 피하기 위해
	//      - 닿지 않는 위치가 생기게 된다. 
	//      - TrailRenderer가 원만한 원으로 되어 있지 않다.
    //  - TrailRenderer 위한 조정
	//    - TrailRenderer는 Position의 변경전과 후 사이에 표시되어 있으므로 회전 방향에 맟우어 위치를  변경한다.                            
	// ------------------------------------------------------------------------
	private void UpdateTransformMesh()
	{
		
		// 현재의 방향에서 플레이어의 방향으로 지정한 스피드로 향한 후의 각도를 취득
        float targetRotationAngle = player.transform.eulerAngles.y;
		float currentRotationAngle = transform.eulerAngles.y;
		currentRotationAngle = Mathf.LerpAngle(
			currentRotationAngle,
			targetRotationAngle,
			ScoutingLaserTurnRate * Time.deltaTime );
		Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
		
		// 콜리전을 사용할 Mesh를 작성
		if ( isScanOn )
		{
			float[] tmpAngle = new float[]{ player.transform.eulerAngles.y, transform.eulerAngles.y };
			scoutingLaserMesh.SendMessage("makeFanShape", tmpAngle);
		}
		else
		{
			scoutingLaserMesh.SendMessage("clearShape");
		}
		
		// 각도 설정
		transform.rotation = tiltedRotation;
		
		// 위치 변경
		transform.position = new Vector3(
			ScoutingLaserFowardPosition * Mathf.Sin( Mathf.Deg2Rad * currentRotationAngle ),
			0,
			ScoutingLaserFowardPosition * Mathf.Cos( Mathf.Deg2Rad * currentRotationAngle )
		);

	}
	
	// ------------------------------------------------------------------------
	// 록온 처리  
	// ------------------------------------------------------------------------
	public void Lockon( Collider collider )
	{
		// 적기를 록온.
		if ( collider.gameObject.tag == "Enemy" ) {
			
			// 록온 하지 않은 경우에 록온한다.
			int targetId = collider.gameObject.GetInstanceID();
			bool isLockon = IncreaseLockonCount( targetId );
			if ( isLockon ) {
				
				// ------------------------------------------------------------
				// 록온   
				// ------------------------------------------------------------
				
				// 록온 번호를 결정
				int lockonNumber = getLockonNumber();
							
				if ( lockonNumber >= 0 ) {

                    // lockonSight의 표시 위치는 록온한 적기의 위치에 표시                  
					Vector3 targetPosition = collider.gameObject.transform.position;
					Quaternion tagetRotation = new Quaternion( 0f, 180f, 0f, 0f );

                    // lockonSight의 인스턴스를 생성      
					GameObject lockonSight;
					lockonSight = Instantiate( lockonSights[lockonNumber], targetPosition, tagetRotation ) as GameObject;
					lockonSight.SendMessage( "SetLockonEnemy", collider.gameObject );
					
					// 록온 리스트에 록온한 적기를 추가.
					lockedOnEnemyIds[lockonNumber] = targetId;
					
					// 록온한 오브젝트를 저장한다.   
					lockedOnEnemys[lockonNumber] = collider.gameObject;

                    // lockonSight를 저장한다.         
					lockedOnSights[lockonNumber] = lockonSight;
					
					// 메세지 표시
					subScreenMessage.SendMessage("SetMessage", "LOCKED ON SOME ENEMIES." );
					lockSlot.SendMessage("SetLockCount", lockonCount );
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 록온 수 갱신
	//  - true:가산 성공, false:가산 실패.
	// ------------------------------------------------------------------------
	public bool IncreaseLockonCount( int enemyId )
	{
		// 최대 록온수 이하인지
		if ( lockonCount < MAX_LOCKON_COUNT )
		{
			// 록온 하지 않은 적기인가
			if ( !IsLockon( enemyId ) )
			{
				// 록온수+1.
				lockonCount++;
				return true;
			}
		}
		// 록온 종료했다.
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 록온의 빈 영역의 첨자를 전송
	//  - 빈 영역 존재: 0이상
	//  - 빈 영역 존재하지 않음: -1
	// ------------------------------------------------------------------------
	private int getLockonNumber()
	{
		// 록온 레이저를 관리하는 배열에서 비어 있는 영역을 찾아 그 영역에 첨자를 전송한다. 
		for( int i = 0; i < lockedOnEnemyIds.Length; i++ )
		{
			if ( lockedOnEnemyIds[i] == invalidInstanceId )
			{
				return i;
			}
		}
		return -1;
	}
	
	// ------------------------------------------------------------------------
	// 록온 수를 줄인다.
	// ------------------------------------------------------------------------
	public void DecreaseLockonCount( int instanceId )
	{
		// 록온 수를 줄인다. 
		if ( lockonCount > 0 ) {
			lockonCount--;
			lockSlot.SendMessage("SetLockCount", lockonCount );		
		}
		
		// 록온 완료 정보를 삭제
		for( int i = 0; i < lockedOnEnemyIds.Length; i++ )
		{
			if ( lockedOnEnemyIds[i] == instanceId )
			{
				if ( lockedOnSights[i] )
				{
					lockedOnSights[i].SendMessage( "Destroy" );
				}
				lockedOnEnemyIds[i] = invalidInstanceId;
				lockedOnEnemys[i] = null;
				lockonLaserIds[i] = invalidInstanceId;
				lockedOnSights[i] = null;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 록온 상황 확인
	// ------------------------------------------------------------------------
	public bool IsLockon( int enemyId )
	{
		// 록온 종료 적기 리스트에 미등록되었는가
		int existIndex = System.Array.IndexOf( lockedOnEnemyIds, enemyId );

		if ( existIndex == -1 )
		{
			// 록온하지 않음
			return false;
		}
		// 록온 종료 
		return true;
	}
	
	// ------------------------------------------------------------------------
	// 록온 레이저 발사           
	// ------------------------------------------------------------------------
	private void StartLockonLaser()
	{
		int countLockon = 0;
		
		// 록온 수를 센다.
		for( int i = 0; i < MAX_LOCKON_COUNT; i++ )
		{
			if ( lockedOnEnemyIds[i] != invalidInstanceId )
			{
				if ( lockonLaserIds[i] == invalidInstanceId )
				{
					// 록온 보너스  
					countLockon++;
					lockBonus.SendMessage("SetLockCount", countLockon );
				}
			}
		}

		for( int i = 0; i < MAX_LOCKON_COUNT; i++ )
		{
			if ( lockedOnEnemyIds[i] != invalidInstanceId )
			{
				// 록온 레이저를 아직 작성하지 않은 경우에 한해
				if ( lockonLaserIds[i] == invalidInstanceId )
				{
				
					// ------------------------------------------------------------
					// 록온 레이저 작성
					// ------------------------------------------------------------
					
					// 플레이어의 좌표 취득
					Vector3 playerPos = player.transform.position;
					Quaternion playerRot = player.transform.rotation;
					
					// 레이저의 발사 각도를 정한다.
					Quaternion startRotation = player.transform.rotation;
					float laserRotationAngle = startRotation.eulerAngles.y;

					laserRotationAngle += lockonLaserStartRotation[i];
					Quaternion tiltedRotation = Quaternion.Euler( 0, laserRotationAngle, 0 );
					playerRot = tiltedRotation;
					
					// 록온 레이저 작성
					GameObject tmpLockonLaser;
					tmpLockonLaser = Instantiate( lockonLaser, playerPos, playerRot ) as GameObject;
					tmpLockonLaser.SendMessage( "SetLockonBonus", Mathf.Pow (2, countLockon ) );
					tmpLockonLaser.SendMessage( "SetTargetEnemy", lockedOnEnemys[i] );
					lockonLaserIds[i] = tmpLockonLaser.GetInstanceID();
				}
			}
		}
		
		// 메세지 삭제
		if ( countLockon == 0 )
		{
		}

	}
	
	public void Reset()
	{
		// 탐색 레이저를 무효
		this.isScanOn = false;
		
		// 탐색 레이저를 미표시
		this.GetComponent<TrailRenderer>().enabled = false;
		scoutingLaserLine.enabled = false;
		
		// 탐색 레이저 소리 정지
		if ( this.audio.isPlaying )
		{
			this.audio.Stop();
		} 
		
		// 록온에 관한 정보를 초기값으로 
		for( int i = 0; i < MAX_LOCKON_COUNT; i++ )
		{
			lockedOnEnemys[i] = null;
			lockedOnEnemyIds[i] = invalidInstanceId;
			lockonLaserIds[i] = invalidInstanceId;
			if ( lockedOnSights[i] )
			{
				lockedOnSights[i].SendMessage("Destroy");
			}
			lockedOnSights[i] = null;
		}
		lockonCount = 0;

        // lockSlot 표시를 초기값으로       
		lockSlot.SendMessage( "SetLockCount", lockonCount );

        // lockBonus 표시를 초기값으로         
		lockBonus.SendMessage( "SetLockCount", lockonCount );
		
	}

}