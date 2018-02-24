using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 대상 오브벡트를 가로 방향으로 스크롤                     
// ----------------------------------------------------------------------------
public class EndingSideScrollController : MonoBehaviour {

	public float scrollSpeed = 0.05f;				// 스크롤 하는 스피드.
	public float startPositionX = -0.65f;			// 스크롤 시작 위치
	public float startPositionY = -0.65f;			// 스크롤 시작 위치
    public float distanceToStartEaseIn = 0.05f;		// EaseIn을 시작할 정지 위치에서의 거리.
	public float triggerParentPositionY = 0;		// 부모 위치를 이동시작의 trigger로 한다.
	
	private bool isScrolling = false;
	private float stopPositionX;					// 스크롤 정지 위치
	private GameObject targetObject;				// 스크를 할 게임 오브젝트.
	
	void Awake () 
	{
		// 스크롤 할 게임 오브젝트의 인스턴스를 취득.
		targetObject = this.gameObject;
		
		// 스크롤 정지 위치를 취득.
		stopPositionX = targetObject.transform.position.x;
		
		// 게임오브젝트를  초기 표시 위치로 이동.
		Vector3 tmpPosition = targetObject.transform.position;
		targetObject.transform.position = new Vector3( startPositionX, startPositionY, tmpPosition.z );
	}
	
	void FixedUpdate () 
	{	
		if ( !isScrolling )
		{
			if ( transform.parent.transform.position.y < triggerParentPositionY )
			{
				isScrolling = true;
			}
		}
		else
		{
			// 정지 위치까지 스크롤.
			Vector3 position = targetObject.transform.position;
			
			if ( Mathf.Abs( stopPositionX - position.x ) < distanceToStartEaseIn )
			{
                // EaseIn.
				float additionDistance =
					( Mathf.Abs( stopPositionX - position.x ) / distanceToStartEaseIn )
						*  scrollSpeed * Time.deltaTime;
				position.x += additionDistance;
				position.y += additionDistance;
				targetObject.transform.position = new Vector3( position.x, position.y, position.z );
			}
			else if ( Mathf.Abs( stopPositionX - position.x ) > 0 )
			{
				// 스크롤
				float additionDistance = scrollSpeed * Time.deltaTime;
				position.x += additionDistance;
				position.y += additionDistance;
				targetObject.transform.position = new Vector3( position.x, position.y, position.z );
			}
		}
	}
}
