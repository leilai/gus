using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 대상 오브젝트를 화면 밖의 아래 방향에서 위로 스크롤
// ----------------------------------------------------------------------------
public class EndingScrollController : MonoBehaviour {

	public float scrollSpeed = 0.05f;				// 스크롤 하는 스피드
	public float startPosition = -0.65f;			// 스크롤 시작 위치
    public float distanceToStartEaseIn = 0.05f;		// EaseIn을 시작할 정지 위치에서의 거리.
	public bool isStoppedStarScroll = true;
	
	private float stopPositionY;					// 스크롤 정지위치.
	private GameObject targetObject;				// 스크롤 할 게임 오브젝트
	private GameObject endingSpace;
	private GameObject fadeOut;
	private bool isEaseIn = false;
	
	void Start () 
	{
		// 스코롤 할 게임 오브젝트의 인스턴스를 취득.
		targetObject = this.gameObject;
		
		// 스크롤 정지 위치를 취득.
		stopPositionY = targetObject.transform.position.y;
		
		// 메세지를 초기 표시 위치에 이동.
		Vector3 tmpPosition = targetObject.transform.position;
		targetObject.transform.position = new Vector3( tmpPosition.x, startPosition, tmpPosition.z );
	
		endingSpace = GameObject.Find("EndingSpace").gameObject;
		fadeOut = GameObject.Find("FadeOut").gameObject;
	}
	
	void FixedUpdate () 
	{	
		// 정지 위치까지 스크롤.
		Vector3 position = targetObject.transform.position;
		
		if ( isEaseIn )
		{
            // .EaseIn
			position.y +=
				( Mathf.Abs( stopPositionY - position.y ) / distanceToStartEaseIn )
					*  scrollSpeed * Time.deltaTime;
			targetObject.transform.position = new Vector3( position.x, position.y, position.z );
		}
		else
		{
			if ( Mathf.Abs( stopPositionY - position.y ) < distanceToStartEaseIn )
			{
                //EaseIn시작
				isEaseIn = true;
				if ( isStoppedStarScroll )
				{
					endingSpace.SendMessage("SetEaseIn");
					fadeOut.SendMessage("SetEnable");
				}
			}
			else
			{
				// 스크롤
				position.y += scrollSpeed * Time.deltaTime;
				targetObject.transform.position = new Vector3( position.x, position.y, position.z );
			}
		}
	}
}
