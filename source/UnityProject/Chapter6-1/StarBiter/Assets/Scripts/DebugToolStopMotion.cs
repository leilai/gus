using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 디버그 툴 
//  - P 키로 게임 진행을 일시 정지한다.
// ----------------------------------------------------------------------------
public class DebugToolStopMotion : MonoBehaviour {

	private float origTimeScale;
	
	// 다른 오브젝트보다 빠르게 timeScale을 취득한다.
	void Awake()
	{
		origTimeScale = Time.timeScale;
	}
	
	void Update () 
	{
		// 클릭   
		if ( Input.GetKeyDown(KeyCode.P) )
		{
			if ( Time.timeScale != 0 )
			{
				Time.timeScale = 0;
			}
			else
			{
				Time.timeScale = origTimeScale;
			}
		}
	}
}
