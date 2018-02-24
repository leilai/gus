using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	protected float	jump_speed = 12.0f;			// 점프 초기속도
	public bool		is_landing = false;			// 착지중?.

	// Use this for initialization
	void Start () {

		this.is_landing = false;	
	}
	
	// Update is called once per frame
	void Update ()
	{
        // 착지중이라면…….
		if(this.is_landing) {

            // 마우스 왼쪽 버튼을 누르면(누르는 순간)…….
			if(Input.GetMouseButtonDown(0)) {

                //  착지 플래그를 false으로 (착지하지 않는다.).
				this.is_landing = false;

				//  상향의 속도로 점프한다.
				this.rigidbody.velocity = Vector3.up*this.jump_speed;

				// 왼쪽 버튼을 클릭한 순간(점프 순간)에
				// 게임이 정지하게 된다.
				//Debug.Break();
			}
		}	
	}

	// 다른 게임 오브젝트와 충돌한 경우.
	void OnCollisionEnter(Collision collision)
	{
        // 충돌 대상의 태그가 "Floor"라면…….
		if(collision.gameObject.tag == "Floor") {

			// 착지한다.
			this.is_landing = true;
		}
	}
}
