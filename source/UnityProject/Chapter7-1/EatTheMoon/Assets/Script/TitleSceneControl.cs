using UnityEngine;
using System.Collections;

public class TitleSceneControl : MonoBehaviour {

	// 진행상태
	public enum STEP {

		NONE = -1,

		TITLE = 0,				// 타이틀 표시(버튼 대기)
		WAIT_SE_END,			// 시작 SE의 종료를 기다린다.

		NUM,
	};

	private STEP	step = STEP.NONE;
	private STEP	next_step = STEP.NONE;
	private float	step_timer = 0.0f;

	public Texture	TitleTexture = null;			// 타이틀 화면의 텍스처

	public AudioClip	audio_clip;

	public SimpleSpriteGUI	title;

	// -------------------------------------------------------------------------------- //

	void Start () {
	
		this.next_step = STEP.TITLE;

		this.audio.clip = this.audio_clip;

		this.title = new SimpleSpriteGUI();

		this.title.create();
		this.title.setTexture(this.TitleTexture);
	}

	void Update ()
	{
		this.step_timer += Time.deltaTime;

		// 다음 상태로 이동할지를 체크한다.
		switch(this.step) {

			case STEP.TITLE:
			{
				// 마우스를 클릭하였다.
				//
				if(Input.GetMouseButtonDown(0)) {
		
					this.next_step = STEP.WAIT_SE_END;
				}
			}
			break;

			case STEP.WAIT_SE_END:
			{
				// SE 의 재생이 종료되면 게임씬을 로드하여 종료

				bool	to_finish = true;

				do {

					if(!this.audio.isPlaying) {

						break;
					}

					if(this.audio.time >= this.audio.clip.length) {

						break;
					}

					to_finish = false;

				} while(false);

				if(to_finish) {

					Application.LoadLevel("GameScene");
				}
			}
			break;
		}

		// 상태를 파악하여 초기화 처리.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.WAIT_SE_END:
				{
					// 시작 SE를 재생한다.
					this.audio.Play();
				}
				break;
			}

			this.step = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// 각 상태에서의 실행 처리

		/*switch(this.step) {

			case STEP.TITLE:
			{
			}
			break;
		}*/

	}

	void OnGUI()
	{
		this.title.draw();
	}
}
