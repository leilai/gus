using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// SCORE 표시
// ----------------------------------------------------------------------------
public class PrintScore : MonoBehaviour {
	
	private int score = 0;
	private GUIText textScore;
	
	void Start () 
	{
		// score 인스턴스를 취득
		textScore = GetComponent<GUIText>();
		
		// 초기값 표시
		textScore.text = score.ToString();
	}
	
	// ------------------------------------------------------------------------
	// SCORE에 덧셈
	// ------------------------------------------------------------------------
	public void AddScore( int score )
	{
		// 덧셈
		this.score += score;
		
		// 표시
		textScore.text = this.score.ToString();
	}
	
	// ------------------------------------------------------------------------
	// SCORE를 보낸다.
	// ------------------------------------------------------------------------
	public int GetScore()
	{
		return score;
	}
		
}
