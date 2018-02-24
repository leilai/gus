using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

    public Launcher launcher;		    // Launcher의 프리팹.

	public	Vector3		velocity;		// 초기속도（Launcher로부터 받는 용도）.
	public	bool		is_touched;		// 플레이어에게 닿았는가?.
	private	float		time;			// 실행중 타이머.
	private bool		is_launched;	// 발사되었는가？（false 이라면 페이드인 중）.

	// ---------------------------------------------------------------- //

	void Start ()
	{
        // Launcher의 게임 오브젝트를 준비한다.
		this.launcher = GameObject.FindGameObjectWithTag("Launcher").GetComponent<Launcher>();

		// 알파로 보이지 않도록 한다.

		Color	color = this.renderer.material.color;

		color.a = 0.0f;

		this.renderer.material.color = color;

		//

		this.rigidbody.useGravity = false;

		this.is_touched = false;
		this.is_launched = false;

		this.time = 0.0f;
	}

	void Update ()
	{
		// 발사되기 전이라면 (페이드인 중이라면)…….
		if(!this.is_launched) {

			float	delay = 0.5f;

			// 알파로 페이드인한다.

			Color color = this.renderer.material.color;

			// [0.0f ～ delay] 의 범위를 [0.0f ～ 1.0f] 으로 변경한다.
			float	t = Mathf.InverseLerp(0.0f, delay, this.time);

			t = Mathf.Min(t, 1.0f);

			color.a = Mathf.Lerp(0.0f, 1.0f, t);

			this.renderer.material.color = color;

			// 일정시간이 경과하면, 발사.
			if(this.time >= delay) {

				this.rigidbody.useGravity = true;
				this.rigidbody.velocity = this.velocity;

				this.is_launched = true;
			}
		}

		this.time += Time.deltaTime;

		DebugPrint.print(this.rigidbody.velocity.ToString());
	}

	// 화면 밖에 표시되는 경우.
	void	OnBecameInvisible()
	{
        // 공이 삭제된 사실을 Launcher에게 통지.
		this.launcher.OnBallDestroy();

		// 플레이어와 닿지 않았다면(헛스윙) 미스.
		if(!this.is_touched) {

			if(this.launcher != null) {

				this.launcher.setResult(false);
			}
		}

		// 게임오브젝트를 삭제.
		Destroy(this.gameObject);
	}

	// 다른 오브젝트와 충돌한 경우.
	void OnCollisionEnter(Collision collision)
	{
		// 충돌한 상대가 플레이어인 경우.
        if(collision.gameObject.tag == "Player") {

			if(collision.gameObject.GetComponent<Player>().isLanded()) {

				// 플레이어가 착지중이었다면 미스.

				this.launcher.setResult(false);

				// 플레이어가 터치했던 것을 기억해둔다.
				this.is_touched = true;

			} else {

				// 플레이어가 점프중이라면 성공.

				this.launcher.setResult(true);
				this.is_touched = true;
			}
		}
	}
}
