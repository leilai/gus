using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// StoneController
//  - 암석「M12_asteroid」의 움직임을 제어한다.
//  - 사용 방법
//    - 스크립트가 첨부된 암석을 배치한다.
//  - 작동 방법
//    - 랜덤한 방향으로 랜덤한 스피드로 회전한다.
// ---------------------------------------------------------------------------
public class StoneController : MonoBehaviour {
	
	private float rotateSpeed = 0;			// 암석의 회전 스피드
	
	void Start () 
	{
	
		// 암석의 방향을 랜덤하게 설정
		transform.rotation = new Quaternion(
			Random.Range( 0, 360 ),
			Random.Range( 0, 360 ),
			Random.Range( 0, 360 ),
			Random.Range( 0, 360 ));
		
		// 암석의 회전 스피드를 랜덤하게 설정
		rotateSpeed = Random.Range( 0.01f, 3f );
		
		// 암석을 공격대상으로 설정.
		SendMessage( "SetIsAttack", true );
	}
	
	void Update ()
	{
	
		// 암석을 회전시킨다.
		transform.Rotate( new Vector3( 0, rotateSpeed, 0 ) );
		
	}
}
