using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// RKeyPressToOpening
//  - 오프닝 씬을 불러온다.
//  - 사용 방법
//    - 스크립트를 메인 카메라에 첨부한다.
//  - 작동 방법
//    - R키를 누르면 오프닝 씬을 불러온다.
// ---------------------------------------------------------------------------
public class RKeyPressToOpening : MonoBehaviour {

	private string gameSceneName = "opening";	// 게임 씬 제목

	void Update () {
	
		// 클릭
		if ( Input.GetKeyDown(KeyCode.R) )
		{
			// 게임 씬을 불러온다.
			Application.LoadLevel( gameSceneName );
		}
			
	}
}
