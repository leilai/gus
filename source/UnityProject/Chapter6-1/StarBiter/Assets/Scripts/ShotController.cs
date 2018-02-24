using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss Shot 에서 발사되는 총알의 움직임을 제어한다.
// ----------------------------------------------------------------------------
public class ShotController : MonoBehaviour {

	public float bulletSpeed = 5f;					// 총알의 스피드
	
	private GameObject target;						// 타겟
	private BattleSpaceController battleSpaceContoller;	// 전투 공간
	
	private float breakingDistance = 20f;			// 총알 소멸 조건(타겟과 총알의 거리가 지정 이상일 때)
	private bool isStart = false;					// 총알 발사(true:발사).
	
	void Start () {
		
		// 전투 공간을 취득
		battleSpaceContoller = 
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
	}
	
	void Update () {
		
		if ( isStart )
		{
			// 총알을 전진시킨다.
			ForwardBullet();
		
			// 타겟이 소멸되면 총알도 소멸시킨다. 
			IsDestroyTarget();
			
			// 총알이 범위 밖에 있는 경우 소멸시킨다. 
			IsOverTheDistance();
		}
	}
	
	// ------------------------------------------------------------------------
    // 총알을 전진시킨다.
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 적기가 존재할 때에만 처리
		if ( target )
		{
			// 총알을 이동시킨다.
			transform.Translate ( new Vector3( 0f, 0f, bulletSpeed * Time.deltaTime ) );
			
			// 전투 공간의 스크롤 방향을 추가한다.
			transform.position -= battleSpaceContoller.GetAdditionPos();
		}
	}
	
	// ------------------------------------------------------------------------
	// 타겟을 설정  
	// ------------------------------------------------------------------------
	public void SetTarget( GameObject target )
	{
		this.target = target;
		
		isStart = true;	// 총알 발사
	}
	
	// ------------------------------------------------------------------------
	// 스스로 파괴 처리
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		if ( !target )
		{
			// 총알 삭제
			Destroy( this.gameObject );
		}
	}
	
	// ------------------------------------------------------------------------
	// 타겟과의 거리가 일정이상이 경우 삭제한다.
	// ------------------------------------------------------------------------
	private void IsOverTheDistance()
	{
		if ( target )
		{
			float distance = Vector3.Distance(
				target.transform.position,
				transform.position );
			
			if ( distance > breakingDistance )
			{
				// 총알 삭제
				Destroy( this.gameObject );
			}
		}
	}
}
