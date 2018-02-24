using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneControl : MonoBehaviour {


	public GameObject	StackBlockPrefab = null;


	public PlayerControl	player_control = null;

	public StackBlockControl	stack_control = null;
	public BGControl			bg_control = null;
	public GUIControl			gui_control = null;
	public GoalSceneControl		goal_scene = null;
	public 	VanishEffectControl	vanish_fx_control = null;

	public float	slider_value = 0.5f;

    // 각 색의 Material（Blockl.cs）.
	//
	// ・실체를 하나로 설정하고 싶다.
	// ・Inspector 로 변경할 수 있도록 하고 싶다.
	//
	// 때문에, 인스턴스를 한 개만 만들 수 있는 SceneControl에 저장한다.
	//
	public Material[]	block_materials;


	// ---------------------------------------------------------------- //

	public int		height_level = 0;

	public static int	MAX_HEIGHT_LEVEL = 50;

	public int			player_stock;				// 플레이어의 소지품.

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		PLAY = 0,			// 게임중  
		GOAL_ACT,			// 목표지점 연출
		MISS,				// 실패 연출

		GAMEOVER,			// 게임 오버 

		NUM,
	};

	public STEP			step;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;


	// ---------------------------------------------------------------- //

	public enum SE {

		NONE = -1,

		DROP = 0,			// 블록을 드롭한 경우 
		DROP_CONNECT,		// 블록이 사라지는 경우(같은 색의 블록이 4개가 배열되었을 때).
		LANDING,			// 위에서 내려온 블록이 착지했을 경우          
		SWAP,				// 상하 블록이 회전하여 교체되는 경우
		EATING,				// 케익을 먹는 경우
        JUMP,				// 위에서 내려온 블록이 착지했을 경우 
		COMBO,				// 연쇄동작한 경우

		CLEAR,				// 클리어 
		MISS,				// 실패

		NUM,
	};

	public AudioClip[]	audio_clips;

	public AudioSource[]	audios;

	// ---------------------------------------------------------------- //

	public void	playSe(SE se)
	{
		if(se == SE.SWAP) {

			this.audios[1].clip = this.audio_clips[(int)se];
			this.audios[1].Play();

		} else {

			this.audios[0].PlayOneShot(this.audio_clips[(int)se]);
		}
	}
	void	Start()
	{

		//

		Block.materials = this.block_materials;

		this.stack_control = new StackBlockControl();

		this.stack_control.StackBlockPrefab = this.StackBlockPrefab;
		this.stack_control.scene_control = this;
		this.stack_control.create();

		this.vanish_fx_control = GameObject.FindGameObjectWithTag("VanishEffectControl").GetComponent<VanishEffectControl>();

		//

		this.player_control = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		this.player_control.scene_control = this;

		this.bg_control = GameObject.FindGameObjectWithTag("BG").GetComponent<BGControl>();

		this.gui_control = GameObject.FindGameObjectWithTag("GUIControl").GetComponent<GUIControl>();

		this.goal_scene = new GoalSceneControl();
		this.goal_scene.scene_control = this;
		this.goal_scene.create();

		//

		this.audios = this.GetComponents<AudioSource>();

		//

		this.slider_value = Mathf.InverseLerp(RotateAction.ROTATE_TIME_SWAP_MIN, RotateAction.ROTATE_TIME_SWAP_MAX, RotateAction.rotate_time_swap);

		this.height_level = 0;

		this.bg_control.setHeightRateDirect((float)this.height_level/(float)MAX_HEIGHT_LEVEL);

		this.player_stock = 3;
	}
	
	void	Update()
	{
		this.step_timer += Time.deltaTime;

	#if false
		if(Input.GetKeyDown(KeyCode.G)) {

			this.next_step = STEP.GOAL_ACT;
		}
		if(Input.GetKeyDown(KeyCode.W)) {

			this.height_level = MAX_HEIGHT_LEVEL - 1;
	
			this.bg_control.setHeightRateDirect((float)this.height_level/(float)MAX_HEIGHT_LEVEL);
		}
	#endif

		// -------------------------------------------------------- //
		// 다음 상태로 이동할지 체크한다.

		switch(this.step) {

			case STEP.PLAY:
			{
				do {

					if(this.player_control.isHungry()) {

						this.player_stock--;

						this.player_stock = Mathf.Max(0, this.player_stock);

						this.next_step = STEP.MISS;

						break;
					}
	
					if(this.height_level >= MAX_HEIGHT_LEVEL) {
	
						this.next_step = STEP.GOAL_ACT;
						break;
					}

				} while(false);
			}
			break;

			case STEP.MISS:
			{
				if(this.step_timer > 1.0f) {

					if(	this.player_stock == 0) {

						this.next_step = STEP.GAMEOVER;

					} else {

						this.player_control.revive();
						this.next_step = STEP.PLAY;
					}
				}
			}
			break;

			case STEP.GOAL_ACT:
			case STEP.GAMEOVER:
			{
				// 마우스를 클릭하였다.
				//
				if(Input.GetMouseButtonDown(0)) {
		
					Application.LoadLevel("TitleScene");
				}
			}
			break;
		}

		// -------------------------------------------------------- //
		// 상태가 전환되었을 때의 초기화

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.MISS:
				{
					this.playSe(SE.MISS);
				}
				break;

				case STEP.GAMEOVER:
				{
					this.gui_control.is_disp_gameover = true;
				}
				break;

				case STEP.GOAL_ACT:
				{
					this.goal_scene.start();
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// -------------------------------------------------------- //
		// 각 상태에서 실행처리

		switch(this.step) {

			case STEP.GOAL_ACT:
			{
				this.goal_scene.execute();
			}
			break;
		}

		// ---------------------------------------------------------------- //
				
		this.stack_control.update();

		this.gui_control.stomach_rate = this.player_control.getLifeRate();

	}

	void	OnGUI()
	{
#if false
		GUI.contentColor = Color.black;

		float	x, y;

		x = 10;
		y = 300;

		for(int i = 0;i < Block.NORMAL_COLOR_NUM;i++) {

			this.stack_control.is_color_enable[i] = GUI.Toggle(new Rect(x, y, 100, 16), this.stack_control.is_color_enable[i], ((Block.COLOR_TYPE)i).ToString());

			y += 20.0f;
		}

		y += 20.0f;
		this.slider_value = GUI.HorizontalSlider(new Rect(10, y, 100, 16), this.slider_value, 0.0f, 1.0f);

		y += 20.0f;
		GUI.Label(new Rect(10, y, 100, 20), RotateAction.rotate_time_swap.ToString());
		RotateAction.rotate_time_swap = Mathf.Lerp(RotateAction.ROTATE_TIME_SWAP_MIN, RotateAction.ROTATE_TIME_SWAP_MAX, this.slider_value);


		//

		y = 100;

		GUI.Label(new Rect(10, y, 100, 20), this.stack_control.block_feeder.cake.is_enable.ToString());
		y += 20.0f;

		GUI.Label(new Rect(10, y, 100, 20), this.stack_control.eliminate_to_cake.ToString());
		y += 20.0f;

		GUI.Label(new Rect(10, y, 100, 20), this.stack_control.eliminate_to_fall.ToString());
		y += 20.0f;

		GUI.contentColor = Color.white;
#endif

		//
#if false
		GUI.contentColor = Color.black;
		GUI.Label(new Rect(300, 10, 100, 20), "continuous = " + this.stack_control.score);
		GUI.contentColor = Color.white;
#endif
	}

	public void		heightGain()
	{
		if(this.height_level < MAX_HEIGHT_LEVEL) {

			this.height_level++;
	
			this.bg_control.setHeightRate((float)this.height_level/(float)MAX_HEIGHT_LEVEL);
		}
	}

}
