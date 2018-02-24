using UnityEngine;
using System.Collections;

public class Launcher : MonoBehaviour {

	public GameObject	ball_prefab;				// 공의 프리팹.
	private Player		player;						// 플레이어.
	private string		result = "";				// 직전의 결과.

	private bool		is_launch_ball = false;		// 「공 발사」플래그.

	private string[ ]	good_mess;					// 성공인 경우의 메세지.
    private int			good_mess_index;			// 다음에 표시할 성공 메세지.

	// ---------------------------------------------------------------- //

	void Start ()
	{
		this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

		this.is_launch_ball = true;

		//

		this.good_mess = new string[4];

		this.good_mess[0] = "Nice!";
		this.good_mess[1] = "Okay!";
		this.good_mess[2] = "Yatta!";
		this.good_mess[3] = "*^o^*v";

		this.good_mess_index = 0;

	}

	void Update ()
	{
		// 「공 발사」플래그가 만들어지고, 플레이어가 착지중이라면…….	
		if(this.is_launch_ball && this.player.isLanded()) {

			//

			GameObject ball = Instantiate(ball_prefab) as GameObject;

			ball.transform.position = new Vector3(5.0f, 2.0f, 0.0f);	

			//

			float		x_speed;
			float		y_speed;
			float		height;

			//　X방향 스피드와 최고도달점의 높이를 랜덤하게 구한다.

			// 전회차의 값과 어느 정도 차이가 나도록 정수를 사용한다.
			x_speed = -Random.Range(2, 7)*2.5f;

			height  =  Random.Range(2.0f, 3.0f);

			// 공의 초기속도인 Y성분을, X방향 스피드와 플레이어 위치의 높이로 구한다.
			y_speed = this.calc_ball_y_speed(x_speed, height, ball.transform.position);

			Vector3		velocity = new Vector3(x_speed, y_speed, 0.0f);

			ball.GetComponent<Ball>().velocity = velocity;

			// 「공 발사」플래그를 내린다.
			this.is_launch_ball = false;
		}
	}

	// 공의 초기속도인 Y성분을, X방향 스피드와 플레이어의 위치의 높이로 구한다.
	private float	calc_ball_y_speed(float x_speed, float height, Vector3 ball_position)
	{
		float		t;
		float		y_speed;

		// 플레이어의 위치에 도착하기까지의 시간.
		t = (this.player.transform.position.x - ball_position.x)/x_speed;

		// y = v*t - 0.5f*g*t*t
		// 에서 v 를 구한다.
		y_speed = ((height - ball_position.y) - 0.5f*Physics.gravity.y*t*t)/t;

		return(y_speed);
	}

	// 성공/실패를 세팅한다.
	public void setResult(bool is_success)
	{
		if(is_success) {

			// 성공이라면 성공 메세지를 순서대로 표시.

			this.result = this.good_mess[this.good_mess_index];

			this.good_mess_index = (this.good_mess_index + 1)%4;

		} else {

			this.result = "Miss!";
		}

		// 일정 시간이 경과한 후에 결과표시를 지운다.
		StartCoroutine(clearResult());
	}

	// 일정 시간이 경과한 후에 결과표시를 지우기 위한 처리.
	private IEnumerator clearResult()
	{
		yield return new WaitForSeconds(0.5f);

		this.result = "";
	}

	public void OnGUI()
	{
		GUI.Label(new Rect(200, 200, 200, 20), this.result);
	}

	// 공이 삭제되는 경우.
	public void OnBallDestroy()
	{
		// 「공 발사」플래그를 만든다.
		is_launch_ball = true;
	}
}
