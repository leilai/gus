using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// BattleSpaceController
//  - 전투 공간(플레이어가 비행하는 공간) 스크롤을 제어한다.
//  - 사용 방법.
//    - 비어 있는 게임 오브젝트에 스크립트를 작성한다.
//    - 위의 게임오브젝트의 자식구조로서 암석「M12_asteroid」,
//     크로스 해치「T06_crossharch」를 배치한다. 
//  - 작동 방법.
//    - 플레이어의 진행방향과 역방향으로 전투 공간을 움직인다.
//    - 움직인 이동량은 다른 곳에서 참조할 수 있도록 한다.
//  - 주의 사항.
//    - 전투 공간의 경계 부근에는 암석은 배치하지 않는다.
//      (경계에서 반대쪽으로 이동한 순간에 암석이 사라져 보이도록 하기위해)
// ----------------------------------------------------------------------------
public class BattleSpaceController : MonoBehaviour {
	
	public float scrollSpeed = 3f;					// 플레이어의 진행 방향에 맞추어.
													// 전투 공간이 스크롤하는 스피드.
	
	private Vector3 additionPosition;				// 전투 공간이 이동한 이동량.
	private GameObject player;						// プレイヤーのインスタンス.
	
	private float bgX1 = -40f;						// 전투 공간의 경계(왼쪽)
    private float bgX2 = 40f;						// 전투 공간의 경계(오른쪽)
    private float bgZ1 = -40f;						// 전투 공간의 경계(위쪽)
    private float bgZ2 = 40f;						// 전투 공간의 경계(아래쪽)
	void Start () {
	
		// 플레이어의 인스턴스를 취득
		player = GameObject.FindGameObjectWithTag("Player");
		
	}
	
	void LateUpdate() {
		
		// 전투공간을 스크롤한다.
		ScrollBattleSpace();
		
	}
	
	// ------------------------------------------------------------------------
    // 전투공간을 스크롤한다.
	// ------------------------------------------------------------------------
	private void ScrollBattleSpace()
	{
		
		// 플레이어의 방향을 취득
		Quaternion playerRotation = player.transform.rotation;

        // 전투 공간을 스크롤한다.(플레이어의 방향과 반대방향으로 이동한다.)    
		additionPosition = playerRotation * Vector3.forward * scrollSpeed * Time.deltaTime;
		transform.position -= additionPosition;
		
		// 전투 공간의 루프 제어.
		IsOutOfWorld();
		
	}
	
	// ------------------------------------------------------------------------
    // 전투 공간의 루프 제어.
	// ------------------------------------------------------------------------
	private void IsOutOfWorld()
	{
		// 전투 공간의 오른쪽을 나오다.
		if ( transform.position.x < bgX1 )
		{
			// 전투 공간을 왼쪽으로 이동
			transform.position = new Vector3(
				bgX2,
				transform.position.y,
				transform.position.z );
		}
		
		// 전투 공간의 왼쪽으로 나오다.
		if ( transform.position.x > bgX2 )
		{
			// 전투 공간을 오른쪽으로 이동
			transform.position = new Vector3(
				bgX1,
				transform.position.y,
				transform.position.z );
		}
		
		// 전투 공간의 위쪽으로 나오다.
		if ( transform.position.z < bgZ1 )
		{
			// 전투 공간을 아래쪽으로 이동.
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				bgZ2 );
		}
		
		//전투 공간의 아래쪽으로 나오다.
		if ( transform.position.z > bgZ2 )
		{
			// 전투 공간을 위쪽으로 이동
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				bgZ1 );
		}
	}
	
	// ------------------------------------------------------------------------
	// 전투 공간이 이동한 이동량을 전송한다.
	// ------------------------------------------------------------------------
	public Vector3 GetAdditionPos()
	{
		return additionPosition;
	}

}
