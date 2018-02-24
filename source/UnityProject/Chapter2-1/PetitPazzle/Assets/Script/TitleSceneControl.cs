using UnityEngine;
using System.Collections;

public class TitleSceneControl : MonoBehaviour {

	enum STEP {

		NONE = -1,

		WAIT = 0,		// 클릭 대기중.
		PLAY_JINGLE,	// 시작 음악 재생중.

		NUM,
	};

	private STEP	step      = STEP.WAIT;
	private STEP	next_step = STEP.NONE;

	private float		step_timer = 0.0f;

	// -------------------------------------------------------- //

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		// ---------------------------------------------------------------- //

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 상태 변화 점검.

		switch(this.step) {

			case STEP.WAIT:
			{
				if(Input.GetMouseButtonDown(0)) {

					this.next_step = STEP.PLAY_JINGLE;
				}
			}
			break;

			case STEP.PLAY_JINGLE:
			{
				// SE의 재생이 종료되면 게임씬을 로드하여 종료.

				if(this.step_timer > this.audio.clip.length + 0.5f) {

					Application.LoadLevel("GameScene0");
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 변화시 초기화.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.PLAY_JINGLE:
				{
					this.audio.Play();
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 실행처리.

		switch(this.step) {

			case STEP.WAIT:
			{
			}
			break;
		}

	}

	void OnGUI()
	{
	}
}
