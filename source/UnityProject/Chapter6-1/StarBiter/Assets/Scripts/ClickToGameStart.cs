using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 클릭으로 게임시작.
//  - 사용 방법
//    - 스크립트를 메인 카메라에 첨부한다.
//    - 메인 카메라에 클릭음을 첨부한다.
//  - 동작 방법
//    - 클릭으로 게임씬을 불러온다.
// ----------------------------------------------------------------------------
public class ClickToGameStart : MonoBehaviour {
	
	private string gameSceneName = "game";	// 게임 씬 제목
	private GameObject mainCamera;			// 메인 카메라
	
	void Start ()
	{	
		// 메인 카메라의 인스턴스를 취득.
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	void Update ()
	{	
		// 마우스 왼쪽 버튼 클릭.
		if ( Input.GetButtonDown("Fire1") )
		{
			// 버튼을 누를 때의 소리 재생.
			mainCamera.audio.Play();
			
			// 게임 씬을 불러온다.
			Application.LoadLevel( gameSceneName );
		}
	}
}
