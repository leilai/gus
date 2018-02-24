using UnityEngine;
using System.Collections;

public class TitleSceneControl : MonoBehaviour {

	// 진행상태.
	public enum STEP {

		NONE = -1,

		TITLE = 0,				// 타이틀 표시(버튼 대기).
		WAIT_SE_END,			// 스타트 SE가 종료하는 것을 기다린다.
		FADE_WAIT,				// 페이드 종료 대기.

		NUM,
	};

	private STEP	step = STEP.NONE;
	private STEP	next_step = STEP.NONE;
	private float	step_timer = 0.0f;

	public Texture	TitleTexture = null;			// 『시작』 텍스처
    public Texture StartButtonTexture = null;			// 『시작』 텍스처
	private FadeControl	fader = null;					// 페이트 컨트롤
	
	// 타이틀 화면
	public float	title_texture_x		=    0.0f;
	public float	title_texture_y		=  100.0f;
	
	// 『시작』문자.
	public float	start_texture_x		=    0.0f;
	public float	start_texture_y		= -100.0f;
	
	// 시작이 실행되는 때에 애니메이션을 할 시간.
	private static float	TITLE_ANIME_TIME = 0.1f;
	private static float	FADE_TIME = 1.0f;
	
	// -------------------------------------------------------------------------------- //

	void Start () {
		// 플레이어를 조작 불가능하게 설정한다.
		PlayerControl	player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		player.UnPlayable();
		
		// 페이드 컨트롤 추가.
		this.fader = gameObject.AddComponent<FadeControl>();
		//fader	= gameObject.AddComponent();
		this.fader.fade( 1.0f, new Color( 0.0f, 0.0f, 0.0f, 1.0f ), new Color( 0.0f, 0.0f, 0.0f, 0.0f) );
		
		this.next_step = STEP.TITLE;
	}

	void Update ()
	{
		this.step_timer += Time.deltaTime;

		// 다음 상황으로 이동할지 그렇지 않을지를 체크한다.
		switch(this.step) {

			case STEP.TITLE:
			{
				// 마우스를 클릭한다.
				//
				if(Input.GetMouseButtonDown(0)) {

					this.next_step = STEP.WAIT_SE_END;
				}
			}
			break;

			case STEP.WAIT_SE_END:
			{
				// SE 의 재생이 종료되면 후드아웃
			
				bool	to_finish = true;

				do {

					if(!this.GetComponent<AudioSource>().isPlaying) {

						break;
					}

					if(this.GetComponent<AudioSource>().time >= this.GetComponent<AudioSource>().clip.length) {

						break;
					}

					to_finish = false;

				} while(false);

				if(to_finish) {

					this.fader.fade( FADE_TIME, new Color( 0.0f, 0.0f, 0.0f, 0.0f ), new Color( 0.0f, 0.0f, 0.0f, 1.0f) );
				
					this.next_step = STEP.FADE_WAIT;
				}
			}
			break;
			
			case STEP.FADE_WAIT:
			{
				// 페이드가 종료되면 게임씬을 로드하여 종료.
				if(!this.fader.isActive()) 
				{
					Application.LoadLevel("GameScene");
				}
			}
			
			break;
		}

		// 상태가 바뀌는 경우 초기화 처리.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.WAIT_SE_END:
				{
					// 시작 SE를 재생한다.
					this.GetComponent<AudioSource>().Play();
				}
				break;
			}

			this.step = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// 각 상태에서의 실행 처리.

		/*switch(this.step) {

			case STEP.TITLE:
			{
			}
			break;
		}*/

	}

	void OnGUI()
	{	
		GUI.depth = 1;
		
		float	scale	= 1.0f;
		
		if( this.step == STEP.WAIT_SE_END )
		{
			float	rate = this.step_timer / TITLE_ANIME_TIME;
			
			scale = Mathf.Lerp( 2.0f, 1.0f, rate );
		}
		
		TitleSceneControl.drawTexture(this.StartButtonTexture, start_texture_x, start_texture_y, scale, scale, 0.0f, 1.0f);		
		TitleSceneControl.drawTexture(this.TitleTexture, title_texture_x, title_texture_y, 1.0f, 1.0f, 0.0f, 1.0f);		
	}

	public static void drawTexture(Texture texture, float x, float y, float scale_x = 1.0f, float scale_y = 1.0f, float angle = 0.0f, float alpha = 1.0f)
	{
		Vector3		position;
		Vector3		scale;
		Vector3		center;

		position.x =  x + Screen.width/2.0f;
		position.y = -y + Screen.height/2.0f;
		position.z = 0.5f;

		scale.x = scale_x;
		scale.y = scale_y;
		scale.z = 1.0f;

		center.x = texture.width/2.0f;
		center.y = texture.height/2.0f;
		center.z = 0.0f;

		Matrix4x4	matrix = Matrix4x4.identity;

		matrix *= Matrix4x4.TRS(position - center, Quaternion.identity, Vector3.one);

		// 텍스처 중심을 기준으로 회전과 스케일을 곱한다.
		//
		matrix *= Matrix4x4.TRS( center,           Quaternion.identity, Vector3.one);
		matrix *= Matrix4x4.TRS(Vector3.zero,      Quaternion.AngleAxis(angle, Vector3.forward), scale);
		matrix *= Matrix4x4.TRS(-center,           Quaternion.identity, Vector3.one);

		GUI.matrix = matrix;
		GUI.color  = new Color(1.0f, 1.0f, 1.0f, alpha);

		Rect	rect = new Rect(0.0f, 0.0f, texture.width, texture.height);

		GUI.DrawTexture(rect, texture);
	}
}
