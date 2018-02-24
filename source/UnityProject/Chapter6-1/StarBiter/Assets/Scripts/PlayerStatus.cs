using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 플레이어의 상태
// ----------------------------------------------------------------------------
public class PlayerStatus : MonoBehaviour {

	public int playerCount = 3;						// 플레이어 수
	
	public GameObject effectBomb;
	public GameObject GameOver;
	
	private enum State
	{
		INITIALIZE,
		INVISIBLE,			// 시작 시에는 적이 없음
		NOWPLAYING,
		STARTDESTRUCTION,	// 플레이어 파괴
		DESTRUCTION,		// 플레이어 파괴중
		WAITING,			// 대기
		RESTART				// 재시작
	}

	private State programState = State.INITIALIZE;	// 내부 처리 상태
	private int waitTimeAfterExplosion = 2;			// 폭발 애니메이션 재생 후의 대기 시간
	
	private bool isGAMEOVER	= false;				// 게임 종료

	private GameObject scoutingLaser;
	private MeshRenderer invisibleZone;
	private GameObject printScore;
	
	void Start () 
	{
		// scoutingLaser의 인스턴스를 취득
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser");
		
		// 적이 없음을 표시하는 인스턴스의 MeshRenderer를 취득
		invisibleZone = GameObject.FindGameObjectWithTag("InvisibleZone").GetComponent<MeshRenderer>();
		
		// score의 인스턴스를 취득
		printScore = GameObject.FindGameObjectWithTag("Score");
		
		// 게임 시작을 위해 플레이어를 표시한다.
		ShowPlayer();
		
		programState = State.NOWPLAYING;
		
	}
	
	void Update ()
	{
	
		if ( !isGAMEOVER )
		{
			// 파괴확인
			if ( programState == State.STARTDESTRUCTION )
			{
				programState = State.DESTRUCTION;
				
				// 플레이어를 파괴한다.
				DestructPlayer();
			}
	
			// 대기
			if ( programState == State.DESTRUCTION )
			{
				if ( !this.audio.isPlaying )
				{
					programState = State.WAITING;
					
					// 일정 시간 대기
					StartCoroutine("Waiting");
				}
			}
			
			// 게임 재시작
			if ( programState == State.RESTART )
			{
				programState = State.INITIALIZE;
				
				// 게임 재시작을 위해 플레이어를 다시 표시한다.
				bool ret = ShowPlayer();
				if ( ret )
				{
					// 재시작 직후는 적이 없음
					programState = State.INVISIBLE;
					// 적 없음 표시
					invisibleZone.enabled = true;
					// 적이 없는 삭제 시간 설정
					StartCoroutine( WaitAndUpdateState( 2f, State.NOWPLAYING ) );
				}
				else
				{
					isGAMEOVER = true;
				}
			}
		}
	}

	// ------------------------------------------------------------------------
	// 플레이어의 파괴 판정
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{

		if ( programState == State.NOWPLAYING ) {
			// 암석에 충돌
			if ( collider.tag == "Stone" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// 적기에 충돌
			if ( collider.tag == "Enemy" )
			{
				programState = State.STARTDESTRUCTION;
			}

            // Vulcan에 충돌
			if ( collider.tag == "EnemyVulcan" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// 레이저에 충돌
			if ( collider.tag == "EnemyLaser" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// 총알에 충돌
			if ( collider.tag == "EnemyShot" )
			{
				programState = State.STARTDESTRUCTION;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 플레이어를 표시한다.
	// ------------------------------------------------------------------------
	private bool ShowPlayer()
	{
		// 플레이어의 수가 0인지 확인
		if ( playerCount <= 0 )
		{
            // HiScore 갱신    
			bool isHiScore = SetHiscore();
			
			// 게임 오버 문자 표시
			GameObject gameOver = Instantiate( GameOver, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
			gameOver.SendMessage( "SetIsHiScore", isHiScore );
			gameOver.SendMessage( "Show" );
			
			// 오프닝의 클릭 이벤트를 유효화한다.
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ClickToOpening>().enabled = true;
			
			// 일정 시간 후에 오프닝 화면으로 돌아간다.
			StartCoroutine( WaitAndCallScene( 5f, "opening" ) );
			
			return false;
		}
		else
		{
		
			// 플레이어 수를 한 개 줄인다.
			playerCount--;
			
			// 플레이어를 표시한다.
			Component[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
	        foreach ( MeshRenderer meshRenderer in meshRenderers ) {
	            meshRenderer.enabled = true;
	        }
			
			// Player Left 의 표시
			if ( playerCount < 2 )
			{
				GameObject.FindGameObjectWithTag("PlayerLeft02").GetComponent<GUITexture>().enabled = false;
			}
			if ( playerCount < 1 )
			{
				GameObject.FindGameObjectWithTag("PlayerLeft01").GetComponent<GUITexture>().enabled = false;
			}
			
			// 플레이어가 살아 있는 상태로 간주
			this.GetComponent<PlayerController>().SetIsAlive( true );
			
			return true;
		}
	}
	
	// ------------------------------------------------------------------------
	// 플레이어를 숨기다.
	// ------------------------------------------------------------------------
	private void HidePlayer()
	{
		// 플레이어를 숨기다.
		Component[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
        foreach ( MeshRenderer meshRenderer in meshRenderers ) {
            meshRenderer.enabled = false;
        }
		
		// 플레이어는 죽어 있는 상태로 간주
		this.GetComponent<PlayerController>().SetIsAlive( true );
	}
	
	// ------------------------------------------------------------------------
	// 플레이어를 파괴한다.
	// ------------------------------------------------------------------------
	private void DestructPlayer()
	{
	
		// 플레이어를 삭제한다.
		HidePlayer();
		
		// 플레이어의 컨트롤에 관한 정보를 초기 상태로 되돌린다.
		SendMessage( "Reset" );
		
		// 탐색 레이저, 록온에 관한 정보를 초기 상태로 되돌린다.
		scoutingLaser.SendMessage( "Reset" );
		
		//폭발
		ShowExplosion();

	}
	
	// ------------------------------------------------------------------------
	// 플레이어가 파괴된 후 회복하여 대기
	// ------------------------------------------------------------------------
	IEnumerator Waiting()
	{
		// 일정 시간 대기
		yield return new WaitForSeconds( waitTimeAfterExplosion );

		// 화면을 클리어
		ClearDisplay();
		
		// 게임 재시작
		programState = State.RESTART;
	}
	
	// ------------------------------------------------------------------------
	// 플레이어의 폭발 표시
	// ------------------------------------------------------------------------
	private void ShowExplosion()
	{
		// 폭발 효과의 오브젝트가 존재하는가?
		if ( effectBomb )
		{
			// 표과 재생 
			Instantiate(
				effectBomb,
				Vector3.zero, 
				new Quaternion(0f, 0f, 0f, 0f) );
		}
		
		// 폭발음 작성
		this.audio.Play();
		
	}
	
	// ------------------------------------------------------------------------
	// 화면 클리어
	// ------------------------------------------------------------------------
	private void ClearDisplay()
	{	
		// EnemyMaker에서 작성한 적기를 모두 삭제
		GameObject[] enemyMakers = GameObject.FindGameObjectsWithTag("EnemyMaker");
		foreach( GameObject enemyMaker in enemyMakers )
		{
			enemyMaker.SendMessage("DestroyEnemys");
		}
		
		// 위의 처리에서 삭제하지 않은 단독 적기, 포메이션에서 멀어진 적기 제거
		GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
		foreach( GameObject enemy in enemys )
		{
			enemy.SendMessage("DestroyEnemyEx");
		}
		
		// 적기의 총알 제거
		GameObject[] enemyShots = GameObject.FindGameObjectsWithTag("EnemyShot");
		foreach( GameObject enemyShot in enemyShots )
		{
			Destroy( enemyShot );
		}
		
		// BOSS의 레이저 제거
		GameObject[] enemyLasers = GameObject.FindGameObjectsWithTag("EnemyLaser");
		foreach( GameObject enemyLaser in enemyLasers )
		{
			Destroy( enemyLaser );
		}
		
		// BOSS의 Bullet을 제거
		GameObject[] enemyVulcans = GameObject.FindGameObjectsWithTag("EnemyVulcan");
		foreach( GameObject enemyVulcan in enemyVulcans )
		{
			Destroy( enemyVulcan );
		}
		
		// 효과 제거   
		GameObject[] tmpGameObjects = GameObject.FindGameObjectsWithTag("EffectBomb");
	    for ( int i = 0; i < tmpGameObjects.Length; i++)
		{
			if ( tmpGameObjects[i] != null )
			{
				Destroy( tmpGameObjects[i] );
				tmpGameObjects[i] = null;
			}
	    }
	}
	
	// ------------------------------------------------------------------------
	// 플레이 중인지 아닌지를 전송한다.
	//  - INVISIBLE은 포함되지 않음
	// ------------------------------------------------------------------------
	public bool GetIsNOWPLAYING()
	{
		if ( programState == State.NOWPLAYING )
		{
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 플레이어를 조작가능한지 전송한다.
	//  - INVISIBLE도 포함
	// ------------------------------------------------------------------------
	public bool GetCanPlay()
	{
		if ( programState == State.NOWPLAYING ||
			 programState == State.INVISIBLE )
		{
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 지정 시간후 상태를 변경한다.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateState( float waitForSeconds, State state )
	{
		// 지정한 시간 대기
		yield return new WaitForSeconds( waitForSeconds );
		
		// 상태를 갱신
		programState = state;
		
		// 적이 없는 상태의 비표시
		invisibleZone.enabled = false;
	}
	
	// ------------------------------------------------------------------------
	// 지정한 시간 후, 씬을 읽어온다.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndCallScene( float waitForSeconds, string sceneName )
	{
		// 지정한 시간 대기
		yield return new WaitForSeconds( waitForSeconds );
		
		// 게임 씬을 불러온다.
		Application.LoadLevel( sceneName );
	}
	
	// ------------------------------------------------------------------------
	// HISCORE룰 저장한다.
	// ------------------------------------------------------------------------
	private bool SetHiscore()
	{
		// SCORE/HISCORE를 취득
		int hiScore = int.Parse(
			GameObject.FindGameObjectWithTag("HighScore").GetComponent<GUIText>().text );
		int score = int.Parse(
			GameObject.FindGameObjectWithTag("Score").GetComponent<GUIText>().text );
		
		// HISCORE를 초과하였는가?
		if ( score > hiScore )
		{
			// 하이 스코어 갱신
			GameObject.FindGameObjectWithTag("HighScore").GetComponent<GUIText>().text = score.ToString();
			
			// 글로벌 영역에 저장한다.
			GlobalParam.GetInstance().SetHiScore( printScore.GetComponent<PrintScore>().GetScore() );
			
			return true;
		}
		return false;
	}
	
}
