using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// ClickToOpening
//  - 오프닝을 불러온다.   
//  - 사용 방법
//    - 스크립트를 메인카메라에 첨부한다.
//  - 동작 방법
//    - 클릭으로 오프닝을 불러온다. 
// ---------------------------------------------------------------------------
public class ClickToOpening : MonoBehaviour {

	private string gameSceneName = "opening";	// ゲームシーン名.

	void Update ()
	{
		// 클릭  
		if ( Input.GetButtonDown("Fire1") ) {
			// 게임 씬을 불러온다.
			Application.LoadLevel( gameSceneName );
		}
			
	}
}
