using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss Shot 총알 발사 제어.
// ----------------------------------------------------------------------------
public class ShotMaker : MonoBehaviour {

	public float fireInterval = 0.1f;		// 발포 간격
	public int numberOfBullets = 10;		// 1회의 총알 수
	
	public GameObject Shot;					// 총알 Prefab.
	
	private GameObject player;				// 플레이어 
	
	private int fireCount;					// 발포횟수
	private bool isFiring = false;			// 발포중
	private bool isMakingBullet = false;	// 탄 작성 중
	
	private float fireAngle = 0;			// 발사 각도
	
	void Start () {
	
		// player의 인스턴스를 취득
		player = GameObject.FindGameObjectWithTag("Player");

	}
	
	void Update () {
		
		// 발사 중인가?
		if ( isFiring )
		{
			// 발사전?
			if ( fireCount == 0 )
			{
				// 발사 각도 계산
				SetAngle();
			}
			
			// 탄 발사 준비중인가?
			if ( !isMakingBullet )
			{
				isMakingBullet = true;
				MakeBullet();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 탄을 작성.
	// ------------------------------------------------------------------------
	private void MakeBullet()
	{
		// 탄의 GameObject는 지정되어 있는가?
		if ( Shot )
		{
			// 탄 작성.
			GameObject tmpBullet;
			tmpBullet = Instantiate( Shot, transform.position, Quaternion.Euler( 0, fireAngle, 0 ) ) as GameObject;
			tmpBullet.SendMessage( "SetTarget", player );	
			
			// 발사 수를 카운트.
			fireCount++;
			
			// 각도를 1발 마다 15도 움직인다.
			fireAngle -= 15f;
			
			// 지정한 수가 발사되면 탄 작성을 종료한다.
            if ( fireCount >= numberOfBullets )
			{
				isFiring = false;
			}
			
			// 다음 발사까지 일정 시간 대기한다.
			StartCoroutine( WaitAndUpdateFlag( fireInterval ) );
	
		}
	}
	
	// ------------------------------------------------------------------------
	// 지정한 기간을 기다리고, 상태를 변경한다.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateFlag( float waitForSeconds )
	{
		// 대기.
		yield return new WaitForSeconds( waitForSeconds );
		
		// 스테이지 갱신
		isMakingBullet = false;
	}
	
	// ------------------------------------------------------------------------
	// 발사 각도를 구한다.
	// ------------------------------------------------------------------------
	private void SetAngle()
	{
		// 발사 각도 계산
		Vector3 targetPosition = player.transform.position;
		Vector3 relativePosition = targetPosition - transform.position;
		Quaternion tiltedRotation = Quaternion.LookRotation( relativePosition );
		fireAngle = tiltedRotation.eulerAngles.y + ( numberOfBullets / 2 ) * 15;
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
	// 발사 중인지 아닌지의 상태를 전송한다.
	// ------------------------------------------------------------------------
	public bool GetIsFiring()
	{
		return isFiring;
	}
}
