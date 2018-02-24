using UnityEngine;
using System.Collections;

public class ShojiControl : MonoBehaviour {

	public struct HoleIndex {

		public int	x;
		public int	y;

	};

	// 초기 위히X,Y.
	public float init_x = 0.0f;
	public float init_y = 2.0f;
	public float init_z = 5.0f;
	
	// 바닥의 폭（Z방향）.
	public static float WIDTH = 15.0f;
	
	// 플레이어와의 거리 (DIST < WIDTH).
	public float DIST = 10.0f;
	
	// 오브젝트의 이동 거리.
	public static float MOVE = 0.1f;

    //  GameObject neutral_position
	public Vector3 neutral_position;

	public GameObject paperPrefab;

	public SyoujiPaperControl[,] papers;

	public static float	TILE_SIZE_X = 0.85f;
	public static float	TILE_SIZE_Y = 0.94f;
	public static float	TILE_ORIGIN_X = -0.85f;
	public static float	TILE_ORIGIN_Y =  1.92f;

	public static int	TILE_NUM_X = 3;
	public static int	TILE_NUM_Y = 3;

	public int	paper_num = TILE_NUM_X*TILE_NUM_Y;		// 나머지 종이의 수

	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {

        //  GameObject neutral_position
		this.neutral_position = this.transform.position;

		this.papers = new SyoujiPaperControl[TILE_NUM_X, TILE_NUM_Y];

		for(int x = 0;x < TILE_NUM_X;x++) {

			for(int y = 0;y < TILE_NUM_Y;y++) {
				
				GameObject	go = Instantiate(this.paperPrefab) as GameObject;

				go.transform.parent = this.transform;

				//

				Vector3	position = go.transform.localPosition;

				position.x = TILE_ORIGIN_X + x*TILE_SIZE_X;
				position.y = TILE_ORIGIN_Y + y*TILE_SIZE_Y;
				position.z = 0.0f;

				go.transform.localPosition = position;

				//

				SyoujiPaperControl	paper_control = go.GetComponent<SyoujiPaperControl>();

				paper_control.shoji_control = this;
				paper_control.hole_index.x = x;
				paper_control.hole_index.y = y;

				this.papers[x, y] = paper_control;
			}
		}

		//

		this.paper_num = this.papers.Length;
	}
	
	// Update is called once per frame
	void Update () {

	}

	// 종이가 찢어졌을 때의 처리
	public void	onPaperBreak()
	{
		this.paper_num--;

		this.paper_num = Mathf.Max(0, this.paper_num);

		// 공중 점프 카운터（찢어진 후에 통과한 횟수)를 재설정한다.
		//
		// 1.종이 A를 찢는다.
		// 2.종이 A를 통과(종이A의 공중 점프 카운터가 증가)
		// 3.종이 B를 찢는다.
		// 4.종이B를 통과　←이 시점에 종이 A를 철판화하는 경우가 있으므로

		for(int x = 0;x < TILE_NUM_X;x++) {

			for(int y = 0;y < TILE_NUM_Y;y++) {

				this.papers[x, y].resetThroughCount();
			}
		}
	}

	// 나머지 종이의 수를 점검한다.
	public int		getPaperNum()
	{
		return(this.paper_num);
	}

	// 『격자 구멍』인덱스가 유효한가?.
	public bool	isValidHoleIndex(HoleIndex hole_index)
	{
		bool	ret = false;

		do {

			ret = false;

			if(hole_index.x < 0 || TILE_NUM_X <= hole_index.x) {

				break;
			}
			if(hole_index.y < 0 || TILE_NUM_Y <= hole_index.y) {

				break;
			}
			
			ret = true;

		} while(false);

		return(ret);
	}

	// 가장 가까운『격자 구멍』을 취득한다.
	public HoleIndex	getClosetHole(Vector3 position)
	{
		HoleIndex hole_index;

		position = this.transform.InverseTransformPoint(position);

		hole_index.x = Mathf.RoundToInt((position.x - TILE_ORIGIN_X)/TILE_SIZE_X);
		hole_index.y = Mathf.RoundToInt((position.y - TILE_ORIGIN_Y)/TILE_SIZE_Y);

		return(hole_index);
	}

	// 『격자 구멍』의 위치좌표를 취득한다.
	public Vector3	getHoleWorldPosition(int hole_pos_x, int hole_pos_y)
	{
		Vector3	position;

		position.x = (float)hole_pos_x*TILE_SIZE_X + TILE_ORIGIN_X;
		position.y = (float)hole_pos_y*TILE_SIZE_Y + TILE_ORIGIN_Y;
		position.z = 0.0f;

		position = this.transform.TransformPoint(position);

		return(position);
	}

}
