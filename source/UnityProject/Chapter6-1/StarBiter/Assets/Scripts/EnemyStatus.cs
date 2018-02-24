using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 적의 상태를 관리한다.
// ----------------------------------------------------------------------------
public class EnemyStatus : MonoBehaviour {
	
	public float breakingDistance = 20f;				// 적기 소멸 조건 (플레이어와 적기의 거리)
	public GameObject effectBomb;
	public GameObject effectDamage;
	public int life = 1;								// 라이프
	public bool isEnbleByCollisionStone = false;		// 암석과의 충돌은 유효(=true).
	public int score = 0;								// 점수
	public string enemyTypeId = "";						// 적기의(*)-TYPE.
	
	private enum State
	{
		INITIALIZE,				// 초기화
		FOLLOWINGLEADER,		// 리더를 따라가ㄴ다(BOSS의 경우에는 coreに를 따른다).
		ATTACK,					// 공격
		BREAK,					//파괴
		DESTROY					//소멸
	}
	
	private State enemyState = State.INITIALIZE;		// 적기의 상태
	
	protected int lockonBonus = 1;						// 록온 보너스  
	
	private GameObject player;							// 플레이어의 인스턴스 
	private GameObject enemyMaker;						// enemyMaker의 인스턴스
	private PrintScore printScore;						// printScore의 인스턴스
	
	private bool isMadeInEnemyMaker = false;			// enemyMaker에서 작성된다(=true).
	private bool isPlayerBackArea = false;				// 플레이어의 배후 영역에 있다.(=true).
	private bool isBreakByPlayer = false;				// 플레이어에 의해 파괴된다.(=true).
	private bool isBreakByStone = false;				// 암석에 의해 파괴된다.(=true).
	
	private GameObject txtEnemyStatus;
	private GameObject subScreenMessage;				// SubScreen의 메세지 영역.
	
	private string enemyTypeString = "";				// 적기의 TYPE명.
	
	private Vector3 beforePosition;						// 폭발 효과용: 작동 전 위치.
	private bool isMoving = false;						// 폭발 효과용 : 작동하고 있는지?
	private float speed = 0f;							// 폭발 효과용: 스피드.
	
	void Start () {

		// 플레이어의 인스턴스를 취득.
		player = GameObject.FindGameObjectWithTag("Player");
		
		// printScore의 인스턴스를 취득
		printScore = GameObject.FindGameObjectWithTag("Score").GetComponent<PrintScore>();
		
		// SubScreenMessage의 인스턴스를 취득
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
		// 적기의 TYPE명 설정
		enemyTypeString = SetEnemyType();
		
		// 추가의 최기화 처리  
		StartSub();
	}
	
	public virtual void StartSub()
	{
		// 이곳는 파생 클래스용.
	}
	
	void LateUpdate()
	{
		// 적기가 작동하고 있는지(폭발 효과용).
		if ( enemyState == State.INITIALIZE ||
			 enemyState == State.FOLLOWINGLEADER ||
			 enemyState == State.ATTACK )
		{
			if ( beforePosition != transform.position )
			{
				isMoving = true;
				speed = Vector3.Distance( beforePosition, transform.position );
			}
			else
			{
				isMoving = false;
				speed = 0f;
			}
			beforePosition = transform.position;
		}
	}
	
	void Update ()
	{
		// 적기 소멸 판정.
		IsOverTheDistance();
		
		// 적기의 파괴 확인
		IsBreak();
		
		// 적기 소멸
		DestroyEnemy();
		
		// 추가 갱신 처리
		UpdateSub();
	}
	
	public virtual void UpdateSub()
	{
		// 이곳은 파생 클래스용	
	}
	
	// ------------------------------------------------------------------------
	// 플레이어와의 거리가 일정이상의 경우에는 삭제한다.
	// ------------------------------------------------------------------------
	private void IsOverTheDistance()
	{
		if ( enemyState == State.ATTACK )
		{
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			if ( distance > breakingDistance )
			{
				enemyState = State.DESTROY;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 적기의 파괴 확인
	// ------------------------------------------------------------------------
	private void IsBreak()
	{
		if ( enemyState == State.BREAK )
		{			
			// 파괴 애니메이션
			if ( effectBomb )
			{
				// 각도를 조정(rotate의 값을 그대로 사용하면 Particle에서는 의도하지 않는 방향으로 진행되어 버린다.).
				Quaternion tmpRotation = Quaternion.Euler(0,transform.rotation.eulerAngles.y / 2,0);
				// 폭발 효과 작성.
				GameObject tmpGameObject = Instantiate( 
					effectBomb,
					transform.position,
					tmpRotation ) as GameObject;
				// 효과를 적기가 움직이는 방향으로 움직이게 한다.
				if ( isMoving )
				{
					tmpGameObject.SendMessage( "SetIsMoving", speed );
				}
			}
			
			// 적기를 정지
			enemyState = State.DESTROY;
		}
	}
	
	
	protected virtual void DestroyEnemyEx()
	{
		// 적기를 소멸시킨다.
		Destroy( this.gameObject );
	}
	
	// ------------------------------------------------------------------------
	// 적기 파괴.
	// ------------------------------------------------------------------------
	private void DestroyEnemy()
	{
		if ( enemyState == State.DESTROY )
		{
			// 파괴 대상이 포메이션의 리더인 경우, 포메이션을 해제한다(각각 단독 행동시킨다.)
			Transform[] children = this.GetComponentsInChildren<Transform>();
      		foreach ( Transform child in children )
			{
    			if ( child.tag == "Enemy" )
				{
					if ( child.GetComponent<EnemyStatus>() )
					{
						// 포메이션의 리더 이외의 멤버 (적기) 에 대해 처리한다. 
						if ( child.GetComponent<EnemyStatus>().GetIsFollowingLeader() == true )
						{
							// 단독으로 헹동을 시작한다.
							child.SendMessage( "SetIsAttack", true );
							child.transform.parent = null;
						}
					}
				}
			}
			
			// 적기 작성 수에서 한 개 감소시킨다.
			if ( isMadeInEnemyMaker )
			{
				enemyMaker.SendMessage( "DecreaseEnemyCount", this.GetInstanceID() );
			}
			
			// 추가 파괴시의 처리.
			DestroyEnemySub();
			
			// 적기를 소멸시킨다.
			Destroy( this.gameObject );
			
		}
	}
	
	public virtual void DestroyEnemySub()
	{
		// 이곳은 파생 클래스용
	}
	
	// ------------------------------------------------------------------------
	// 적기를 파괴 상태로 한다(데미지를 부모에게 전달)
	// ------------------------------------------------------------------------
	public void SetIsBreak( bool isBreak )
	{
		
		if ( isBreak )
		{
			if (
				enemyState == State.FOLLOWINGLEADER ||
				enemyState == State.ATTACK )
			{
				if ( life > 0 )
				{
					life--;
				}
				
				if ( life <= 0 )
				{
					// 적기를 파괴했다.
					enemyState = State.BREAK;
					isBreakByPlayer = true;
					
					// 점수가산.
					printScore.SendMessage( "AddScore", score * lockonBonus );
					
					// 서브 스크린으로 메세지 출력
					subScreenMessage.SendMessage(
						"SetMessage",
						"DESTROYED " + enemyTypeString + " BY LOCK BONUS X " + lockonBonus );
				}
				else
				{
					// 데미지 애니메이션 
					if ( effectDamage )
					{
						Instantiate( 
							effectDamage,
							transform.position,
							transform.rotation );
					}
				}
				isBreak = false;
			}
		}
	}
	
	private void SetIsBreakEx( int damage, int lockonBonus )
	{
		if (
			enemyState == State.FOLLOWINGLEADER ||
			enemyState == State.ATTACK )
		{
			if ( life > 0 )
			{
				life -= damage;
			}
			
			if ( life <= 0 )
			{
				// 적기를 파괴했다.
				enemyState = State.BREAK;
				isBreakByPlayer = true;
				
				// 점수가산.
				printScore.SendMessage( "AddScore", score * lockonBonus );
				
				// 서브스크린에 메세지 출력
				subScreenMessage.SendMessage(
					"SetMessage",
					"DESTROYED " + enemyTypeString + " BY LOCK BONUS X " + lockonBonus );
			}
			else
			{
				// 데미지 애니메이션
				if ( effectDamage )
				{
						Instantiate(
							effectDamage,
							transform.position, 
							new Quaternion(0f, 0f, 0f, 0f) );
				}
			}
		}
	}
	
	public void SetLockonBonus( int lockonBonus )
	{
		this.lockonBonus = lockonBonus;
	}
	
	public void SetIsBreakByLaser( int damage )
	{
		SetIsBreakEx( damage, lockonBonus );
	}
	
	public void SetIsBreakByShot( int damage )
	{
		SetIsBreakEx( damage, 1 );
	}
	
	public void SetIsBreakEx2()
	{
		if ( enemyState != State.BREAK )
		{
			life = 0;
			enemyState = State.BREAK;
		}
	}
	
	// ------------------------------------------------------------------------
	// 적기를 공격 상태로 한다.
	// ------------------------------------------------------------------------
	public void SetIsAttack( bool isAttack )
	{
		if ( isAttack )
		{
			if ( enemyState == State.INITIALIZE ||
				 enemyState == State.FOLLOWINGLEADER )
			{
				enemyState = State.ATTACK;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// (포메이션)리더를 추종한다.
	// ------------------------------------------------------------------------
	public void SetIsFollowingLeader( bool isFollowingLeader )
	{
		if ( isFollowingLeader )
		{
			if ( enemyState == State.INITIALIZE )
			{
				enemyState = State.FOLLOWINGLEADER;
			}
		}
	}
	
	public void SetEmenyMaker( GameObject enemyMaker )
	{
		this.enemyMaker = enemyMaker;
		isMadeInEnemyMaker = true;
	}
	public GameObject GetEmenyMaker()
	{
		return enemyMaker;
	}
	
	// ------------------------------------------------------------------------
	// 적기가 공격 상태인지 확인한다.
	// ------------------------------------------------------------------------
	public bool GetIsAttack()
	{
		if ( enemyState == State.ATTACK )
		{
			return true;
		}
		return false;
	}

	// ------------------------------------------------------------------------
	// 적기가 대기 상태인지 확인한다.
	// ------------------------------------------------------------------------
	public bool GetIsFollowingLeader()
	{
		if ( enemyState == State.FOLLOWINGLEADER )
		{
			return true;
		}
		return false;
	}

	// ------------------------------------------------------------------------
	// 충돌 판정
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		// 암석과의 충돌판정.
		if ( isEnbleByCollisionStone )
		{
			if ( collider.tag == "Stone" )
			{
				isBreakByStone = true;
				SetIsBreakEx2();
			}
		}
		
		// 플레이어의 배후 판정
		if ( collider.tag == "PlayerBackArea" )
		{
			isPlayerBackArea = true;
		}

	}
	
	void OnTriggerExit( Collider collider )
	{
		if ( collider.tag == "PlayerBackArea" )
		{
			isPlayerBackArea = false;
		}
	}
	
	public bool GetIsPlayerBackArea()
	{
		return isPlayerBackArea;
	}
	
	public bool GetIsBreakByPlayer()
	{
		return isBreakByPlayer;
	}
	public bool GetIsBreakByStone()
	{
		return isBreakByStone;
	}
	
	private string SetEnemyType()
	{
		if ( enemyTypeId.Length == 0 )
		{
			return "";
		}
		
		string tmpString;
		if ( enemyTypeId.Length == 1  )
		{
			tmpString = enemyTypeId + "-TYPE";
		}
		else
		{
			tmpString = enemyTypeId;
		}
		return tmpString;
	}
	
	public int GetLife()
	{
		return life;
	}
	
}
