using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss Laser 에서 나오는 레이저 동작을 제어한다.
// ----------------------------------------------------------------------------
public class LaserController : MonoBehaviour {

	public float bulletSpeed = 1f;					// 레이저의 스피드
	public float laserSize = 100f;					// 레이저 길이
	
	private GameObject target;						// 타겟   
	private BattleSpaceController battleSpaceContoller;	// 전투 공간.
	
	private float breakingDistance = 20f;			// 레이저 소멸 조건(타겟과 레이저의 거리가 지정 이상이라면)
	private bool isStart = false;					// 레이저 발사(true:발사).
	
	void Start () {
		
		// 전투 공간 취득.
		battleSpaceContoller =
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
	}
	
	void Update () {
		
		if ( isStart )
		{
			// 레이저를 전진시킨다.
			ForwardBullet();
		
			// 타겟이 소멸되면 레이저도 소멸시킨다.
			IsDestroyTarget();
			
			// 레이저가 범위 밖의 경우에는 소멸시킨다.
			IsOverTheDistance();
		}
	}
	
	// ------------------------------------------------------------------------
	// 레이저를 전진시킨다.
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 적기가 있을 때에만 처리.
		if ( target )
		{
			// 레이저를 전진시킨다.
			transform.Translate ( new Vector3( 0f, 0f, bulletSpeed * Time.deltaTime ) );
			
			// 전투 공간의 스크롤 방향을 추가한다.
			transform.position -= battleSpaceContoller.GetAdditionPos();
			
			// 레이저를 늘린다.
			if ( transform.localScale.z < laserSize )
			{
				transform.localScale = new Vector3( 
					transform.localScale.x,
					transform.localScale.y,
					transform.localScale.z + ( bulletSpeed * Time.deltaTime * 30 ) );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 타겟을 설정  
	// ------------------------------------------------------------------------
	public void SetTarget( GameObject target )
	{
		this.target = target;
		
		// 레이저 방향을 지정
		SetDirection();
		
		isStart = true;	// 레이저 발사
	}
	
	// ------------------------------------------------------------------------
	// 진행 방향은 타겟을 향한다.
	// ------------------------------------------------------------------------
	private void SetDirection()
	{
		// 타겟이 있을 시에만 처리.
		if ( target )
		{
			// 타겟의 방향을 취득
			Vector3 targetPosition = target.gameObject.transform.position;
			Vector3 relativePosition = targetPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			
			// 레이저의 각도를 변경
			transform.rotation = targetRotation;
		}
	}
	
	// ------------------------------------------------------------------------
	// 스스로 파괴처리
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		if ( !target )
		{
			// 레이저 제거
			Destroy( this.gameObject );
		}
	}
	
	// ------------------------------------------------------------------------
	// 타겟과의 거리가 일정 이상의 경우는 제거한다.
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
				// 레이저 제거
				Destroy( this.gameObject );
			}
		}
	}
}
