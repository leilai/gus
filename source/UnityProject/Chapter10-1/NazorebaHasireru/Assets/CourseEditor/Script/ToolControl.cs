using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;

//[ExecuteInEditMode()]
public class ToolControl : MonoBehaviour {

	// ゲームカメラ.
	private	GameObject		main_camera;

	private ToolCameraControl	tool_camera = null;
	private GameCameraControl	game_camera = null;
	private CarCamera			car_camera = null;
	private GameControl			game_control = null;

	public GameObject		TunnelPrefab = null;		// トンネル.
	public GameObject		TreePrefab = null;			// 木.
	public GameObject[]		BuildingPrefabs;			// ビル（いっぱい）.
	public GameObject		CarPrefab = null;			// 車.
	public GameObject		JumpSlopePrefab = null;		// ジャンプ台.
	public GameObject		StartGatePrefab = null;		// スタート地点のゲート.
	public GameObject		GoalGatePrefab = null;		// ゴール地点のゲート.

	public Material			material = null;
	public Material			road_material = null;
	public Material			wall_material = null;
	public PhysicMaterial	physic_material = null;

	public GameObject			LineDrawerPrefab = null;
	public LineDrawerControl	line_drawer;

	public RoadCreator		road_creator;
	public TunnelCreator	tunnel_creator;
	public ForestCreator	forest_creator;
	public BuildingArranger	buil_arranger;
	public JumpSlopeCreator	jump_slope_creator;
	public JunctionFinder	junction_finder;			// 線が交差しているところを探す.
	public ToolGUI			tool_gui;

	public GameObject		car_object = null;

	private GameObject		start_gate = null;			// スタートゲート（インスタンス）.
	private GameObject		goal_gate = null;			// ゴールゲート（インスタンス）.

	private GameObject		waku_object = null;

	// オーディオクリップ.
	public AudioClip		audio_clip_ignition;		// エンジンスタート音.
	public AudioClip		audio_clip_appear;			// コース生成音.

	public Texture			texture_tunnel_icon;
	public Texture			texture_forest_icon;
	public Texture			texture_building_icon;
	public Texture			texture_jump_icon;
	
	public enum STEP {

		NONE = -1,

		EDIT = 0,
		RUN,

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	// ------------------------------------------------------------------------ //

	// Use this for initialization
	void Start ()
	{
		this.tool_gui = this.GetComponent<ToolGUI>();

		// カメラのインスタンスを探しておく.
		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.tool_camera = this.main_camera.GetComponent<ToolCameraControl>();
		this.game_camera = this.main_camera.GetComponent<GameCameraControl>();
		this.car_camera  = this.main_camera.GetComponent<CarCamera>();

		this.tool_camera.enabled = true;
		this.game_camera.enabled = false;
		this.car_camera.enabled  = false;

		this.game_control = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();
		this.game_control.tool_control = this;

		//

		this.line_drawer = (Instantiate(this.LineDrawerPrefab) as GameObject).GetComponent<LineDrawerControl>();
		this.line_drawer.root = this;

		this.waku_object = GameObject.Find("waku");

		//

		this.road_creator   = new RoadCreator();
		this.tunnel_creator = new TunnelCreator();
		this.forest_creator = new ForestCreator();
		this.buil_arranger  = new BuildingArranger();
		this.jump_slope_creator = new JumpSlopeCreator();

		this.junction_finder = new JunctionFinder();

		this.tunnel_creator.TunnelPrefab = this.TunnelPrefab;
		this.tunnel_creator.road_creator = this.road_creator;
		this.tunnel_creator.main_camera  = this.main_camera;
		this.tunnel_creator.texture_tunnel_icon = this.texture_tunnel_icon;
		this.tunnel_creator.create();

		this.forest_creator.TreePrefab   = this.TreePrefab;
		this.forest_creator.road_creator = this.road_creator;
		this.forest_creator.main_camera  = this.main_camera;
		this.forest_creator.texture_forest_icon = this.texture_forest_icon;
		this.forest_creator.create();

		this.buil_arranger.BuildingPrefabs = this.BuildingPrefabs;
		this.buil_arranger.road_creator    = this.road_creator;
		this.buil_arranger.main_camera  = this.main_camera;
		this.buil_arranger.texture_building_icon = this.texture_building_icon;
		this.buil_arranger.create();

		this.jump_slope_creator.texture_jump_icon = this.texture_jump_icon;
		this.jump_slope_creator.road_creator    = this.road_creator;
		this.jump_slope_creator.main_camera  = this.main_camera;
		this.jump_slope_creator.create();
		this.jump_slope_creator.JumpSlopePrefab = this.JumpSlopePrefab;

		this.junction_finder.create();

		//

		this.game_camera.road_creator = this.road_creator;

		this.step = STEP.EDIT;


	}

	void OnGUI()
	{
#if false
		float	x = 10;
		float	y = 100;

		GUI.Label(new Rect(x, y, 100, 100), this.line_drawer.position_num.ToString());
		y += 20;
#endif
		this.tunnel_creator.onGUI();
		this.forest_creator.onGUI();
		this.buil_arranger.onGUI();
		this.jump_slope_creator.onGUI();
	}

	// Update is called once per frame
	void Update ()
	{
		ToolGUI.Button[]	buttons = this.tool_gui.buttons;

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 次の状態に移るかどうかを、チェックする.

		if(this.next_step == STEP.NONE) {

			switch(this.step) {

				case STEP.EDIT:
				{
					// 車で走るボタン.
					if(buttons[(int)ToolGUI.BUTTON.RUN].trigger_on) {
			
						if(this.road_creator.is_created) {

							this.next_step = STEP.RUN;
						}
					}
				}
				break;

				case STEP.RUN:
				{
					if(buttons[(int)ToolGUI.BUTTON.RUN].trigger_on) {

						// テスト走行終了.
	
						this.game_control.stopTestRun();
	
						this.car_object.gameObject.SetActiveRecursively(false);
	
						this.waku_object.active = true;

						this.tool_gui.onStopTestRun();


						this.next_step = STEP.EDIT;
					}
				}
				break;

			} // switch(this.step)
		}

		// ---------------------------------------------------------------- //
		// 状態が遷移したときの初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.EDIT:
				{
					this.tool_camera.setEnable(true);
					this.car_camera.setEnable(false);

	
					this.tunnel_creator.setIsDrawIcon(true);
					this.forest_creator.setIsDrawIcon(true);
					this.buil_arranger.setIsDrawIcon(true);
					this.jump_slope_creator.setIsDrawIcon(true);
				}
				break;

				case STEP.RUN:
				{
					// 車が生成されていなかったら作る.
					if(this.car_object == null) {


						RoadCreator.Section	section = this.road_creator.sections[1];
	
						this.car_object = Instantiate(this.CarPrefab) as GameObject;
		
						this.car_object.transform.position = section.center;
	
						this.car_object.transform.Translate(0.0f, 0.1f, 0.0f);
	
						this.car_object.transform.rotation = Quaternion.FromToRotation(Vector3.forward, section.direction);
	
						// カメラ.
	
						// ターゲット（見るもの）をセット.
						this.car_camera.target = this.car_object.transform;
	
						// 位置、注視点を初期化しておく.
						this.car_camera.reset();
						this.car_camera.calcPosture();
	
						// イグニッション音.
	
						this.audio.PlayOneShot(this.audio_clip_ignition);
					}

					//

					this.tool_camera.setEnable(false);
					this.car_camera.setEnable(true);

					this.tunnel_creator.setIsDrawIcon(false);
					this.forest_creator.setIsDrawIcon(false);
					this.buil_arranger.setIsDrawIcon(false);
					this.jump_slope_creator.setIsDrawIcon(false);
				
					// テスト走行開始.

					this.game_control.startTestRun();

					this.car_object.gameObject.SetActiveRecursively(true);

					this.waku_object.active = false;

					this.tool_gui.onStartTestRun();
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 各状態での実行処理.


		switch(this.step) {

			case STEP.EDIT:
			{
			}
			break;
		}

		// -------------------------------------------------------------------------------------------- //

		if(this.step == STEP.EDIT) {

			//クリアーボタン.
			if(buttons[(int)ToolGUI.BUTTON.NEW].current) {
	
				// 前回済みのものを削除する.
	
				if(this.line_drawer.isLineDrawed()) {
	
					this.line_drawer.clear();
	
					this.line_drawer.setVisible(true);
				}
	
				if(this.road_creator.is_created) {
	
					this.road_creator.clearOutput();		
				}
	
				this.tunnel_creator.clearOutput();
				this.forest_creator.clearOutput();
				this.buil_arranger.clearOutput();
				this.jump_slope_creator.clearOutput();

				this.tunnel_creator.setIsDrawIcon(false);
				this.forest_creator.setIsDrawIcon(false);
				this.buil_arranger.setIsDrawIcon(false);
				this.jump_slope_creator.setIsDrawIcon(false);

				if(this.car_object != null) {
	
					Destroy(this.car_object);
					this.car_object = null;
				}

				if(this.start_gate != null) {

					Destroy(this.start_gate);
					this.start_gate = null;
				}
				if(this.goal_gate != null) {

					Destroy(this.goal_gate);
					this.goal_gate = null;
				}

				//

				this.game_control.onClearOutput();

				//
	
				this.tool_camera.enabled = true;
				this.game_camera.enabled = false;
			}

			// ロードボタン.
			if(buttons[(int)ToolGUI.BUTTON.LOAD].current) {
	
				FileStream    BinaryFile = new FileStream("test-cs.dat", FileMode.Open, FileAccess.Read);
		        BinaryReader  Reader     = new BinaryReader(BinaryFile);
	
				this.line_drawer.loadFromFile(Reader);
	
				Reader.Close();
			}
	
			// セーブボタン.
			if(buttons[(int)ToolGUI.BUTTON.SAVE].current) {
	
				FileStream    BinaryFile = new FileStream("test-cs.dat", FileMode.Create, FileAccess.Write);
		        BinaryWriter  Writer     = new BinaryWriter(BinaryFile);
	
				this.line_drawer.saveToFile(Writer);
	
				Writer.Close();
			}
		}

		// -------------------------------------------------------------------------------------------- //

		// 道を生成するボタン.
		if(buttons[(int)ToolGUI.BUTTON.CREATE_ROAD].current) {

			bool	is_create_road = false;

			do {

				if(!this.line_drawer.isLineDrawed()) {

					break;
				}

				if(this.road_creator.is_created) {

					break;
				}

				is_create_road = true;

			} while(false);

			if(is_create_road) {

				this.create_road();
	
				// トンネル.
				// 形状を作ったときに（TunnelCreator.createTunnel()）再計算する.
				this.tunnel_creator.place_min = 0.0f;
				this.tunnel_creator.place_max = 0.0f;
	
				// 森.
	
				this.forest_creator.place_max = (float)this.road_creator.sections.Length - 1.0f;
				this.forest_creator.start     = this.road_creator.sections.Length*0.25f;
				this.forest_creator.end       = this.road_creator.sections.Length*0.75f;
	
				this.forest_creator.setIsDrawIcon(true);

				// ビル街.
	
				this.buil_arranger.place_max = (float)this.road_creator.sections.Length - 1.0f;
				this.buil_arranger.start     = this.road_creator.sections.Length*0.25f;
				this.buil_arranger.end       = this.road_creator.sections.Length*0.75f;
	
				this.buil_arranger.setIsDrawIcon(true);

				this.jump_slope_creator.place_min = 0.0f;
				this.jump_slope_creator.place_max = (float)this.road_creator.sections.Length - 1.0f;

				if(this.road_creator.sections.Length >= 5) {

					// スタートゲート.
	
					RoadCreator.Section	gate_section;
	
					gate_section = this.road_creator.sections[2];
	
					this.start_gate = Instantiate(this.StartGatePrefab) as GameObject;
	
					this.start_gate.transform.position = gate_section.center;
	
					this.start_gate.transform.rotation = Quaternion.FromToRotation(Vector3.forward, gate_section.direction);
	
					// ゴールゲート.
	
					gate_section = this.road_creator.sections[this.road_creator.sections.Length - 1 - 1];
	
					this.goal_gate = Instantiate(this.GoalGatePrefab) as GameObject;
	
					this.goal_gate.transform.position = gate_section.center;
	
					this.goal_gate.transform.rotation = Quaternion.FromToRotation(Vector3.forward, gate_section.direction);
				}

				// サウンドを鳴らす.

				this.audio.PlayOneShot(this.audio_clip_appear);
			}
		}


		// -------------------------------------------------------------------------------------------- //

		// トンネル作るボタン.
		if(buttons[(int)ToolGUI.BUTTON.TUNNEL_CREATE].current) {

			if(this.road_creator.is_created) {

				this.tunnel_creator.createTunnel();

				this.tunnel_creator.is_draw_icon = true;
			}
		}

		// トンネル移動ボタン.
		if(this.tunnel_creator.is_created) {

			if(buttons[(int)ToolGUI.BUTTON.TUNNEL_BACKWARD].current) {

				float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.TUNNEL_BACKWARD);

				this.tunnel_creator.setPlace(this.tunnel_creator.place + speed);
			}
			if(buttons[(int)ToolGUI.BUTTON.TUNNEL_FORWARD].current) {

				float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.TUNNEL_FORWARD);

				this.tunnel_creator.setPlace(this.tunnel_creator.place - speed);
			}

		}

		// -------------------------------------------------------------------------------------------- //

		// 森つくるボタン.
		if(buttons[(int)ToolGUI.BUTTON.FOREST_CREATE].current) {

			do {

				// 道路が出来てないとだめ.
				if(!this.road_creator.is_created) {

					break;
				}

				// 生成済み.
				if(this.forest_creator.is_created) {

					break;
				}

				//

				this.forest_creator.createForest();

			} while(false);
		}

		// 森in 前ボタン
		if(buttons[(int)ToolGUI.BUTTON.FOREST_START_FORWARD].current) {
	
			if(this.road_creator.is_created) {

				if(!this.forest_creator.is_created) {
	
					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.FOREST_START_FORWARD);

					this.forest_creator.setStart(this.forest_creator.start + speed);
				}
			}
		}
		// 森in 後ろボタン
		if(buttons[(int)ToolGUI.BUTTON.FOREST_START_BACKWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.forest_creator.is_created) {
	
					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.FOREST_START_BACKWARD);

					this.forest_creator.setStart(this.forest_creator.start - speed);
				}
			}
		}
		// 森out 前ボタン
		if(buttons[(int)ToolGUI.BUTTON.FOREST_END_FORWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.forest_creator.is_created) {

					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.FOREST_END_FORWARD);

					this.forest_creator.setEnd(this.forest_creator.end + speed);
				}
			}
		}
		// 森out 後ろボタン
		if(buttons[(int)ToolGUI.BUTTON.FOREST_END_BACKWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.forest_creator.is_created) {

					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.FOREST_END_BACKWARD);

					this.forest_creator.setEnd(this.forest_creator.end - speed);
				}
			}
		}
		
		// -------------------------------------------------------------------------------------------- //

		// ビル並べるボタン.
		if(buttons[(int)ToolGUI.BUTTON.BUIL_CREATE].current) {

			do {

				// 道路が出来てないとだめ.
				if(!this.road_creator.is_created) {

					break;
				}

				// 生成済み.
				if(this.buil_arranger.is_created) {

					break;
				}

				//

				this.buil_arranger.base_offset = 40.0f;

				this.buil_arranger.createBuildings();

			} while(false);
		}
		// ビルin 前ボタン
		if(buttons[(int)ToolGUI.BUTTON.BUIL_START_FORWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.buil_arranger.is_created) {

					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.BUIL_START_FORWARD);

					this.buil_arranger.setStart(this.buil_arranger.start + speed);
				}
			}
		}
		// ビルin 後ろボタン
		if(buttons[(int)ToolGUI.BUTTON.BUIL_START_BACKWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.buil_arranger.is_created) {
	
					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.BUIL_START_BACKWARD);

					this.buil_arranger.setStart(this.buil_arranger.start - speed);
				}
			}
		}
		// ビルout 前ボタン
		if(buttons[(int)ToolGUI.BUTTON.BUIL_END_FORWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.buil_arranger.is_created) {
	
					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.BUIL_END_FORWARD);

					this.buil_arranger.setEnd(this.buil_arranger.end + speed);
				}
			}
		}
		// ビルout 後ろボタン
		if(buttons[(int)ToolGUI.BUTTON.BUIL_END_BACKWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.buil_arranger.is_created) {
	
					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.BUIL_END_BACKWARD);

					this.buil_arranger.setEnd(this.buil_arranger.end - speed);
				}
			}
		}

		// -------------------------------------------------------------------------------------------- //

		// ジャンプ台作るボタン.
		if(buttons[(int)ToolGUI.BUTTON.JUMP_CREATE].current) {

			if(this.road_creator.is_created) {

				this.jump_slope_creator.createJumpSlope();

				this.jump_slope_creator.is_draw_icon = true;
			}
		}
		// ジャンプ台移動ボタン.
		if(this.jump_slope_creator.is_created) {

			if(buttons[(int)ToolGUI.BUTTON.JUMP_FORWARD].current) {

				float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.JUMP_FORWARD);

				this.jump_slope_creator.setPlace(this.jump_slope_creator.place + speed);
			}
			if(buttons[(int)ToolGUI.BUTTON.JUMP_BACKWARD].current) {

				float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.JUMP_BACKWARD);

				this.jump_slope_creator.setPlace(this.jump_slope_creator.place - speed);
			}

		}

		/*if(this.road_creator.is_created) {

			foreach(RoadCreator.Section section in this.road_creator.sections) {

				Debug.DrawLine(section.positions[0], section.positions[1], Color.red, 0.0f, false);
			}
		}*/

		// -------------------------------------------------------------------------------------------- //

		this.forest_creator.execute();
	}

	// "<<" ">>" ボタンを押しっぱにしたときの、アイコンの移動スピード.
	// （だんだん早くなる）.
	private float	calc_icon_move_speed(ToolGUI.BUTTON button)
	{
		ToolGUI.Button[]	buttons = this.tool_gui.buttons;

		float	speed_scale;
		float	speed;

		float	pushed_timer = buttons[(int)button].pushed_timer;

		if(pushed_timer < 0.5f) {

			speed_scale = 1.0f;

		} else if(pushed_timer < 1.0f) {

			speed_scale = Mathf.InverseLerp(0.5f, 1.0f, pushed_timer);
			speed_scale = Mathf.Lerp(1.0f, 4.0f, speed_scale);

		} else {

			speed_scale = 4.0f;
		}

		speed = speed_scale*6.0f*Time.deltaTime;

		return(speed);
	}

	// 実際に道を作る.
	private void	create_road()
	{
		do {

			if(!this.line_drawer.isLineDrawed()) {

				break;
			}
			if(this.road_creator.is_created) {

				break;
			}

			// ------------------------------------------------ //
			// 線が重なっているところを探す.

			this.junction_finder.positions    = this.line_drawer.positions;
			this.junction_finder.position_num = this.line_drawer.position_num;

			this.junction_finder.findJunction();

			// ------------------------------------------------ //
			// 立体交差する個所ごとに、ブロックわけする.
			// 立体交差の上を通過するときに、立体交差の下の壁のコリジョンを無効にするため.

			StaticArray<int>	junction_points = new StaticArray<int>(2 + this.junction_finder.junctions.size()*2);

			// 始点、終点、立体交差する個所をマークする.

			junction_points.push_back(0);

			for(int i = 0;i < this.junction_finder.junctions.size();i++) {

				JunctionFinder.Junction	junction = this.junction_finder.junctions[i];

				junction_points.push_back(junction.i0);
				junction_points.push_back(junction.i1);
			}

			junction_points.push_back(this.line_drawer.position_num - 1);

			// コース上の道のりでソートする

			for(int i = 0;i < junction_points.size();i++) {

				int		current = junction_points[i];
				int		j;

				for(j = i - 1;j >= 0;j--) {

					if(junction_points[j] <= current) {

						break;
					}

					junction_points[j + 1] = junction_points[j];
				}

				junction_points[j + 1] = current;
			}

			// ブロックの分割点を列挙する.
			// 立体交差の個所がブロックの中になるように.
			//（ブロックの分割個所とならないように）.

			StaticArray<int>	split_points = new StaticArray<int>((junction_points.size() - 1)*2 + 2);

			split_points.push_back(junction_points[0]);

			for(int i = 0;i < junction_points.size() - 1;i++) {

				split_points.push_back((int)Mathf.Lerp((float)junction_points[i], (float)junction_points[i + 1], 1.0f/3.0f));
				split_points.push_back((int)Mathf.Lerp((float)junction_points[i], (float)junction_points[i + 1], 2.0f/3.0f));
			}

			split_points.push_back(junction_points[junction_points.size() - 1]);

			//

			this.road_creator.split_points = split_points.entity;

			// ------------------------------------------------ //
			// 立体交差でクロスするところで、高さを指定する.

			StaticArray<RoadCreator.HeightPeg>	pegs = new StaticArray<RoadCreator.HeightPeg>(this.junction_finder.junctions.size()*2 + 2);
			RoadCreator.HeightPeg				peg;

			// 始点
			//
			peg.position = 0;
			peg.height   = 0.0f;

			pegs.push_back(peg);

			for(int i = 0;i < this.junction_finder.junctions.size();i++) {

				JunctionFinder.Junction	junction = this.junction_finder.junctions[i];

				// 交差するところ.

				peg.position = junction.i0;
				peg.height   = 0.0f;

				pegs.push_back(peg);

				peg.position = junction.i1;
				peg.height   = 10.0f;

				pegs.push_back(peg);
			}

			// 終点.
			peg.position = this.line_drawer.position_num - 1;
			peg.height   = 0.0f;

			pegs.push_back(peg);

			// コース上の道のりでソートする.

			for(int i = 0;i < pegs.size();i++) {

				RoadCreator.HeightPeg		current = pegs[i];
				int							j;

				for(j = i - 1;j >= 0;j--) {

					if(pegs[j].position <= current.position) {

						break;
					}

					pegs[j + 1] = pegs[j];
				}

				pegs[j + 1] = current;
			}

			this.road_creator.height_pegs = pegs.entity;

			// ------------------------------------------------ //
			//

			this.road_creator.positions       = this.line_drawer.positions;
			this.road_creator.position_num    = this.line_drawer.position_num;
			this.road_creator.material        = this.material;
			this.road_creator.road_material   = this.road_material;
			this.road_creator.wall_material   = this.wall_material;
			this.road_creator.physic_material = this.physic_material;
			this.road_creator.peak_position   = this.junction_finder.junction.i0;

			this.road_creator.createRoad();

			// マウスでひいた線をみえなくする.
			//
			this.line_drawer.setVisible(false);

		} while(false);
	}

	// マウスの位置を、３D空間のワールド座標に変換する.
	//
	// ・マウスカーソルとカメラの位置を通る直線
	// ・地面の当たり判定となる平面
	//　↑の二つが交わるところを求めます.
	//
	public bool		unproject_mouse_position(out Vector3 world_position, Vector3 mouse_position)
	{
		bool	ret;

		// 地面の当たり判定となる平面.
		Plane	plane = new Plane(Vector3.up, new Vector3(0.0f, 0.0f, 0.0f));

		// カメラ位置とマウスカーソルの位置を通る直線.
		Ray		ray = this.main_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

		// 上の二つが交わるところを求める.

		float	depth;

		if(plane.Raycast(ray, out depth)) {

			world_position = ray.origin + ray.direction*depth;

			ret = true;

		} else {

			world_position = Vector3.zero;

			ret = false;
		}

		return(ret);
	}

}

