using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss Laser 레이저 발사 제어
// Boss Laser Unit의 상태 관리
// ----------------------------------------------------------------------------
public class LaserMaker : MonoBehaviour {

	public float fireInterval = 1f;			// 발포간격
	public int numberOfBullets = 3;			// 1회의 발포수
	
	public GameObject Laser;				// 레이저의 Prefab.
	
	private GameObject player;				// 플레이어의 인스턴스 
    private PlayerStatus playerStatus;		// PlayerStatus의 인스턴스           
	
	private GameObject shotPosition;		// 레이저 발사 장소
	
	private int fireCount;					// 발포횟수
	private bool isFiring = false;			// 발포중
	private bool isMakingBullet = false;	// 레이저 작성중
	
	private GameObject subScreenMessage;	// SubScreen의 메세지 영역
	
	void Start () {
	
		// player의 인스턴스를 취득
		player = GameObject.FindGameObjectWithTag("Player");

        // PlayerStatus의 인스턴스를 취득          
		playerStatus = player.GetComponent<PlayerStatus>();
		
		// 레이저의 발사 위치 정보를 취득
		shotPosition = GetComponentInChildren<Transform>().Find("ShotPosition").gameObject;
		
		// SubScreenMessage의 인스턴스를 취득
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
	}
	
	void Update () {
	
		// 발사중인가?
		if ( isFiring )
		{
			// 레이저의 발사 준비 중인가?
			if ( !isMakingBullet )
			{
				isMakingBullet = true;
				MakeLaser();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 레이저를 작성
	//  - 플레이어가 생존 시에만 발사한다.
	// ------------------------------------------------------------------------
	private void MakeLaser()
	{
		//  레이저의 GameObject은 지정되어 있는가?
		if ( Laser )
		{
			// 레이저 작성
			GameObject tmpBullet;
			if ( playerStatus.GetIsNOWPLAYING() )
			{
				tmpBullet = Instantiate( Laser, shotPosition.transform.position, this.transform.rotation ) as GameObject;
				tmpBullet.SendMessage( "SetTarget", player );	
			}
				
			// 발사수를 카운트
			fireCount++;
			
			// 지정한 수를 발사하면 레이저의 작성을 종료한다.
			if ( fireCount >= numberOfBullets )
			{
				isFiring = false;
			}
			
			// 다음 발사까지 일정 시간 대기한다.
			StartCoroutine( WaitAndUpdateFlag( fireInterval ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 지정한 시간을 기다려 상태를 갱신한다.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateFlag( float waitForSeconds )
	{
		// 대기
		yield return new WaitForSeconds( waitForSeconds );
		
		// 스테이지 갱신
		isMakingBullet = false;
	}
	
	// ------------------------------------------------------------------------
	// 발사 시작
	// ------------------------------------------------------------------------
	public void SetIsFiring()
	{
		fireCount = 0;
		this.isFiring = true;
	}
	
	// ------------------------------------------------------------------------
	// BOSS Laser Unit가 파괴된 경우의 처리
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
				subScreenMessage.SendMessage("SetMessage", "DESTROYED LASER UNIT." );
				subScreenMessage.SendMessage("SetMessage", " ");
			}
		}
	}
}
