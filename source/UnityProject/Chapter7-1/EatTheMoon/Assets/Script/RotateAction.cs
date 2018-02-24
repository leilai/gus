using UnityEngine;

// 블록 회전(SWAP, 색 변경).
public struct RotateAction {


	public enum TYPE {

		NONE = -1,

        SWAP_UP = 0,		// SWAP 아래에서 위. 
        SWAP_DOWN,			// SWAP 위에서 아래 
		COLOR_CHANGE,		// 색 변경(중심에서 회전)

		NUM,
	};

	public bool			is_active;		// 실행중?
	public float		timer;			// 경과시간.
	public float		rate;			// 경과시간 비율


	public TYPE			type;

	public Block.COLOR_TYPE	target_color;	// 색 변경의 경우, 변경후의 색

	public static float	rotate_time_swap = 0.25f;

	public static float	ROTATE_TIME_SWAP_MIN = 0.1f;
	public static float	ROTATE_TIME_SWAP_MAX = 1.0f;

	// ---------------------------------------------------------------- //

	// 초기화
	public void init()
	{
		this.is_active = false;
		this.timer     = 0.0f;
		this.rate      = 0.0f;
		this.type      = RotateAction.TYPE.NONE;
	}

	// 회전동작을 시작한다.
	public void start(RotateAction.TYPE type)
	{
		this.is_active = true;
		this.timer     = 0.0f;
		this.rate      = 0.0f;
		this.type      = type;
	}

	// 회전 동작 실행
	public void	execute(StackBlock block)
	{
		float	x_angle = 0.0f;
		float	rotate_time;

		if(this.type == RotateAction.TYPE.COLOR_CHANGE) {

			rotate_time = 0.5f;

		} else {

			rotate_time = RotateAction.rotate_time_swap;
		}

		if(this.is_active) {

			this.timer += Time.deltaTime;

			// 종료 체크

			if(this.timer > rotate_time) {

				this.timer     = rotate_time;
				this.is_active = false;
			}

			// 회전의 중심

			Vector3		rotation_center = Vector3.zero;
			
			if(this.is_active) {

				switch(this.type) {
	
					case RotateAction.TYPE.SWAP_UP:
					{
						rotation_center.y = -Block.SIZE_Y/2.0f;
					}
					break;
	
					case RotateAction.TYPE.SWAP_DOWN:
					{
						rotation_center.y =  Block.SIZE_Y/2.0f;
					}
					break;

					case RotateAction.TYPE.COLOR_CHANGE:
					{
						rotation_center.y =  0.0f;
					}
					break;
				}

				// 각도.

				this.rate = this.timer/rotate_time;

				this.rate = Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, this.rate);
				this.rate = (Mathf.Sin(this.rate) + 1.0f)/2.0f;
				
				x_angle = Mathf.Lerp(-180.0f, 0.0f, this.rate);
			}

			// rotation_center 을 중심으로 상대적으로 회전한다.
			block.transform.Translate(rotation_center);
			block.transform.Rotate(Vector3.right, x_angle);
			block.transform.Translate(-rotation_center);
		}
	}
}
