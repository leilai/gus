using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// HISCORE 표시
// ----------------------------------------------------------------------------
public class PrintHiScore : MonoBehaviour {

	private int hiScore = 0;
	private GUIText textHiScore;
	
	void Start () 
	{
		// hi-score 인스턴스를 취득
		textHiScore = GetComponent<GUIText>();
		
		// 글로벌 파라미터에서 hi-score를 취득
		hiScore = GlobalParam.GetInstance().GetHiScore();		
		
		// 초기값 표시
		textHiScore.text = hiScore.ToString();
	}
	
	// ------------------------------------------------------------------------
	// HISCORE 설정
	// ------------------------------------------------------------------------
	public void SetHiScore( int hiScore )
	{
		// 저장
		this.hiScore = hiScore;
	
		// 표시
		textHiScore.text = this.hiScore.ToString();
	}

}
