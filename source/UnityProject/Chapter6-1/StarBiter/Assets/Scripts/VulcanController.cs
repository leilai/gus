using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss vulcan 에서 발사되는 발칸탄의 움직임을 제어한다.              
// ----------------------------------------------------------------------------
public class VulcanController : MonoBehaviour {

	public float bulletSpeed = 5f;					// 발칸탄의 스피드.
	
	private GameObject target;						// 타겟   

	private BattleSpaceController battleSpaceContoller;	// 전투 공간
	
	private float breakingDistance = 20f;			// 발칸탄 소멸 조건(타겟과 발칸탄의 거리가 지정 이상일 경우)   
	private bool isStart = false;					// 발칸탄 발사(true:발사).
	
	void Start () {
		
		// 전투 공간을 취득
		battleSpaceContoller = 
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
	}
	
	void Update () {
		
		if ( isStart )
		{
			// 발칸탄을 전진시킨다.
			ForwardBullet();
		
			// 타겟이 소멸되면 발칸탄도 소멸시킨다.
			IsDestroyTarget();
			
			// 발칸탄이 범위 밖에 있는 경우에는 소멸시킨다.
			IsOverTheDistance();
		}
	}
	
	// ------------------------------------------------------------------------
	// 발칸탄을 전진시킨다.
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 적기가 있는 경우에만 처리.
		if ( target )
		{
			// 발칸탄을 이동한다.
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

		isStart = true;	// 발칸탄 발사
	}
	
	// ------------------------------------------------------------------------
	// 발칸탄의 진행방향은 타겟을 향하도록 한다.
	// ------------------------------------------------------------------------
	private void SetRotation( float rate )
	{
		// 타겟 방향 취득  
		Vector3 targetPosition = target.gameObject.transform.position;
		Vector3 relativePosition = targetPosition - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
		
		// 발칸탄의 현재 방향에서 적기의 방향으로, 지정된 스피드로 향하게 한 후의 각도를 취득.
		float targetRotationAngle = targetRotation.eulerAngles.y;
		float currentRotationAngle = transform.eulerAngles.y;
		currentRotationAngle = Mathf.LerpAngle(
			currentRotationAngle,
			targetRotationAngle,
			rate * Time.deltaTime );
		Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
		
		// 레이저 각도를 변경
		transform.rotation = tiltedRotation;
	}
	
	// ------------------------------------------------------------------------
	// 스스로 파괴처리.
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		if ( !target )
		{
			// 발칸탄 제거 
			Destroy( this.gameObject );
		}
	}
	
	// ------------------------------------------------------------------------
	// 타겟과의 거리가 일정이상인 경우에는 제거한다.
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
				// 발칸탄 제거  
				Destroy( this.gameObject );
			}
		}
	}
}
