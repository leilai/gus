using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 게임 내 정보 저장(GlobalParam은 Unity실행 시작 후에 저장할 영역).
//  - HISCORE를 저장한다./전송한다.
// ----------------------------------------------------------------------------
public class GlobalParam : MonoBehaviour {
	
	private int hiScore = 0;
	private static GlobalParam instance = null;
	
	// GlobalParam은 1번만 작성하여 인스턴스를 전송한다.
	// (작성 후는 작성완료된 인스턴스를 전송한다.).
	public static GlobalParam GetInstance()
	{
		if ( instance == null )
		{
			GameObject globalParam = new GameObject("GlobalParam");
			instance = globalParam.AddComponent<GlobalParam>();
			DontDestroyOnLoad( globalParam );
		}
		return instance;
	}
	
	// HISCORE를 전송한다.
	public int GetHiScore()
	{
		return hiScore;
	}
	
	// HISCORE를 저장한다.
	public void SetHiScore( int hiScore )
	{
		this.hiScore = hiScore;
	}
}
