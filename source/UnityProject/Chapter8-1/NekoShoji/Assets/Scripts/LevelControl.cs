using UnityEngine;
using System.Collections;

public class LevelControl : MonoBehaviour {

	public enum LEVEL {

		NONE = -1,

		EASY = 0,
		NORMAL = 1,
		HARD = 2,

		NUM,
	};

	public LEVEL	level = LEVEL.EASY;

	public SceneControl	scene_control = null;

	private bool	random_bool_prev;

	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {

		switch(GlobalParam.GetInstance().difficulty()) {

			case 0:
			{	
				this.level = LEVEL.EASY;
			}
			break;
		
			case 1:
			{	
				this.level = LEVEL.NORMAL;
			}
			break;
			
			case 2:
			{	
				this.level = LEVEL.HARD;
			}
			break;

			default:
			{	
				this.level = LEVEL.EASY;
			}
			break;
		}

		this.scene_control = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<SceneControl>();

		this.random_bool_prev = Random.Range(0, 2) == 0 ? true : false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// 미닫이문이 닫히기 시작하는 거리.
	public float getCloseDistance()
	{
		// 작은 값의 경우, 미닫이문이 닫히기 시작하는 타이밍이 느려지게 되어 어려워진다.
		
		float	close_distance = 14.0f;
		
		int		paper_num = this.scene_control.getPaperNum();

		switch(this.level) {
		
			case LEVEL.EASY:
			{
				close_distance = 14.0f;
			}
			break;
			
			case LEVEL.NORMAL:
			{
				close_distance = 14.0f;
			}
			break;
			
			case LEVEL.HARD:
			{
				if(paper_num >= 8) {

					close_distance = 12.0f;

				} else if(paper_num >= 5) {

					close_distance = 12.0f;

				} else {

					close_distance = FloorControl.SHUTTER_POSITION_Z;
				}
			}
			break;

			default:
			{
				close_distance = 14.0f;
			}
			break;
		}


		return(close_distance);
	}
	
	// 닫히는 미닫이문의 패턴을 설정한다.
	public void	getClosingPattern(out FloorControl.CLOSING_PATTERN_TYPE out_type, out bool out_is_flip, out FloorControl.ClosingPatternParam out_param)
	{
		int		paper_num   = this.scene_control.getPaperNum();
		bool	random_bool = Random.Range(0, 2) == 0 ? true : false;

		out_param.as_float = 0.0f;
		out_param.as_bool  = false;

		switch(this.level) {

			case LEVEL.EASY:
			{
				// easy 는 normal 만

				out_type = FloorControl.CLOSING_PATTERN_TYPE.NORMAL;

				out_is_flip = false;
			}
			break;

			case LEVEL.NORMAL:
			{
				StaticArray<FloorControl.CLOSING_PATTERN_TYPE>	out_type_cadidates = new StaticArray<FloorControl.CLOSING_PATTERN_TYPE>((int)FloorControl.CLOSING_PATTERN_TYPE.NUM);

				if(9 >= paper_num && paper_num >= 8) {

					// １、２번째는 NORMAL.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.NORMAL);

					out_is_flip = false;

				} else if(paper_num == 7) {

					// 나머지７개는  OVERSHOOT.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.OVERSHOOT);

					out_is_flip = false;

				} else if(paper_num == 6) {

					// 나머지６개는 SECONDTIME.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.SECONDTIME);

					out_is_flip = false;

				} else if(paper_num == 5) {

					// 나머지５개는 ARCODION.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.ARCODION);

					out_is_flip = false;

				} else if(4 >= paper_num && paper_num >= 3) {

					// 나머지４～３개는 DELAY（is_flip 은 랜덤）.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.DELAY);

					out_is_flip = random_bool;

					if(paper_num == 4) {

						// 나머지４장은 오른쪽에서

						out_param.as_bool = false;

					} else {

						// 나머지３장은 오른쪽에서（장지문의 뒤에서）.
						out_param.as_bool = true;
					}

				} else if(paper_num == 2) {

					// 나머지２개는 반드시 FALLDOWN.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.FALLDOWN);

					out_is_flip = false;

				} else {

					// 나머지 １개는 반드시 FLIP（is_flip은 랜덤）.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.FLIP);

					out_is_flip = random_bool;
				}

				out_type = out_type_cadidates[Random.Range(0, out_type_cadidates.size())];
			}
			break;

			case LEVEL.HARD:
			{
				if(paper_num >= 8) {

					// 나머지９～８개는 NORMAL.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.NORMAL;

					if(paper_num == 9) {

						out_is_flip = random_bool;

					} else {

						out_is_flip = !this.random_bool_prev;
					}

				} else if(paper_num >= 5) {

					// 나머지７～５개는 SLOW.
					// 닫히는 동작이 점점 느려진다.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.SLOW;

					if(paper_num == 7) {

						out_is_flip        = random_bool;
						out_param.as_float = 1.5f;

					} else if(paper_num == 6) {

						out_is_flip        = !this.random_bool_prev;
						out_param.as_float = 1.7f;

						// 다음 회차를 위해 기록해둔다.
						// （7, 6, 5 에서 반드시 상호동작하도록）.
						random_bool = !this.random_bool_prev;

					} else {

						out_is_flip        = !this.random_bool_prev;
						out_param.as_float = 2.0f;
					}

				} else {

					// 나머지４～１개는 SUPER_DELAY.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.SUPER_DELAY;

					out_is_flip = random_bool;

					if(paper_num >= 4) {

						out_param.as_float = 0.6f;

					} else if(paper_num >= 3) {

						out_param.as_float = 0.7f;

					} else {

						out_param.as_float = 0.9f;
					}
				}
			}
			break;

			default:
			{
				out_type = FloorControl.CLOSING_PATTERN_TYPE.NORMAL;

				out_is_flip = false;
			}
			break;
		}

		this.random_bool_prev = random_bool;
	}

	// 『몇 차례 통과（찢고 구멍을 통과)하면 철판이 되는지』를 설정한다.
	public int	getChangeSteelCount()
	{
		// -1 의 경우에는 철판이 아님
		int	count = -1;

		int	paper_num = this.scene_control.getPaperNum();

		switch(this.level) {
		
			case LEVEL.EASY:
			{
				// easy 는 철판 없음
				count = -1;
			}
			break;

			case LEVEL.NORMAL:
			{
				// hard는 철판 없음
				count = -1;
			}
			break;

			case LEVEL.HARD:
			{
				// （가정）
				// 나머지 개수가 적어질수록 철판화를 설정하도록 한다.

				if(paper_num >= 8) {
				
					count = -1;
				
				} else if(paper_num >= 6) {

					count = 5;

				} else if(paper_num >= 3) {

					count = 2;

				} else { 

					count = 1;
				}

			}
			break;

			default:
			{
				count = -1;
			}
			break;
		}

		return(count);
	}
}
