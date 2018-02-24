using UnityEngine;
using System.Collections;

public class RoomControl : MonoBehaviour {

	public GameObject	roomPrefab = null;
	public GameObject	shojiPrefab = null;

	private FloorControl[]	rooms;

	// 카메라
	private GameObject main_camera = null;

	public static float	MODEL_LENGTH   = 15.0f;
	public static float	MODEL_Z_OFFSET = 0.0f;

	public static float	RESTART_Z_OFFSET = 5.0f;		//재시작 위치의 오프셋

	public static int	MODEL_NUM = 3;

	private int		start_model_index = 0;				// 가장 가까이에 있는 모델의 인덱스

	private LevelControl	level_control;
	private	ShojiControl	shoji_control;				// 미닫이문(한 개를 교차로 사용)
	private	SceneControl	scene_control;

	private int		room_count = 0;						// 이동하는 방의 수
	private	bool	is_closed = false;					// 미닫이문이 닫혔는가?(다음 방으로 이동할 때마다 재설정).

	// ---------------------------------------------------------------- //
	
	// Sound
	public AudioClip CLOSE_SOUND = null;
	public AudioClip CLOSE_END_SOUND = null;
	
	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {

		this.rooms = new FloorControl[MODEL_NUM];

		for(int i = 0;i < 3;i++) {
	
			this.rooms[i] = (Instantiate(this.roomPrefab) as GameObject).GetComponent<FloorControl>();

			this.rooms[i].transform.position = new Vector3(0.0f, 0.0f, MODEL_Z_OFFSET + (float)i*MODEL_LENGTH);
		}

		this.start_model_index = 0;

		this.rooms[(this.start_model_index + 0)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 1)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 2)%MODEL_NUM].setClose();

		this.shoji_control = (Instantiate(this.shojiPrefab) as GameObject).GetComponent<ShojiControl>();

		this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);

		//

		// 카메라 인스턴스를 찾는다.
		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.scene_control = this.main_camera.GetComponent<SceneControl>();

		this.level_control = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelControl>();
	}

	// Update is called once per frame
	void 	Update()
	{
		FloorControl	room = this.rooms[this.start_model_index];

		// 가장 뒤의 모델이 카메라의 뒤로 오면, 안쪽으로 이동시킨다.                       
		//
		if(room.transform.position.z + MODEL_LENGTH < this.main_camera.transform.position.z) {

			// 가장 뒤의 모델을 안쪽으로 이동시킨다. 

			Vector3		new_position = room.transform.position;

			new_position.z += MODEL_LENGTH*MODEL_NUM;

			room.transform.position = new_position;

			//

			this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(null);

			// 「가장 가까이에 있는 모델의 인덱스」를 진행한다.
			//
			this.start_model_index = (this.start_model_index + 1)%MODEL_NUM;


            // 가장 가까이에 있는 방　→　미닫이문을 attached하여 열린상태가 되게한다.

			if(this.scene_control.step == SceneControl.STEP.GAME) {

				this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);
				this.rooms[(this.start_model_index + 0)%MODEL_NUM].setOpen();
				
			} else {

				this.shoji_control.gameObject.SetActiveRecursively(false);
			}

			// 두 번째 방　→　장지문을 열기 시작한다.

			this.rooms[(this.start_model_index + 1)%MODEL_NUM].beginOpen();

			// 세 번째 방　→　장지문이 닫힌 상태로 한다.

			this.rooms[(this.start_model_index + 2)%MODEL_NUM].setClose();

			// 몇회 통과한 구멍을 철판으로 설정한다.

			foreach(var paper_control in this.shoji_control.papers) {

				if(this.level_control.getChangeSteelCount() > 0) {

					if(paper_control.through_count >= this.level_control.getChangeSteelCount()) {

						paper_control.beginSteel();
					}
				}
			}

			//

			this.room_count++;
			this.is_closed = false;
		}

		// 카메라가 가까워지면 닫는다.
		//
		// 실제로는 플레이어를 보지만, 방 모델의 반복 처리가 카메라를 
		// 보고 있는 관계상, 여기서도 카메라를 보도록 한다.

		float	close_distance = MODEL_LENGTH - this.level_control.getCloseDistance();

		if(room.transform.position.z + close_distance < this.main_camera.transform.position.z) {

			do {

				if(this.is_closed) {

					break;
				}

				// 시작 직후에는 닫히지 않도록 한다. 
				if(this.room_count < 1) {

					break;
				}

				// 미닫이문을 닫는다.
	
				if(this.scene_control.step == SceneControl.STEP.GAME) {
	
					FloorControl.CLOSING_PATTERN_TYPE	type;
					bool								is_flip;
					FloorControl.ClosingPatternParam	param;

					this.level_control.getClosingPattern(out type, out is_flip, out param);

					this.rooms[(this.start_model_index + 0)%MODEL_NUM].setClosingPatternType(type, is_flip, param);
					this.rooms[(this.start_model_index + 0)%MODEL_NUM].beginCloseShoji();
				}

				this.is_closed = true;

			} while(false);
		}

	#if false
		// デバッグ機能.
		// フルキーの数字キーで、障子の登場パターンを再生する.

		for(int i = (int)KeyCode.Alpha1;i <= (int)KeyCode.Alpha9;i++) {

			if(Input.GetKeyDown((KeyCode)i)) {

				FloorControl.CLOSING_PATTERN_TYPE	type = (FloorControl.CLOSING_PATTERN_TYPE)(i - (int)KeyCode.Alpha1);

				bool	is_flip = Input.GetKey(KeyCode.RightShift);

				this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);
				this.rooms[(this.start_model_index + 0)%MODEL_NUM].setClosingPatternType(type, is_flip);
				this.rooms[(this.start_model_index + 0)%MODEL_NUM].beginCloseShoji();
			}
		}
	#endif
	}

	public void	onRestart()
	{
		this.room_count = 0;
		this.is_closed = false;

		this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);
		this.rooms[(this.start_model_index + 0)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 1)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 2)%MODEL_NUM].setClose();
	}

	// 실패한 후의 재시작 위치를 설정한다.
	public Vector3	getRestartPosition()
	{
		Vector3	position;

		position = this.rooms[this.start_model_index].transform.position;

		position.z += RESTART_Z_OFFSET;

		return(position);
	}

	// 나머지 종이수를 설정한다.
	public int	getPaperNum()
	{
		return(this.shoji_control.getPaperNum());
	}
}
