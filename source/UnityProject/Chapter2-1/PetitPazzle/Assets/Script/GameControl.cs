using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

	enum STEP {

		NONE = -1,

		PLAY = 0,		// 플레이 중.
		CLEAR,			// 클리어.

		NUM,
	};

	private STEP	step      = STEP.PLAY;
	private STEP	next_step = STEP.NONE;

	private float		step_timer = 0.0f;

	// -------------------------------------------------------- //

	public GameObject		pazzlePrefab = null;

	public	PazzleControl	pazzle_control = null;

	private	SimpleSpriteGUI	finish_sprite = null;
	public	Texture			finish_texture = null;

	private RetryButtonControl	retry_button = null;

	// -------------------------------------------------------- //

	public enum SE {

		NONE = -1,

		GRAB = 0,		// 조각을 집었을 경우.
		RELEASE,		// 조각을 놓았을 경우(정답이 아닌 경우).

        ATTACH,			// 조각을 놓았을 경우(정답인 경우).

		COMPLETE,		// 퍼즐을 완성했을 때의 음악.

		BUTTON,			// GUI 의 버튼.

		NUM,
	};

	public AudioClip[]	audio_clips;

	// -------------------------------------------------------- //

	// Use this for initialization
	void Start () {

		this.pazzle_control = (Instantiate(this.pazzlePrefab) as GameObject).GetComponent<PazzleControl>();

		// 『다시하기』버튼의 그림 표시 순서를 세팅한다.
		// （조각의 뒷면）.

		this.retry_button = GameObject.FindGameObjectWithTag("RetryButton").GetComponent<RetryButtonControl>();

		this.retry_button.renderer.material.renderQueue = this.pazzle_control.GetDrawPriorityRetryButton();


		this.finish_sprite = new SimpleSpriteGUI();
		this.finish_sprite.create();
		this.finish_sprite.setTexture(this.finish_texture);
	}

	// Update is called once per frame
	void Update () {
	
		// ---------------------------------------------------------------- //

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 상태 변화 점검.

		switch(this.step) {

			case STEP.PLAY:
			{
				if(this.pazzle_control.isCleared()) {

					this.next_step = STEP.CLEAR;
				}
			}
			break;

			case STEP.CLEAR:
			{
				if(this.step_timer >this.audio_clips[(int)SE.COMPLETE].length + 0.5f) {

					if(Input.GetMouseButtonDown(0)) {

						Application.LoadLevel("TitleScene");
					}
				}
			}
			break;
		}


		// ---------------------------------------------------------------- //
		// 변화시 초기화.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.CLEAR:
				{
					this.retry_button.renderer.enabled = false;
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 실행 처리.

		switch(this.step) {

			case STEP.PLAY:
			{
			}
			break;
		}
	}
	void	OnGUI()
	{
		if(pazzle_control.isDispCleard()) {

			this.finish_sprite.draw();
		}
	}

	public void	playSe(SE se)
	{
		this.audio.PlayOneShot(this.audio_clips[(int)se]);
	}

	// 『다시하기』버튼을 눌렀을 때의 처리.
	public void	OnRetryButtonPush()
	{
		if(!this.pazzle_control.isCleared()) {

			this.playSe(GameControl.SE.BUTTON);

			this.pazzle_control.beginRetryAction();
		}
	}
}
