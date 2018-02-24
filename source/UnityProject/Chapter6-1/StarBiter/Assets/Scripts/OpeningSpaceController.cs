using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 배경인 별빛 하늘 스크롤
//  - 위에서 아래로 스크롤 하기
// ----------------------------------------------------------------------------
public class OpeningSpaceController : MonoBehaviour {

	public float scrollSpeedStar1 = 0.2f;			// 별이 스크롤 하는 스피드
    public float scrollSpeedStar2 = 0.5f;			// 별이 스크롤 하는 스피드
    public float scrollSpeedStar3 = 1f;				// 별이 스크롤 하는 스피드
	
	const int MAX_STARS = 3;
	private GameObject[] stars = new GameObject[MAX_STARS];	// 별
    private float[] scrollSpeed = new float[MAX_STARS];		// 별의 스크롤 하는 스피드
	
	private float bgZ1 = -10f;						// 전투 공간의 경계(위쪽).
    private float bgZ2 = 10f;						// 전투 공간의 경계(아래쪽).

    private bool isEaseIn = false;					// EaseIn
	private float easeInRate = 0.6f;				// 감소해 가는 비율
	
	void Start ()
	{
		// 별의 인스턴스를 취득
		GameObject star1 = GameObject.FindGameObjectWithTag("Star1");
		GameObject star2 = GameObject.FindGameObjectWithTag("Star2");
		GameObject star3 = GameObject.FindGameObjectWithTag("Star3");
		stars[0] = star1;
		stars[1] = star2;
		stars[2] = star3;
		
		// 별의 스크롤 스피드를 설정
		scrollSpeed[0] = scrollSpeedStar1;
		scrollSpeed[1] = scrollSpeedStar2;
		scrollSpeed[2] = scrollSpeedStar3;
	}

	void LateUpdate()
	{
		// 별을 스크롤(위에서 아래로)
		Scroll();
	}
	
	// ------------------------------------------------------------------------
    // 별을 스크롤(위에서 아래로 움직인다.)
	// ------------------------------------------------------------------------
	private void Scroll()
	{
		// 별을 Z축의 마이너스 방향으로 이동한다.
		for( int i = 0; i < MAX_STARS; i++ )
		{
			if ( !stars[i] || scrollSpeed[i] == 0 )
			{
				// 별 게임 오브젝트 또는 스크롤 스피드가 미설정된 경우에는 처리하지 않는다. 
				continue;
			}
			
			// 별을 이동한다.
			Vector3 additionPos = new Vector3( 0, 0, 1f )  * scrollSpeed[i] * Time.deltaTime;
			stars[i].transform.position -= additionPos;
			
			// 별의 루프 제어
			IsOutOfWorld( stars[i] );

            // isEaseIn 
			if ( isEaseIn )
			{
				scrollSpeed[i] -= scrollSpeed[i] * easeInRate * Time.deltaTime;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 메인 카메라의 표시 영역이 별의 영역에서 제외되는 경우의 루프 제어
	// ------------------------------------------------------------------------
	private void IsOutOfWorld( GameObject star )
	{
		
		if ( star.transform.position.z < bgZ1 )
		{
			star.transform.position = new Vector3(
				star.transform.position.x,
				star.transform.position.y,
				bgZ2 );
		}

	}
	
	public void SetEaseIn()
	{
		isEaseIn = true;
	}
	
}
