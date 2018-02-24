using UnityEngine;
using System.Collections;

public class RoadCreatorTestControl : MonoBehaviour {

	// 게임 카메라
	private	GameObject		game_camera;

	public GameObject		BallPrefab = null;

	public Material			material;
	public PhysicMaterial	physic_material = null;

	private Vector3[]	positions;
	private int			position_num = 0;

	private static int	POSITION_NUM_MAX = 100;

	enum STEP {

		NONE = -1,

		IDLE = 0,		// 대기중
		DRAWING,		// 라인을 그리는 중(드래그 중)
		DRAWED,			// 라인 그리기 종료
		CREATED,		// 도로 모델이 생성됨

		NUM,
	};
	
	private STEP	step      = STEP.NONE;
	private STEP	next_step = STEP.NONE;

	private RoadCreator	road_creator;

	// Use this for initialization
	void Start ()
	{
		// 카메라의 인스턴스를 찾아둔다. 
		this.game_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.GetComponent<LineRenderer>().SetVertexCount(0);

		this.positions = new Vector3[POSITION_NUM_MAX];

		this.road_creator = new RoadCreator();
	}

	void OnGUI()
	{
		float	x = 100;
		float	y = 100;

		GUI.Label(new Rect(x, y, 100, 100), this.position_num.ToString());
		y += 20;

		if(GUI.Button(new Rect(200, 100, 100, 20), "create")) {

			if(this.step == STEP.DRAWED) {

				this.next_step = STEP.CREATED;
			}
		}

		if(GUI.Button(new Rect(310, 100, 100, 20), "clear")) {

			this.next_step = STEP.IDLE;
		}

		if(GUI.Button(new Rect(200, 130, 100, 20), "ball")) {

			if(this.step == STEP.CREATED) {

				GameObject ball = Instantiate(this.BallPrefab) as GameObject;
	
				Vector3	ball_position;
	
				ball_position = (road_creator.sections[0].center + road_creator.sections[1].center)/2.0f + Vector3.up*1.0f;
	 
				ball.transform.position = ball_position;
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{
		// 상태 전환 체크

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
				}
			}
			break;
		}

		// 상태가 전환될 때의 초기화 

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.IDLE:
				{
					// 전회에 작성한 것을 삭제해둔다. 

					this.road_creator.clearOutput();

					this.position_num = 0;

					this.GetComponent<LineRenderer>().SetVertexCount(0);
				}
				break;

				case STEP.CREATED:
				{
					this.road_creator.positions       = this.positions;
					this.road_creator.position_num    = this.position_num;
					this.road_creator.material        = this.material;
					this.road_creator.physic_material = this.physic_material;
		
					this.road_creator.createRoad();
				}
				break;
			}

			this.step = this.next_step;

			this.next_step = STEP.NONE;
		}

		// 각 상태에서의 처리

		switch(this.step) {

			case STEP.DRAWING:
			{
				Vector3	position = this.unproject_mouse_position();

				// 정점을 라인에 추가할지 체크한다. 

				bool	is_append_position = false;

				if(this.position_num == 0) {

					// 최초 한 개는 무조건 추가

					is_append_position = true;

				} else if(this.position_num >= POSITION_NUM_MAX) {

					// 최대 개수를 초과한 경우에는 추가하지 않는다. 

					is_append_position = false;

				} else {

					// 직전에 추가한 정점에서 일정거리 떨어지면 추가

					if(Vector3.Distance(this.positions[this.position_num - 1], position) > 0.5f) {

						is_append_position = true;
					}
				}

				//

				if(is_append_position) {

					if(this.position_num > 0) {

						Vector3	distance = position - this.positions[this.position_num - 1];

						distance *= 0.5f/distance.magnitude;

						position = this.positions[this.position_num - 1] + distance;
					}

					this.positions[this.position_num] = position;

					this.position_num++;

					// LineRender 을 재작성해둔다. 

					this.GetComponent<LineRenderer>().SetVertexCount(this.position_num);

					for(int i = 0;i < this.position_num;i++) {
			
						this.GetComponent<LineRenderer>().SetPosition(i, this.positions[i]);
					}
				}
			}
			break;
		}

		/*if(is_created) {

			foreach(Section section in this.sections) {

				Debug.DrawLine(section.positions[0], section.positions[1], Color.red, 0.0f, false);
			}
		}*/
	}

	// 마우스의 위치를 ３D공간의 월드 좌표로 변환한다. 
	//
	// ・마우스 커서와 카메라의 위치를 통과하는 직선
	// ・조각의 중심을 통과하는 수평한 면
	//　↑이 두 가지 요소가 교차하는 곳을 구한다.
	//
	private Vector3	unproject_mouse_position()
	{
		Vector3	mouse_position = Input.mousePosition;

		// 조각의 중심을 통과하는 수평(법선이 Y축, XZ평면)인 면.
		Plane	plane = new Plane(Vector3.up, new Vector3(0.0f, 0.0f, 0.0f));

		// 카메라 위치와 마우스 커서를 통과하는 직선
		Ray		ray = this.game_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

        // 위의 두 가지 요소가 교차하는 곳을 구한다.

		float	depth;

		plane.Raycast(ray, out depth);

		Vector3	world_position;

		world_position = ray.origin + ray.direction*depth;

		return(world_position);
	}
}
