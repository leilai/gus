using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss Vulcan     Vulcan탄 발사 제어.
// Boss Vulcan Unit의 상태 관리.
// ----------------------------------------------------------------------------
public class BulletMaker : MonoBehaviour {
	
	public float fireInterval = 0.3f;		// 발포 간격.
	public int numberOfBullets = 15;		// 1회의 발포수.

    public GameObject Bullet;				// Vulcan탄의 Prefab.
	
	private GameObject player;				// 플레이어의 인스턴스 
    private PlayerStatus playerStatus;		// PlayerStatus의 인스턴스          

    private GameObject shotPosition;		// Vulcan탄의 발사장소.  
	
	private int fireCount;					// 발포 횟수.
	private bool isFiring = false;			// 발포중.
    private bool isMakingBullet = false;	// Vulcan탄 작성중.
	
	private GameObject subScreenMessage;	// SubScreen의 메세지 영역
	
	void Start () {
	
		// player의 인스턴스를 취득.
		player = GameObject.FindGameObjectWithTag("Player");

        // playerStatus의 인스턴스를 취득.
		playerStatus = player.GetComponent<PlayerStatus>();

        // Vulcan탄의 발사 위치 정보를 취득.
		shotPosition = GetComponentInChildren<Transform>().Find("ShotPosition").gameObject;
		
		// SubScreenMessage의 인스턴스를 취득.
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");

	}
	
	void Update () {
	
		// 발사중인지?
		if ( isFiring )
		{
            // Vulcan탄의 발사 준비중인지.
			if ( !isMakingBullet )
			{
				isMakingBullet = true;
				MakeBullet();
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // Vulcan탄을 작성.  
	//  - 플레이어가 살아있을 때에만 발사한다.
	// ------------------------------------------------------------------------
	private void MakeBullet()
	{
        // Vulcan탄의 GameObject는 지정되어 있는가?
		if ( Bullet )
		{
            // Vulcan탄 작성중   
			GameObject tmpBullet;
			if ( playerStatus.GetIsNOWPLAYING() )
			{
				tmpBullet = Instantiate( Bullet, shotPosition.transform.position, this.transform.rotation ) as GameObject;
				tmpBullet.SendMessage( "SetTarget", player );	
			}
			
			// 발사 수를 카운트.
			this.fireCount++;

            // Vulcan탄를 발사하면 Vulcan탄 작성을 종료한다.    
			if ( this.fireCount >= numberOfBullets )
			{
				isFiring = false;
			}
			
			// 다음 발사까지 일정 시간 대기한다.
			StartCoroutine( WaitAndUpdateFlag( fireInterval ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 지정한 시간을 기다리고 발사를 변경한다.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateFlag( float waitForSeconds )
	{
		// 대기
		yield return new WaitForSeconds( waitForSeconds );
		
		// 스테이지 갱신.
		isMakingBullet = false;
	}
	
	// ------------------------------------------------------------------------
	// 발사 시작.
	// ------------------------------------------------------------------------
	public void SetIsFiring()
	{
		fireCount = 0;
		this.isFiring = true;
	}
	
	// ------------------------------------------------------------------------
	// BOSS Vulcan Unit가 파괴되는 경우의 처리
	// ------------------------------------------------------------------------
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if (
				this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
				this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
			{
				subScreenMessage.SendMessage("SetMessage", " ");
				subScreenMessage.SendMessage("SetMessage", "DESTROYED VULCAN UNIT." );
				subScreenMessage.SendMessage("SetMessage", " ");
			}
		}
	}
}
