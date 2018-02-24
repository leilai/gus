using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
//  PlayerShotController
// ----------------------------------------------------------------------------
public class PlayerShotController : MonoBehaviour {

	public float bulletSpeed = 15f;					// 총알의 스피드
	public int power = 2;							// 공격력

	private BattleSpaceController battleSpaceContoller;	// 전투 공간
	private GameObject player;							// 플레이어의 인스턴스
	
	private float breakingDistance = 20f;			// 총알 소멸 조건(플레이어와 총의 거리가 지정 이상)

	private bool isClear = false;					// 총알 제거.
	
	void Start () 
	{
		// 전투 공간 취득
		battleSpaceContoller =
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
		// player의 인스턴스를 취득
		player = GameObject.FindGameObjectWithTag("Player");

	}
	
	void Update ()
	{
		// 총알을 전진시킨다.
		ForwardBullet();
	
		// 적기에 닿은 총알은 소멸시킨다.
		IsDestroy();
		
		// 범위 밖의 총알은 소멸시킨다. 
		IsOverTheDistance();
	}
	
	// ------------------------------------------------------------------------
	// 총알을 전진시킨다. 
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 총알을 이동시킨다. 
		transform.Translate ( new Vector3( 0f, 0f, bulletSpeed * Time.deltaTime ) );
		
		// 전투 공간의 스크롤 방향을 추가한다. 
		transform.position -= battleSpaceContoller.GetAdditionPos();
	}
	
	// ------------------------------------------------------------------------
	// 총알이 닿은 경우의 처리 
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		if ( collider.tag == "Enemy" )
		{
			// 적기에 파괴 지시
			isClear = true;
			collider.gameObject.SendMessage( "SetIsBreakByShot", power );
		}
		
		if ( collider.tag == "Stone" )
		{
			// 암석에 파괴 지시
			isClear = true;
			collider.gameObject.SendMessage( "SetIsBreakByShot", power );
		}
		
	}
		
	
	// ------------------------------------------------------------------------
	// 스스로 파괴 처리
	// ------------------------------------------------------------------------
	private void IsDestroy()
	{
		
		if ( isClear )
		{
			// 총알 제거
			Destroy( this.gameObject );
		}
		
	}
	
	// ------------------------------------------------------------------------
	// 플레이어와의 거리가 일정 이상인 경우 제거한다.
	// ------------------------------------------------------------------------
	private void IsOverTheDistance()
	{
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		
		if ( distance > breakingDistance )
		{
			// 총알 제거
			Destroy( this.gameObject );
		}
	}
}
