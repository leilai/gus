using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		this.rigidbody.velocity = new Vector3(-10.0f, 9.0f, 0.0f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// 다른 게임 오브젝트가 화면 밖에 표시된 경우.
	void OnBecameInvisible()
	{
		// 게임 오브젝트를 삭제한다.
		// 인수에 주의!
		// Destory(this) 에서는 컴포넌트만 삭제된다.
		Destroy(this.gameObject);
	}
}
