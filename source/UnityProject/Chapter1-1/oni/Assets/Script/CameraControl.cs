using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	// 플레이어.
	private GameObject	player = null;

	public Vector3		offset;

	// Use this for initialization
	void Start () {

		// 플레이어의 인스턴스를 준비한다.
		this.player = GameObject.FindGameObjectWithTag("Player");

		this.offset = this.transform.position - this.player.transform.position;
	}
	
	// Update is called once per frame
	void Update () {

		// 플레이어와 함께 이동
		this.transform.position = new Vector3(player.transform.position.x + this.offset.x, this.transform.position.y, this.transform.position.z);

	}
}
