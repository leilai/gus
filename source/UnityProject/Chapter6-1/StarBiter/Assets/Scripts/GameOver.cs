using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 게임 오버 시의 메세지 표시 처리.
// ----------------------------------------------------------------------------
public class GameOver : MonoBehaviour {
	
	public GameObject MessageMission;
	public GameObject MessageAccomplished;
	public GameObject MessageAccomplishedLine1;
	public GameObject MessageAccomplishedLine2;
	public GameObject MessageAccomplishedLine3;
	public GameObject MessageGameOver;
	public GameObject MessageHiScore;
	
	private bool isHiScore = false;		// HISCORE인가?
	private bool defeatedBoss = false;	// BOSS를 쓰러트렸는가?
	private bool isEnable = false;		// 본 기능을 유효화할 것인가?
	
	void Update () {
		
		if ( isEnable )
		{
			// 미션 수행했는지 게임 오버의 메세지에 표시                  
			if ( defeatedBoss )
			{
				// 미션 수행의 문자를 표시
				Instantiate( MessageMission, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplished, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplishedLine1, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplishedLine2, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplishedLine3, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
			}
			else
			{
				// 게임오버 문자를 표시
				Instantiate( MessageGameOver, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
			}
			
			// HISCORE표시
			if ( isHiScore )
			{
				// HISCORE표시처리
				StartCoroutine( WaitAndPrintHiScoreMessage( 0.5f ) );
			}
			
			// 표시처리는 1회만
			isEnable = false;
		}
	}
	
	// ------------------------------------------------------------------------
	// HISCORE인지 아닌지 상태를 저장
	// ------------------------------------------------------------------------
	public void SetIsHiScore( bool isHiScore )
	{
		this.isHiScore = isHiScore;
	}
	
	// ------------------------------------------------------------------------
	// BOSS를 쓰러트렸는지 아닌지 상태를 저장
	// ------------------------------------------------------------------------
	public void SetDefeatedBoss( bool defeatedBoss )
	{
		this.defeatedBoss = defeatedBoss;
	}
	
	// ------------------------------------------------------------------------
	// 메세지를 표시하는 플래그를 설정한다..
	// ------------------------------------------------------------------------
	public void Show()
	{
		isEnable = true;
	}
	
	// ------------------------------------------------------------------------
	// HISCORE시의 메세지 표시.
	//  일정 시간 표시 타이밍을 늦춘다.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndPrintHiScoreMessage( float waitForSeconds )
	{
		// 일정 시간 기다린다.
		yield return new WaitForSeconds( waitForSeconds );

		// HISCORE의 메세지를 표시
		Instantiate( MessageHiScore, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
	}
}
