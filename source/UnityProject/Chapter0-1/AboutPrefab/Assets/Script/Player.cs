using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private bool	is_landed;				// 착지중？.
	public float	JumpHeight = 4.0f;		// 점프 높이.

	// ---------------------------------------------------------------- //

	void Start ()
	{
		this.is_landed = false;
	}

	void Update ()
	{
		// 착지중이라면…….
		if(this.is_landed) {

			// 마우스 오른쪽 버튼을 누르면…….
			if(Input.GetMouseButtonDown(0)) {

				this.is_landed = false;
	
				// 점프 높이로 초기 속도를 구한다.
				float	y_speed = Mathf.Sqrt(2.0f*Mathf.Abs(Physics.gravity.y)*this.JumpHeight);

				this.rigidbody.velocity = Vector3.up*y_speed;
			}
		}
	}

	// 인수가 맞지 않으면 제대로 불러올 수 없으므로 주의.
	void OnCollisionEnter(Collision collision)
	{
		// Debug.Log  사용 방법
		// 모든 오브젝트를 Debug.Log 할 때에는 "ToString()" 메소드로
		// float 등도 ToString() 으로.
		Debug.Log(collision.gameObject.ToString());

		// 이러한 처리를 하지 않으면 공과 충돌하였을 때에 this.is_landed 가 true 가 된다.
		if(collision.gameObject.tag == "Floor") {

			this.is_landed = true;
		}
	}

	// 착지중？.
	public bool	isLanded()
	{
		return(this.is_landed);
	}
}
