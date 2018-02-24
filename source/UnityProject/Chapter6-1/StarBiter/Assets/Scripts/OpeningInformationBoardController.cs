using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 오프닝 화면에서 표시할 메세지를 화면 밖의 아래 방향에서 위로 스크롤
// ----------------------------------------------------------------------------
public class OpeningInformationBoardController : MonoBehaviour {
	
	public float scrollSpeed = 0.2f;				// 타이틀이 스크롤하는 스피드
	public float startPosition = -0.2f;				// 스크롤 시작위치
	
	private float stopPositionY;					// 타이틀이 정지할 위치
	private GameObject informationBoard;			// 메세지
	
	void Start () 
	{
		// 메세지의 인스턴스를 취득.
		informationBoard = GameObject.FindGameObjectWithTag("InformationBoard");
		
		// 스크롤 정지위치를 취득
		stopPositionY = informationBoard.transform.position.y;
		
		// 메세지를 초기 표시 위치에 이동
		Vector3 tmpPosition = informationBoard.transform.position;
		informationBoard.transform.position = new Vector3( tmpPosition.x, startPosition, 0 );
	}
	
	void Update () 
	{
		// 정지 위치까지 스크롤
		Vector3 position = informationBoard.transform.position;
		if ( position.y < stopPositionY )
		{
			position.y += scrollSpeed * Time.deltaTime;
			informationBoard.transform.position = new Vector3( position.x, position.y, 0 );
		}
	}
}
