using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 별을 동작시킨다.
//  - 방법
//    - 별이 그려진 1장의 그림을 3장 중복시켜, 각각 지정한 스피드로 플레이어와 방향과 반대 방향으로 동작시킨다.
//  - 이용방법
//    - 스크립트를 임의의 게임오브젝트에 설정한다.
//  - 사전 준비
//    - 별 텍스처를 붙인 평면(이하, 별판)을 준비           
//    - 별판 텍스처의 Tiling을 x,y방향 모두 3으로 한다.
//    - 별판에 Star1,Star2,Star3 을 각각 태그를 붙인다.
//    - 별판을 X축 방향으로 움직여 이동전과 같은 그림이 되는 위치를 찾아, 그 값을 maxRightPositionX,maxLeftPositionX 에 설정
//    - 별판을 Z축 방향으로 움직여 이동전과 같은 그림이 되는 위치를 찾아, 그 값을 maxTopPositionZ,maxBottomPositionZ 에 설정
//  - 비고
//    - 연속적으로 별을 움직이기 위해 지정한 좌표에서 별판의 위치를 되돌린다.
//    - 원만하게 교체하기 위해, 교체 좌표는 별판을 실제로 움직여 확인해 보는 것이 포인트
// ----------------------------------------------------------------------------
public class StarController : MonoBehaviour {

	public float scrollSpeedStar1 = 0.2f;					// 별(Star1)이 스크롤하는 스피드.
    public float scrollSpeedStar2 = 0.5f;					// 별(Star2)이 스크롤하는 스피드.
    public float scrollSpeedStar3 = 1f;						// 별(Star3)이 스크롤하는 스피드.
	
	private GameObject player;								// 플레이어의 인스턴스
	
	const int MAX_STARS = 3;								// 별판의 수
	private GameObject[] stars = new GameObject[MAX_STARS];	// 별판의 인스턴스
	private float[] scrollSpeed = new float[MAX_STARS];		// 별판의 스크롤 스피드.
	
	private float maxRightPositionX = -10f;					// 별판 교체 위치X.
    private float maxLeftPositionX = 10f;					// 별판 교체 위치X.
    private float maxTopPositionZ = -10f;					// 별판 교체 위치Z.
    private float maxBottomPositionZ = 10f;					// 별판 교체 위치Z.
	
	void Start ()
	{
		// 플레이어의 인스턴스 취득
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 별판의 인스턴스 취득
		GameObject star1 = GameObject.FindGameObjectWithTag("Star1");
		GameObject star2 = GameObject.FindGameObjectWithTag("Star2");
		GameObject star3 = GameObject.FindGameObjectWithTag("Star3");
		
		// 통합하여 처리하기 쉽도록 배열에 통합한다.
		stars[0] = star1;
		stars[1] = star2;
		stars[2] = star3;
		scrollSpeed[0] = scrollSpeedStar1;
		scrollSpeed[1] = scrollSpeedStar2;
		scrollSpeed[2] = scrollSpeedStar3;
	}
	
	// 플레이어의 진행방향이 확정된 후에 처리한다.
	void LateUpdate() 
	{
		// 별을 스크롤(플레이어 진행방향의 반대방향으로 별을 움직인다.)               
		ScrollStars();
	}
	
	// ------------------------------------------------------------------------
    // 별을 스크롤(플레이어 진행방향의 반대방향으로 별을 움직인다.)               
	// ------------------------------------------------------------------------
	private void ScrollStars()
	{
		// Player가 존재하지 않은 경우는 처리종료.
		if ( !player )
		{
			return;
		}
		
		// 플레이어의 방향을 취득.
		Quaternion playerRot = player.transform.rotation;

		// 별을 플레이어의 방향과 반대 방향으로 이동한다.
		for( int i = 0; i < MAX_STARS; i++ )
		{
			if ( !stars[i] || scrollSpeed[i] == 0 )
			{
				// 별판의 인스턴스 또는 스크롤하는 스피드가 미설정된 경우에는 처리하지 않는다.                             
				continue;
			}
			
			// 별을 스크롤
			Vector3 additionPos = playerRot * Vector3.forward * scrollSpeed[i] * Time.deltaTime;
			stars[i].transform.position -= additionPos;
			
			// 별의 스크롤 루프 제어.
			IsOutOfWorld( stars[i] );
		}
	}
	
	// ------------------------------------------------------------------------
	// 별의 교체 위치가 되는 경우의 루프 제어.
	// ------------------------------------------------------------------------
	private void IsOutOfWorld( GameObject star )
	{
		if ( star.transform.position.x < maxRightPositionX )
		{
			star.transform.position = new Vector3(
				maxLeftPositionX,
				star.transform.position.y,
				star.transform.position.z );
		}
		if ( star.transform.position.x > maxLeftPositionX )
		{
			star.transform.position = new Vector3(
				maxRightPositionX,
				star.transform.position.y,
				star.transform.position.z );
		}
		if ( star.transform.position.z < maxTopPositionZ )
		{
			star.transform.position = new Vector3(
				star.transform.position.x,
				star.transform.position.y,
				maxBottomPositionZ );
		}
		if ( star.transform.position.z > maxBottomPositionZ )
		{
			star.transform.position = new Vector3(
				star.transform.position.x,
				star.transform.position.y,
				maxTopPositionZ );
		}
	}
}
