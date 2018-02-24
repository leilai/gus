using UnityEngine;
using System.Collections;

public class Launcher : MonoBehaviour {

	// 공의 프리팹.
	// 인스펙터에서 값을 세팅한다.
	public GameObject	ballPrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		// 마우스 왼쪽 버튼을 누르면 (누르는 순간)…….		
		if(Input.GetMouseButtonDown(1)) {

			//공의 프리팹을 인스턴스화한다.(게임 오브젝트를 작성한다.)
			Instantiate(this.ballPrefab);
		}	
	}
}
