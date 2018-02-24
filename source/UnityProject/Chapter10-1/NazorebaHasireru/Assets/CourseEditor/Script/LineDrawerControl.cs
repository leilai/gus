using UnityEngine;
using System.Collections;
using System.IO;

public class LineDrawerControl : MonoBehaviour {

	enum STEP {

		NONE = -1,

		IDLE = 0,		// 待機中.
		DRAWING,		// ラインを描いている中（ドラッグ中）.
		DRAWED,			// ラインを描き終わった.

		NUM,
	};
	
	private STEP	step      = STEP.NONE;
	private STEP	next_step = STEP.NONE;

	public Vector3[]	positions;
	public int			position_num = 0;

	private static int	POSITION_NUM_MAX = 1000;

	public ToolControl	root = null;

	private MousePositionSmoother	smoother;

	private Vector3		previous_mouse_position;		// 直前のマウスの位置.
	private bool		is_play_drawing_sound;			// 線を引くときの音を再生中？.
	private float		sound_to_stop_timer = 0.0f;		// 線を引くサウンドを止める判定をするためのタイマー.


	// ------------------------------------------------------------------------ //

	void	Start()	
	{
		this.positions = new Vector3[POSITION_NUM_MAX];

		this.smoother = new MousePositionSmoother();
		this.smoother.create();

		this.previous_mouse_position = Vector3.zero;
		this.is_play_drawing_sound = false;
	}

	void	Update()
	{
		// 状態遷移チェック.
		if(this.next_step == STEP.NONE) {
	
			switch(this.step) {
	
				case STEP.NONE:
				{
					this.next_step = STEP.IDLE;
				}
				break;
	
				case STEP.IDLE:
				{
					if(Input.GetMouseButton(0)) {
	
						this.next_step = STEP.DRAWING;
					}
				}
				break;
	
				case STEP.DRAWING:
				{
					if(!Input.GetMouseButton(0)) {
	
						if(this.position_num >= 2) {
	
							this.next_step = STEP.DRAWED;
	
						} else {
	
							this.next_step = STEP.IDLE;
						}

						this.audio.Stop();
						this.is_play_drawing_sound = false;
					}
				}
				break;
			}
		}

		// 状態が遷移したときの初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.IDLE:
				{
					// 前回作成したものを削除しておく.

					this.position_num = 0;

					this.update_line_renderer();

					this.smoother.reset();
				}
				break;

				case STEP.DRAWING:
				{
					this.smoother.reset();

					this.previous_mouse_position = Input.mousePosition;
					this.is_play_drawing_sound = false;
				}
				break;
			}

			this.step = this.next_step;

			this.next_step = STEP.NONE;
		}

		// 各状態での処理.

		switch(this.step) {

			case STEP.DRAWING:
			{
				this.execute_step_drawing();
			}
			break;

			case STEP.DRAWED:
			{
				for(int i = 0;i < this.position_num - 1;i++) {

					Debug.DrawLine(this.positions[i], this.positions[i + 1], Color.red, 0.0f, false);
				}
			}
			break;
		}
	}
	
	private void	execute_step_drawing()
	{
		Vector3	mouse_position = Input.mousePosition;

		// マウスカーソルの位置をスムージングする.
		mouse_position = this.smoother.append(mouse_position);

		Vector3		position;

		// マウスカーソルの位置を逆透視変換する.
		if(this.root.unproject_mouse_position(out position, mouse_position)) {

			this.execute_step_drawing_sub(mouse_position, position);
		}
	}

	private void	execute_step_drawing_sub(Vector3 mouse_position, Vector3 position_3d)
	{
		// 頂点の間隔（＝道路ポリゴンの縦方向の長さ）.
		float	append_distance = RoadCreator.PolygonSize.z;

		int	append_num = 0;

		while(true) {

			bool	is_append_position;

			// 頂点をラインに追加するか、チェックする.

			is_append_position = false;

			if(this.position_num == 0) {

				// 最初のいっこは無条件に追加.

				is_append_position = true;

			} else if(this.position_num >= POSITION_NUM_MAX) {

				// 最大個数をオーバーした時は追加できない.

				is_append_position = false;

			} else {

				// 直前に追加した頂点から一定距離離れたら追加.

				if(Vector3.Distance(this.positions[this.position_num - 1], position_3d) > append_distance) {

					is_append_position = true;
				}
			}

			if(!is_append_position) {

				break;
			}

			//

			if(this.position_num == 0) {

				this.positions[this.position_num] = position_3d;

			} else {

				// ・「直前に追加した頂点」と「マウスカーソルの位置」を結ぶ直線上
				// ・「直前に追加した頂点」から距離「append_distance」だけ離れている
				//  という頂点を追加する.

				Vector3	distance = position_3d - this.positions[this.position_num - 1];

				distance *= append_distance/distance.magnitude;

				this.positions[this.position_num] = this.positions[this.position_num - 1] + distance;
			}

			this.position_num++;

			//

			append_num++;
		}

		if(append_num > 0) {

			// LineRender を作り直す.
			this.update_line_renderer();
		}

		// 線をひくときの SE の制御.

		this.drawing_sound_control(mouse_position);
	}

	// ラインを削除する.
	public void		clear()
	{
		this.next_step = STEP.IDLE;

		this.Update();
	}

	// ラインを描いた？.
	public bool		isLineDrawed()
	{
		bool	is_drawed = (this.step == STEP.DRAWED);

		return(is_drawed);
	}

	// 表示/非表示する.
	public void		setVisible(bool visible)
	{
		this.set_line_render_visible(visible);
	}

	// ファイルから読む.
	public void		loadFromFile(BinaryReader Reader)
	{
       	this.position_num = Reader.ReadInt32();

		for(int i = 0;i < this.position_num;i++) {

			this.positions[i].x = (float)Reader.ReadDouble();
			this.positions[i].y = (float)Reader.ReadDouble();
			this.positions[i].z = (float)Reader.ReadDouble();
		}

		// LineRender を作り直す.
		this.update_line_renderer();

		this.next_step = STEP.DRAWED;

		this.Update();
	}

	public void		saveToFile(BinaryWriter Writer)
	{
       	Writer.Write((int)this.position_num);

		for(int i = 0;i < this.position_num;i++) {

			Writer.Write((double)this.positions[i].x);
			Writer.Write((double)this.positions[i].y);
			Writer.Write((double)this.positions[i].z);
		}
	}

	// ---------------------------------------------------------------- //

	// ラインを表示/非表示する.
	private void	set_line_render_visible(bool visible)
	{
		this.GetComponent<LineRenderer>().enabled = visible;
	}

	// LineRender を作り直す.
	private void	update_line_renderer()
	{
		this.GetComponent<LineRenderer>().SetVertexCount(this.position_num);

		for(int i = 0;i < this.position_num;i++) {

			this.GetComponent<LineRenderer>().SetPosition(i, this.positions[i]);
		}
	}
	

	private float	DRAW_SE_VOLUME_MIN = 0.3f;
	private float	DRAW_SE_VOLUME_MAX = 1.0f;

	private float	DRAW_SE_PITCH_MIN = 0.75f;
	private float	DRAW_SE_PITCH_MAX = 1.5f;

	// 線をひくときの SE の制御.
	private void	drawing_sound_control(Vector3 mouse_position)
	{
		float	distance = Vector3.Distance(mouse_position, this.previous_mouse_position)/Time.deltaTime;

		// この時間以上マウスの動きが止まったら、線を引くSEを止める.
		// そうしないとサウンドがブツ切れになっちゃうから.
		const float		stop_time = 0.3f;

		if(this.is_play_drawing_sound) {

			if(distance < 60.0f) {

				// マウスの移動量が少なかった.

				this.sound_to_stop_timer += Time.deltaTime;

				if(this.sound_to_stop_timer > stop_time) {

					this.audio.Stop();
					this.is_play_drawing_sound = false;
					this.sound_to_stop_timer = 0.0f;
				}

			} else {

				this.sound_to_stop_timer = 0.0f;

			}

		} else {

			if(distance >= 60.0f) {

				this.audio.Play();
				this.is_play_drawing_sound = true;
				this.sound_to_stop_timer = 0.0f;
			}
		}

		// 線を引くスピードで、ピッチとボリュームを変える.

		if(this.is_play_drawing_sound) {

			float	speed_rate;

			speed_rate = Mathf.InverseLerp(60.0f, 500.0f, distance);

			speed_rate = Mathf.Clamp01(speed_rate);

			speed_rate = Mathf.Round(speed_rate*3.0f)/3.0f;

			// ボリューム.

			float		next_volume = Mathf.Lerp(DRAW_SE_VOLUME_MIN, DRAW_SE_VOLUME_MAX, speed_rate);
			float		current_volume = this.audio.volume;

			float		diff = next_volume - current_volume;

			if(diff > 0.1f) {

				diff = 0.1f;

			} else if(diff < -0.05f) {

				diff = -0.05f;
			}

			next_volume = current_volume + diff;

			this.audio.volume = next_volume;

			// ピッチ.

			float		next_pitch = Mathf.Lerp(DRAW_SE_PITCH_MIN, DRAW_SE_PITCH_MAX, speed_rate);
			float		current_pitch = this.audio.pitch;

			float		pitch_diff = next_pitch - current_pitch;

			if(pitch_diff > 0.1f) {

				pitch_diff = 0.1f;

			} else if(pitch_diff < -0.1f) {

				pitch_diff = -0.1f;
			}

			next_pitch = current_pitch + pitch_diff;

			this.audio.pitch = next_pitch;

		}

		this.previous_mouse_position = mouse_position;
	}

	// ---------------------------------------------------------------- //

}