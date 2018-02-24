using UnityEngine;
using System.Collections;
using System.Linq;

public class ToolGUI : MonoBehaviour {

	public enum BUTTON {

		NONE = -1,

		NEW = 0,					// クリアー.
		LOAD,						// ロード.
		SAVE,						// セーブ.

		CREATE_ROAD,				// 道路を生成する.
		RUN,						// 車で走る.

		TUNNEL_CREATE,				// トンネルを生成する.
		TUNNEL_FORWARD,				// トンネルを前に移動する.
		TUNNEL_BACKWARD,			// トンネルを後ろに移動する.

		FOREST_CREATE,				// 森を作る.
		FOREST_START_FORWARD,		// 森の開始地点を前に移動する.
		FOREST_START_BACKWARD,		// 森の開始地点を後ろに移動する.
		FOREST_END_FORWARD,			// 森の終了地点を前に移動する.
		FOREST_END_BACKWARD,		// 森の終了地点を後ろに移動する.

		BUIL_CREATE,				// ビル街を作る.
		BUIL_START_FORWARD,			// ビル街の開始地点を前に移動する.
		BUIL_START_BACKWARD,		// ビル街の開始地点を後ろに移動する.
		BUIL_END_FORWARD,			// ビル街の終了地点を前に移動する.
		BUIL_END_BACKWARD,			// ビル街の終了地点を後ろに移動する.

		JUMP_CREATE,				// ジャンプ台を作る.
		JUMP_FORWARD,				// ジャンプ台を前に移動する.
		JUMP_BACKWARD,				// ジャンプ台を後ろに移動する.

		NUM,
	};

	public struct Button {

		public bool	current;
		public bool	previous;

		public bool	trigger_on;
		public bool	trigger_off;

		public bool	is_repeat_button;

		public float	pushed_timer;		// 押しっぱの時間.
	};

	public Button[] buttons;

	public GUISkin	gui_skin;

	public AudioClip		audio_clip_click;		// クリック音.

	public bool				is_edit_mode = true;	// エディットモード？.

	// ------------------------------------------------------------------------ //

	// Use this for initialization
	void Start () {

		this.buttons = new Button[(int)BUTTON.NUM];

		foreach(var i in this.buttons.Select((v, i) => i)) {

			this.buttons[i].previous = false;
			this.buttons[i].current  = false;

			this.buttons[i].pushed_timer = 0.0f;

			//

			this.buttons[i].is_repeat_button = false;

			switch((BUTTON)i) {

				case BUTTON.TUNNEL_FORWARD:
				case BUTTON.TUNNEL_BACKWARD:
				case BUTTON.FOREST_START_FORWARD:
				case BUTTON.FOREST_START_BACKWARD:
				case BUTTON.FOREST_END_FORWARD:
				case BUTTON.FOREST_END_BACKWARD:
				case BUTTON.BUIL_START_FORWARD:
				case BUTTON.BUIL_START_BACKWARD:
				case BUTTON.BUIL_END_FORWARD:
				case BUTTON.BUIL_END_BACKWARD:
				case BUTTON.JUMP_FORWARD:
				case BUTTON.JUMP_BACKWARD:
				{
					this.buttons[i].is_repeat_button = true;
				}
				break;
			}
		}

		this.is_edit_mode = true;
	}
	
	private bool	is_mouse_button_current = false;
	private bool	is_mouse_button_down    = false;

	void Update ()
	{

		this.is_mouse_button_current = Input.GetMouseButton(0);
		this.is_mouse_button_down    = Input.GetMouseButtonDown(0);
	}


	public void	onStartTestRun()
	{
		this.is_edit_mode = false;
	}
	public void	onStopTestRun()
	{
		this.is_edit_mode = true;
	}

	void OnGUI()
	{
		GUI.skin = this.gui_skin;

		//

		if(Event.current.type == EventType.Layout) {

			foreach(var i in this.buttons.Select((v, i) => i)) {

				this.buttons[i].previous = this.buttons[i].current;
				this.buttons[i].current  = false;
			}
		}

		//

		int		x, y;

		y = 20;
		x = 10;

		if(this.is_edit_mode) {

			this.on_gui_file(x, y);
		}

		x += 110;

		this.on_gui_road(x, y);
		x += 110;

		this.on_gui_tunnel(x, y);
		x += 110;

		this.on_gui_forest(x, y);
		x += 100;

		this.on_gui_buil(x, y);
		x += 100;

		this.on_gui_jump(x, y);
		x += 100;

		// RepeatButton はボタンを押しっぱにしままカーソルを動かすと、一瞬
		// ボタンを離したことになってしまうので.
		// current が false でも、「直前まで押されていた」かつ「マウスボタンが押されている」
		// なら、ボタンは押しっぱとみなす.
		if(Event.current.type == EventType.Layout) {

			foreach(var i in this.buttons.Select((v, i) => i)) {

				if(this.buttons[i].is_repeat_button) {

					if(this.buttons[i].previous && this.is_mouse_button_current) {

						this.buttons[i].current = true;
					}
				}
			}
		}

		//

		foreach(var i in this.buttons.Select((v, i) => i)) {

			this.buttons[i].trigger_on  = !this.buttons[i].previous &&  this.buttons[i].current;
			this.buttons[i].trigger_off =  this.buttons[i].previous && !this.buttons[i].current;

			if(Event.current.type == EventType.Repaint) {

				if(this.buttons[i].current) {
	
					this.buttons[i].pushed_timer += Time.deltaTime;
	
				} else {
	
					this.buttons[i].pushed_timer = 0.0f;
				}
			}

			// 押した瞬間の SE.
			//

			if(this.buttons[i].trigger_on) {

				if(i == (int)BUTTON.CREATE_ROAD) {

				} else {

					this.audio.PlayOneShot(this.audio_clip_click);
				}
			}
		}
	}

	// ファイル関連.
	private void	on_gui_file(int x, int y)
	{
		if(GUI.Button(new Rect(x, y, 100, 20), "無くな～る")) {

			this.buttons[(int)BUTTON.NEW].current = true;

		}
		y += 30;
#if UNITY_EDITOR
		if(GUI.Button(new Rect(x, y, 100, 20), "読んでく～る")) {

			this.buttons[(int)BUTTON.LOAD].current = true;
		}
		y += 30;

		if(GUI.Button(new Rect(x, y, 100, 20), "書いてく～る")) {

			this.buttons[(int)BUTTON.SAVE].current = true;
		}
		y += 30;
#endif
	}

	// 道路生成とか.
	private void	on_gui_road(int x, int y)
	{
		if(GUI.Button(new Rect(x, y, 100, 20), "道にな～る")) {

			this.buttons[(int)BUTTON.CREATE_ROAD].current = true;
		}
		y += 30;

		string	text;

		if(this.is_edit_mode) {

			text = "車で走～る";

		} else {

			text = "戻～る";
		}

		if(GUI.Button(new Rect(x, y, 100, 20), text)) {

			this.buttons[(int)BUTTON.RUN].current = true;
		}
		y += 30;
	}

	// トンネル関連.
	private void	on_gui_tunnel(int x, int y)
	{
		if(GUI.Button(new Rect(x, y, 100, 20), "長いトンネ～ル")) {

			this.buttons[(int)BUTTON.TUNNEL_CREATE].current = true;
		}
		y += 30;

		if(GUI.RepeatButton(new Rect(x, y, 40, 20), "<<")) {

			this.buttons[(int)BUTTON.TUNNEL_FORWARD].current = true;
		}
		if(GUI.RepeatButton(new Rect(x + 50, y, 40, 20), ">>")) {

			this.buttons[(int)BUTTON.TUNNEL_BACKWARD].current = true;
		}
		y += 30;
	}

	// 森関連.
	private void	on_gui_forest(int x, int y)
	{
		if(GUI.Button(new Rect(x, y, 90, 20), "森があ～る")) {

			this.buttons[(int)BUTTON.FOREST_CREATE].current = true;
		}
		y += 30;

		if(GUI.RepeatButton(new Rect(x, y, 40, 20), "<<")) {

			this.buttons[(int)BUTTON.FOREST_START_BACKWARD].current = true;
		}
		if(GUI.RepeatButton(new Rect(x + 50, y, 40, 20), ">>")) {

			this.buttons[(int)BUTTON.FOREST_START_FORWARD].current = true;
		}
		y += 30;

		if(GUI.RepeatButton(new Rect(x, y, 40, 20), "<<")) {

			this.buttons[(int)BUTTON.FOREST_END_BACKWARD].current = true;
		}
		if(GUI.RepeatButton(new Rect(x + 50, y, 40, 20), ">>")) {

			this.buttons[(int)BUTTON.FOREST_END_FORWARD].current = true;
		}
		y += 30;
	}

	// ビル関連.
	private void	on_gui_buil(int x, int y)
	{
		if(GUI.Button(new Rect(x, y, 90, 20), "高いビ～ル")) {

			this.buttons[(int)BUTTON.BUIL_CREATE].current = true;
		}
		y += 30;

		if(GUI.RepeatButton(new Rect(x, y, 40, 20), "<<")) {

			this.buttons[(int)BUTTON.BUIL_START_BACKWARD].current = true;
		}
		if(GUI.RepeatButton(new Rect(x + 50, y, 40, 20), ">>")) {

			this.buttons[(int)BUTTON.BUIL_START_FORWARD].current = true;
		}
		y += 30;

		if(GUI.RepeatButton(new Rect(x, y, 40, 20), "<<")) {

			this.buttons[(int)BUTTON.BUIL_END_BACKWARD].current = true;
		}
		if(GUI.RepeatButton(new Rect(x + 50, y, 40, 20), ">>")) {

			this.buttons[(int)BUTTON.BUIL_END_FORWARD].current = true;
		}
		y += 30;
	}

	// ジャンプ台関連.
	private void	on_gui_jump(int x, int y)
	{
		if(GUI.Button(new Rect(x, y, 90, 20), "空も飛べ～る")) {

			this.buttons[(int)BUTTON.JUMP_CREATE].current = true;
		}
		y += 30;

		if(GUI.RepeatButton(new Rect(x, y, 40, 20), "<<")) {

			this.buttons[(int)BUTTON.JUMP_BACKWARD].current = true;
		}
		if(GUI.RepeatButton(new Rect(x + 50, y, 40, 20), ">>")) {

			this.buttons[(int)BUTTON.JUMP_FORWARD].current = true;
		}
		y += 30;
	}
}
