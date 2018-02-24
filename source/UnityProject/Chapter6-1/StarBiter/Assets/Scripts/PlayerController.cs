using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 플레이어의 조작/ 제어
// ----------------------------------------------------------------------------
public class PlayerController : MonoBehaviour {

	private	GameObject mainCamera;					// 메인 카메라
	private	GameObject scoutingLaser;				// 탐색 레이저
	private bool isScanOn = false;					// 탐색 모드
	private bool isAlive = false;					// 플레이어 생존
	
	private PlayerStatus playerStatus;				// PlayerStatus인스턴스   
	
	void Start () 
	{
		// 메인 카메라의 인스턴스를 취득
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		
		// 탐색 레이저의 인스턴스를 취득
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser");
		
		// PlayerStatus인스턴스를 취득.
		playerStatus = this.gameObject.GetComponent<PlayerStatus>();

	}
	
	void Update ()
	{
		// 플레이어는 생존해있는가?
		if ( isAlive )
		{
            // playerStatus 의 인스턴스는 존자하는가?          
			if ( playerStatus )
			{
				// 플레이어는 조작 가능한가?
				if ( playerStatus.GetCanPlay() == true )
				{
					// 마우스 커서 방향으로 플레이어를 향하게 한다.
					SetPlayerDirection();
					
					// 탐색 모드 교체
					ChangeScanMode();
				}
			}
		}

	}
	
	// ------------------------------------------------------------------------
    // 마우스 커서 방향으로 플레이어를 향하게 한다.
	// ------------------------------------------------------------------------
	private void SetPlayerDirection()
	{
		// 마우스 포인터 방향을 향하는 각도를 구한다.
		Vector3 mousePos = GetWorldPotitionFromMouse();
		Vector3 relativePos = mousePos - transform.position;
		Quaternion tmpRotation = Quaternion.LookRotation( relativePos );
		
		// 플레이어의 각도를 변경
		transform.rotation = tmpRotation;
		
	}

	// ------------------------------------------------------------------------
	// 탐색 모드 교체
	// ------------------------------------------------------------------------
	private void ChangeScanMode()
	{
		// 마우스 왼쪽이 클릭되면 탐색 모드를 ON으로 한다.
        if ( isScanOn == false ) {
			if ( Input.GetButtonDown("Fire1") ) {
				isScanOn = true;
				scoutingLaser.SendMessage( "SetIsScanOn", isScanOn );
				SendMessage( "SetFireOrder" );
			}
		}
		
		// 마우스 왼쪽 버튼을 떼면 탐색 모드를 OFF로 한다. 
		if ( isScanOn == true ) {
			if ( Input.GetButtonUp("Fire1") ) {
				isScanOn = false;
				scoutingLaser.SendMessage( "SetIsScanOn", isScanOn );
			}
		}
	}

	// ------------------------------------------------------------------------
	// 플레이어의 생존 설정
	// ------------------------------------------------------------------------
	public void SetIsAlive( bool isAlive )
	{
		this.isAlive = isAlive;
	}
	
	// ------------------------------------------------------------------------
	// 마우스 위치를 3D공간의 월드 좌표로 변환한다.
	//   - 다음 두 개가 교차하는 곳을 구한다.
	//     1. 마우스 커서와 카메라 위치가 통과하는 직선
	//     2. 조각의 중심을 지나는 수평한 면
	// ------------------------------------------------------------------------
	private Vector3	GetWorldPotitionFromMouse()
	{
		Vector3	mousePosition = Input.mousePosition;

		// 조각의 중신을 지나는 수평(법선이 Y축, XZ평면）인 면
		// 중심은 플레이어로 한다.
		Plane plane = new Plane( Vector3.up, new Vector3( 0f, 0f, 0f ) );

        // 마우스 커서와 카메라 위치가 통과하는 직선
		Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay( mousePosition );

        // 위의 두 개가 교차하는 곳을 구한다.
		float depth;
		
		plane.Raycast( ray, out depth );
		
		Vector3	worldPosition;
		
		worldPosition = ray.origin + ray.direction * depth;
		
		// Y좌표는 플레이어와 맞추어 둔다.
		worldPosition.y = 0;
		
		return worldPosition;
	}
	
	// ------------------------------------------------------------------------
	// 모든 값을 재설정한다.
	// ------------------------------------------------------------------------
	public void Reset()
	{
		// 탐색 레이저를 OFF로 한다.
		isScanOn = false;
	}
	
}