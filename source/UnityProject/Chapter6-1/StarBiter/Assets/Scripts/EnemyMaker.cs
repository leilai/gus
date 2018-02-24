using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyMaker
//  - 지정된 간격으로 기정한 적기를 작성한다.
//  - 씬에서의 적기 작성에 대한 상한선을 지정한다.
// ----------------------------------------------------------------------------
public class EnemyMaker : MonoBehaviour {
	
	public float creationInterval = 5.0f;		// 적기 작성 간격
	public GameObject enemyGameObject;			// 적기 게임오브젝트  
	
	public int maxEnemysInScene = 6;			// 씬 내에서의 작성 상한선.
	public int maxEnemys = 1;					// 총 작성수.
	
	public bool canShoot = false;				// 총알을 쏠 수 있다.
	public bool addToSpeed = false;				// 스피드를 올린다.
	
	public bool isBoss = false;					// 최종 보스는 어떠한지.
	
	private bool isMaking = false;				// 적기 작성중.
	private int enemyCount = 0;					// 현재의 적기 작성수
	private GameObject[] enemyGameObjects;		// 작성한 적기의 인스턴스.
	private int[] enemyIds;						// 작성한 적기의 인스턴스 ID.

    private PlayerStatus playerStatus;			//  PlayerStatus의 인스턴스           
	
	private int destroyedEnemyCount = 0;		// 파괴된 적기의 수
	
	private GameObject stageController;			// 스테이지 컨트롤러의 인스턴스.
	
	private int stageIndex = 0;					// 스테이지 진행상황
	
	void Start () {
		
		// 스테이지 컨트롤러 인스턴스를 취득.
		stageController = GameObject.FindGameObjectWithTag("StageController");

        //  PlayerStatus의 인스턴스를 취득.          
		playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();

		// 작성할 적기의 정보를 저장할 영역 확보.
		enemyGameObjects = new GameObject[maxEnemysInScene];
		enemyIds = new int[maxEnemysInScene];

	}
	
	void Update () {
	
		// 플레이어가 조작가능한 때에만 처리한다.
		if ( playerStatus.GetIsNOWPLAYING() )
		{
		
			// 적기의 작성 상한선 모두 파괴되었는가?
			if ( destroyedEnemyCount == maxEnemys )
			{
				// StageController에 스테이지 종료 알림
				stageController.SendMessage( "SetStateEnd", stageIndex );
				
				// EnemyMaker을 정지한다.
				SetMakingStop();
			}
		
			// 적기를 아직 만들 수 있는가?
			if ( enemyCount < maxEnemysInScene ) {
				
				// 작성 중인가?
				if ( !isMaking ) {
					
					// 작성중
					isMaking = true;
					
					// 지정한 시간(CreationInterval) 경과 후에 작성한다.
					StartCoroutine( CreateEnemy() );
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 지정한 시간(CreationInterval) 경과후에 적기를 작성한다. .
	// ------------------------------------------------------------------------
	IEnumerator CreateEnemy()
	{
		// 작성 수를 카운트 업
		enemyCount++;
		
		// 일정 간격 대기
		yield return new WaitForSeconds( creationInterval );
		
		// 적기 작성
		GameObject tmpEnemy = Instantiate(
			enemyGameObject,
			Vector3.zero,
			new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		tmpEnemy.SendMessage("SetEmenyMaker", this.gameObject, SendMessageOptions.DontRequireReceiver );

        // Shoot 설정   
		if ( canShoot )
		{
			tmpEnemy.SendMessage( "SetCanShoot", true, SendMessageOptions.DontRequireReceiver );
			// 자식이 있으면 모두 전송.
			Transform[] children = tmpEnemy.GetComponentsInChildren<Transform>();
      		foreach ( Transform child in children )
			{
    			if ( child.tag == "Enemy" )
				{
					child.SendMessage( "SetCanShoot", true, SendMessageOptions.DontRequireReceiver );
				}
			}
		}
		
		// 스피드 설정
		if ( addToSpeed && tmpEnemy.GetComponent<EnemyType02Controller>() )
		{
			tmpEnemy.SendMessage( "SpeedUp", null, SendMessageOptions.DontRequireReceiver );
		}
		
		// 작성한 gameObject 를 저장
	    for ( int i = 0; i < enemyGameObjects.Length; i++)
		{
			if ( enemyGameObjects[i] == null )
			{
				enemyGameObjects[i] = tmpEnemy;
				enemyIds[i] = tmpEnemy.GetInstanceID();
				break;
			}
	    }
		
		// 작성 요청 종료
		isMaking = false;

	}
	
	// ------------------------------------------------------------------------
	// 적기 장성수에서 한 개를 줄인다.
	// ------------------------------------------------------------------------
	public void DecreaseEnemyCount( int instanceId )
	{
		// 적기수를 줄인다.
		if ( enemyCount > 0 ) {
			enemyCount--;
		}
		
		// 작성종료 적기 정보를 삭제    
		for( int i = 0; i < enemyIds.Length; i++ )
		{
			if ( enemyIds[i] == instanceId )
			{
				enemyIds[i] = 0;
				enemyGameObjects[i] = null;
			}
		}
		
		// 파괴한 적기의 수를 늘린다.
		destroyedEnemyCount++;
	}
	
	// ------------------------------------------------------------------------
	// 작성한 모든 적기를 삭제
	// ------------------------------------------------------------------------
	public void DestroyEnemys()
	{
		// 최종 보스의 경우 아무 것도 하지 않는다.
		if ( isBoss ) { return; }
		
		// 작성한 적기를 모두 제거한다.
	    for ( int i = 0; i < enemyGameObjects.Length; i++)
		{
			if ( enemyGameObjects[i] != null )
			{
				Destroy( enemyGameObjects[i] );
				enemyGameObjects[i] = null;
				enemyIds[i] = 0;
			}
	    }
		
		// 적기 작성수를 재설정
		enemyCount = 0;
	}
	
	// ------------------------------------------------------------------------
	// 총알을 쏠 수 있도록 한다.
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}
	
	// ------------------------------------------------------------------------
	// 스피드를 올린다.
	// ------------------------------------------------------------------------
	public void SetAddToSpeed( bool addToSpeed )
	{
		this.addToSpeed = addToSpeed;
	}
	
	// ------------------------------------------------------------------------
	// 적기의 작성 기능을 정지한다.
	// ------------------------------------------------------------------------
	private void SetMakingStop()
	{
		maxEnemysInScene = 0;
		destroyedEnemyCount = 0;
	}
	
	// ------------------------------------------------------------------------
	// 적기 작성 간격을 설정한다.
	// ------------------------------------------------------------------------
	public void SetCreateInterval( float creationInterval )
	{
		this.creationInterval = creationInterval;
	}
	
	// ------------------------------------------------------------------------
	// 씬 내의 작성 상한선을 설정한다.
	// ------------------------------------------------------------------------
	public void SetMaxEnemysInScene( int maxEnemysInScene )
	{
		this.maxEnemysInScene = maxEnemysInScene;
	}
	
	// ------------------------------------------------------------------------
	// 스테이지 진행상황을 저장한다.
	// ------------------------------------------------------------------------
	public void SetStage( int stageIndex )
	{
		this.stageIndex = stageIndex;
	}

}
