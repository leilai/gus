using UnityEngine;
using System.Collections;

public class PazzleControl : MonoBehaviour {

	public GameControl		game_control = null;

	private int		piece_num;				// 조각의 수(전부).
	private int		piece_finished_num;		// 완성한 조각의 수.

	enum STEP {

		NONE = -1,

		PLAY = 0,	// 퍼즐 풀기 중.
		CLEAR,		// 클리어 연출중.

		NUM,
	};

	private STEP	step      = STEP.NONE;
	private STEP	next_step = STEP.NONE;

	private float		step_timer = 0.0f;
	private float		step_timer_prev = 0.0f;

	// -------------------------------------------------------- //

	// 모든 조각.
	private PieceControl[]	all_pieces;

	// 고민 중인 조각. 앞에 표시되는 순서로 나열되어 있다.
	private PieceControl[]	active_pieces;

	// 조각을 셔플하여 배치할 장소(범위).
	private Bounds	shuffle_zone;

	// [degree]퍼즐 전체를 회전하는 각도.
	private float	pazzle_rotation = 37.0f;

	// 조각을 셔플하여 배치한다. 그리드의 네모칸 수(1변).
	private int		shuffle_grid_num = 1;

	// 『완성했다!』를 표시한다?.
	private bool	is_disp_cleared = false;

	// -------------------------------------------------------- //

	void	Start()
	{

		this.game_control =  GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();

		// 조각 오브젝트의 수를 센다.

		this.piece_num = 0;

		for(int i = 0;i < this.transform.GetChildCount();i++) {

			GameObject	piece = this.transform.GetChild(i).gameObject;

			if(!this.is_piece_object(piece)) {

				continue;
			}

			this.piece_num++;
		}

		//

		this.all_pieces    = new PieceControl[this.piece_num];
		this.active_pieces = new PieceControl[this.piece_num];

		// 각 조각에 PieceControl 컴포넌트(스크립트 "PieceControl.cs"）를
		// 추가한다.

		for(int i = 0, n = 0;i < this.transform.GetChildCount();i++) {

			GameObject	piece = this.transform.GetChild(i).gameObject;

			if(!this.is_piece_object(piece)) {

				continue;
			}

			piece.AddComponent("PieceControl");

			piece.GetComponent<PieceControl>().pazzle_control = this;

			//

			this.all_pieces[n++] = piece.GetComponent<PieceControl>();
		}

		this.piece_finished_num = 0;

		// 그림을 표시할 조각의 순서를 설정한다.
		//
		this.set_height_offset_to_pieces();

		// 기본 표시 순서를 설정한다.
		//
		for(int i = 0;i < this.transform.GetChildCount();i++) {

			GameObject	game_object = this.transform.GetChild(i).gameObject;

			if(this.is_piece_object(game_object)) {

				continue;
			}

			game_object.renderer.material.renderQueue = this.GetDrawPriorityBase();
		}

		// 조각을 셔플하여 배치할 장소(범위)를 구한다.
		//
		this.calc_shuffle_zone();


		this.is_disp_cleared = false;
	}

	void	Update()
	{
		// ---------------------------------------------------------------- //

		this.step_timer_prev = this.step_timer;

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 상태 변화 점검.

		switch(this.step) {

			case STEP.NONE:
			{
				this.next_step = STEP.PLAY;
			}
			break;

			case STEP.PLAY:
			{
				// 모든 조각이 정답의 위치에 배치되면 클리어.
				if(this.piece_finished_num >= this.piece_num) {
		
					this.next_step = STEP.CLEAR;
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 변화시 초기화.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.PLAY:
				{
					// this.active_pieces = this.all_pieces 가 되면 배열을 참조하여 복사 
					// 하게 되므로 내용물을 하나씩 복사한다.
					for(int i = 0;i < this.all_pieces.Length;i++) {

						this.active_pieces[i] = this.all_pieces[i];
					}

					this.piece_finished_num = 0;

					this.shuffle_pieces();

					foreach(PieceControl piece in this.active_pieces) {

						piece.Restart();
					}

					// 조각의 표시 순서를 설정한다.
					//
					this.set_height_offset_to_pieces();
				}
				break;

				case STEP.CLEAR:
				{
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

			case STEP.CLEAR:
			{
				// 완성 시의 음악.

				const float	play_se_delay = 0.40f;

				if(this.step_timer_prev < play_se_delay && play_se_delay <= this.step_timer) {

					this.game_control.playSe(GameControl.SE.COMPLETE);
					this.is_disp_cleared = true;
				}
			}
			break;
		}

		PazzleControl.DrawBounds(this.shuffle_zone);
	}

	// 『다시하기』버튼을 누른 경우.
	public void	beginRetryAction()
	{
		this.next_step = STEP.PLAY;
	}

	// 조각의 드래그를 시작할 때의 처리.
	public void		PickPiece(PieceControl piece)
	{
		int	i, j;

		// 클릭한 조각을 배열의 선두에 이동시킨다.
		//
		// this.pieces[] 는 표시된 순서로 나열되므로 선두에 두게 되면
		// 가장 먼저 표시되게 된다.

		for(i = 0;i < this.active_pieces.Length;i++) {

			if(this.active_pieces[i] == null) {

				continue;
			}

			if(this.active_pieces[i].name == piece.name) {

				// 『클릭한 조각』보다 앞에 있는 조각은 하나씩 뒤로 밀린다.
				//
				for(j = i;j > 0;j--) {

					this.active_pieces[j] = this.active_pieces[j - 1];
				}

				// 클릭한 조각을 선두로 가져온다.
				this.active_pieces[0] = piece;

				break;
			}
		}

		this.set_height_offset_to_pieces();
	}

	// 정답의 위치에 조각이 놓여졌을 때의 처리.
	public void		FinishPiece(PieceControl piece)
	{
		int	i, j;

		piece.renderer.material.renderQueue = this.GetDrawPriorityFinishedPiece();

		// 클릭한 조각을 배열에서 지운다.

		for(i = 0;i < this.active_pieces.Length;i++) {

			if(this.active_pieces[i] == null) {

				continue;
			}

			if(this.active_pieces[i].name == piece.name) {

				// 『클릭한 조각』보다 위에 있는 조각을 하나씩 앞으로 당긴다.
				//
				for(j = i;j < this.active_pieces.Length - 1;j++) {

					this.active_pieces[j] = this.active_pieces[j + 1];
				}

				// 배열의 맨끝을 빈 공간으로 한다.
				this.active_pieces[this.active_pieces.Length - 1] = null;

				// 『정답 완료된 조각』의 수를＋１한다.
				this.piece_finished_num++;

				break;
			}
		}

		this.set_height_offset_to_pieces();
	}

	// ---------------------------------------------------------------------------------------- //

	private static float	SHUFFLE_ZONE_OFFSET_X = -5.0f;
	private static float	SHUFFLE_ZONE_OFFSET_Y =  1.0f;
	private static float	SHUFFLE_ZONE_SCALE =  1.1f;

	// 조각을 셔플하여 배치할 장소(범위)를 구한다.
	private void	calc_shuffle_zone()
	{
		Vector3		center;

		// 조각을 섞는 범위의 중심.

		center = Vector3.zero;

		foreach(PieceControl piece in this.all_pieces) {

			center += piece.finished_position;
		}
		center /= (float)this.all_pieces.Length;

		center.x += SHUFFLE_ZONE_OFFSET_X;
		center.z += SHUFFLE_ZONE_OFFSET_Y;

		// 조각을 배치할 그리드의 네모칸의 수.

		this.shuffle_grid_num = Mathf.CeilToInt(Mathf.Sqrt((float)this.all_pieces.Length));

		// 조각의 바운딩박스 안에서의 최대수 ＝１네모칸의 크기.

		Bounds	piece_bounds_max = new Bounds(Vector3.zero, Vector3.zero);

		foreach(PieceControl piece in this.all_pieces) {

			Bounds bounds = piece.GetBounds(Vector3.zero);

			piece_bounds_max.Encapsulate(bounds);
		}

		piece_bounds_max.size *= SHUFFLE_ZONE_SCALE;

		this.shuffle_zone = new Bounds(center, piece_bounds_max.size*this.shuffle_grid_num);
	}

	// 조각의 위치를 셔플한다.
	private void	shuffle_pieces()
	{
	#if true
		// 조각을 그리디에 순서대로 나열한다.

		int[]		piece_index = new int[this.shuffle_grid_num*this.shuffle_grid_num];

		for(int i = 0;i < piece_index.Length;i++) {

			if(i < this.all_pieces.Length) {

				piece_index[i] = i;

			} else {

				piece_index[i] = -1;
			}
		}

		// 조각 두 개를 랜덤하게 선택하여 위치를 교환한다.

		for(int i = 0;i < piece_index.Length - 1;i++) {

			int	j = Random.Range(i + 1, piece_index.Length);

			int	temp = piece_index[j];

			piece_index[j] = piece_index[i];

			piece_index[i] = temp;
		}
	
		// 장소 인덱스에서 실제 좌표로 교환하여 배치한다.

		Vector3	pitch;

		pitch = this.shuffle_zone.size/(float)this.shuffle_grid_num;

		for(int i = 0;i < piece_index.Length;i++) {

			if(piece_index[i] < 0) {

				continue;
			}

			PieceControl	piece = this.all_pieces[piece_index[i]];

			Vector3	position = piece.finished_position;

			int		ix = i%this.shuffle_grid_num;
			int		iz = i/this.shuffle_grid_num;

			position.x = ix*pitch.x;
			position.z = iz*pitch.z;

			position.x += this.shuffle_zone.center.x - pitch.x*(this.shuffle_grid_num/2.0f - 0.5f);
			position.z += this.shuffle_zone.center.z - pitch.z*(this.shuffle_grid_num/2.0f - 0.5f);

			position.y = piece.finished_position.y;

			piece.start_position = position;
		}

		// 조금씩(그리드의 네모칸 내에서) 랜덤하게 위치를 옮긴다.

		Vector3		offset_cycle = pitch/2.0f;
		Vector3		offset_add   = pitch/5.0f;

		Vector3		offset = Vector3.zero;

		for(int i = 0;i < piece_index.Length;i++) {

			if(piece_index[i] < 0) {

				continue;
			}

			PieceControl	piece = this.all_pieces[piece_index[i]];

			Vector3	position = piece.start_position;

			position.x += offset.x;
			position.z += offset.z;

			piece.start_position = position;

			//


			offset.x += offset_add.x;

			if(offset.x > offset_cycle.x/2.0f) {

				offset.x -= offset_cycle.x;
			}

			offset.z += offset_add.z;

			if(offset.z > offset_cycle.z/2.0f) {

				offset.z -= offset_cycle.z;
			}
		}

		// 전체를 회전시킨다.

		foreach(PieceControl piece in this.all_pieces) {

			Vector3		position = piece.start_position;

			position -= this.shuffle_zone.center;

			position = Quaternion.AngleAxis(this.pazzle_rotation, Vector3.up)*position;

			position += this.shuffle_zone.center;

			piece.start_position = position;
		}

		this.pazzle_rotation += 90;

	#else
		// 単純に乱数で座標を決める場合.
		foreach(PieceControl piece in this.all_pieces) {

			Vector3	position;

			Bounds	piece_bounds = piece.GetBounds(Vector3.zero);

			position.x = Random.Range(this.shuffle_zone.min.x - piece_bounds.min.x, this.shuffle_zone.max.x - piece_bounds.max.x);
			position.z = Random.Range(this.shuffle_zone.min.z - piece_bounds.min.z, this.shuffle_zone.max.z - piece_bounds.max.z);

			position.y = piece.finished_position.y;

			piece.start_position = position;
		}
	#endif
	}

	// 조각의 GameObject？.
	private bool is_piece_object(GameObject game_object)
	{
		bool	is_piece = false;

		do {

			// 이름에 "base" 가 들어있는 것은 기본 오브젝트.
			if(game_object.name.Contains("base")) {

				continue;
			}

			//

			is_piece = true;

		} while(false);

		return(is_piece);
	}


	// ---------------------------------------------------------------------------------------- //

	// 모든 조각에 높이 오브젝트를 설정한다.
	private void	set_height_offset_to_pieces()
	{
		float	offset = 0.01f;

		int		n = 0;

		foreach(PieceControl piece in this.active_pieces) {

			if(piece == null) {

				continue;
			}

			// 그림 표시 순서를 지정한다.
			// pieces 의 앞에 있는 조각＝위에 있는 조각만큼 그림 표시 순서가 뒤(보이는)에 오도록.
			//
			piece.renderer.material.renderQueue = this.GetDrawPriorityPiece(n);

			// 클릭했을 때에 가장 위에 있는 조각의 OnMouseDown() 를 불러올 수 있도록 하기 위해
			// Y높이 오브젝트도 설정해 둔다.
			// （그림 표시에 대한 우선권만 설정해 두면 아래에 있는 조각이 클릭되는 경우도 존재한다.)

			offset -= 0.01f/this.piece_num;

			piece.SetHeightOffset(offset);

			//

			n++;
		}
	}

	// 그림 표시 우선권을 설정한다.(기본).
	private int	GetDrawPriorityBase()
	{
		return(0);
	}

	// 그림 표시 우선권을 설정한다 (정답의 위치에 놓인 조각).
	private int	GetDrawPriorityFinishedPiece()
	{
		int	priority;

		priority = this.GetDrawPriorityBase() + 1;

		return(priority);
	}

	// 그림 표시 우선권을 설정한다.（『다시하기』버튼）.
	public int	GetDrawPriorityRetryButton()
	{
		int	priority;

		priority = this.GetDrawPriorityFinishedPiece() + 1;

		return(priority);
	}

    // 그림 표시 우선권을 설정한다.（정답 위치에 놓인 조각).
	private int	GetDrawPriorityPiece(int priority_in_pieces)
	{
		int	priority;

		// 『다시하기』버튼이 존재하므로 하나 더 여유분으로 더한다.
		priority = this.GetDrawPriorityRetryButton() + 1;

		// priority_in_pieces 는０번이 가장 첫 번째이고, renderQueue는 값이 작은 것이 먼저 표시된다.
		priority += this.piece_num - 1 - priority_in_pieces;

		return(priority);
	}

	// ---------------------------------------------------------------------------------------- //

	// 퍼즐을 완성했나？.
	public bool	isCleared()
	{
		return(this.step == STEP.CLEAR);
	}

	// 『완성했다！』를 표시한다？.
	public bool	isDispCleard()
	{
		return(this.is_disp_cleared);
	}

	// ---------------------------------------------------------------------------------------- //

	public static void	DrawBounds(Bounds bounds)
	{
		Vector3[]	square = new Vector3[4];

		square[0] = new Vector3(bounds.min.x, 0.0f, bounds.min.z);
		square[1] = new Vector3(bounds.max.x, 0.0f, bounds.min.z);
		square[2] = new Vector3(bounds.max.x, 0.0f, bounds.max.z);
		square[3] = new Vector3(bounds.min.x, 0.0f, bounds.max.z);

		for(int i = 0;i < 4;i++) {

			Debug.DrawLine(square[i], square[(i + 1)%4], Color.white, 0.0f, false);
		}

	}
}
