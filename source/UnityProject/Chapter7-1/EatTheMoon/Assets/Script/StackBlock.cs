using UnityEngine;
using System.Collections;

// 아래에 쌓여있는 블록
public class StackBlock : Block {

	public StackBlockControl	stack_control = null;

	// 상태
	public enum STEP {
		
		NONE = -1,
	
		IDLE = 0,		// 통상
		VANISHING,		// 사라지는 애미메이션 중(색 변화).
		VACANT,			// 텅 빔(연쇄로 지워져, 회색이 된다)
        FALL,			// 낙하중

		NUM,
	};
	
	public STEP		step;
	public STEP		next_step = STEP.NONE;
	public float	step_timer;

	// 그리드상의 장소
	public struct PlaceIndex {

		public int		x;
		public int		y;
	};

	public PlaceIndex	place;
	public Vector3		position_offset;
	public Vector3		velocity;

	public RotateAction		swap_action;							// 상하 교체시의 움직임 제어
	public RotateAction		color_change_action;					// 색 변경시의 움직임 제어

	public static float		FALL_START_HEIGHT = 6.5f;

	public static float		OFFSET_REVERT_SPEED = 0.1f*60.0f;		// 오프셋이 0을 향하는 스피드

	public bool		shake_is_active;
	public float	shake_timer;
	public Vector3	shake_offset;

	// ---------------------------------------------------------------- //


	void 	Start()
	{
		this.setColorType(this.color_type);

		this.position_offset = Vector3.zero;

		// 회전동작 초기화

		this.swap_action.init();
		this.color_change_action.init();

		this.shake_is_active = false;
	}

	// from_block 에서 색과 위치 등을 복사한다.
	public void	relayFrom(StackBlock from_block)
	{
		this.setColorType(from_block.color_type);

		this.step        = from_block.step;
		this.next_step   = from_block.next_step;
		this.step_timer  = from_block.step_timer;
		this.swap_action = from_block.swap_action;
		this.color_change_action = from_block.color_change_action;

		this.velocity = from_block.velocity;

		// 글로벌 위치가 바뀌지 않도록 오프셋을 계산한다.
		this.position_offset = StackBlockControl.calcIndexedPosition(from_block.place) + from_block.position_offset - StackBlockControl.calcIndexedPosition(this.place);

		//this.position_offset = from_block.transform.position - StackBlockControl.calcIndexedPosition(this.place);
		// 위를 향하면, 회전의 중심을 벗어난 것의 영향을 받게 되므로
	}

	// 블록의 상하교환 액션을 시작한다.
	static public void		beginSwapAction(StackBlock upper_block, StackBlock under_block)
	{
		Block.COLOR_TYPE	upper_color;
		StackBlock.STEP		upper_step;
		RotateAction		upper_color_change;

		upper_color        = upper_block.color_type;
		upper_step         = upper_block.step;
		upper_color_change = upper_block.color_change_action;

		upper_block.setColorType(under_block.color_type);
		upper_block.step                = under_block.step;
		upper_block.color_change_action = under_block.color_change_action;

		under_block.setColorType(upper_color);
		under_block.step                = upper_step;
		under_block.color_change_action = upper_color_change;

		// 회전 동작을 시작한다.   
		upper_block.swap_action.start(RotateAction.TYPE.SWAP_UP);
		under_block.swap_action.start(RotateAction.TYPE.SWAP_DOWN);
	}

	void 	Update()
	{
		this.step_timer += Time.deltaTime;

		const float	vanish_time = 1.0f;

		// -------------------------------------------- //
		// 다음 상태로 이동할지 체크한다.

		switch(this.step) {

			case STEP.VANISHING:
			{
				if(this.step_timer > vanish_time) {

					this.next_step = STEP.VACANT;
				}
			}
			break;

			case STEP.FALL:
			{
				// 착지하면 종료 
				if(this.position_offset.y == 0.0f) {

					this.next_step = STEP.IDLE;
				}
			}
			break;
		}

		// -------------------------------------------- //
		// 상태가 전환될 때 초기화 

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.VACANT:
				{
					this.setColorType(COLOR_TYPE.GRAY);
				}
				break;

				case STEP.FALL:
				{
					this.velocity = Vector3.zero;
				}
				break;

				case STEP.VANISHING:
				{
					this.shake_start();

					this.stack_control.scene_control.vanish_fx_control.createEffect(this);
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// -------------------------------------------- //
		// 각 상태에서의 실행처리

		switch(this.step) {

			case STEP.VANISHING:
			{
				// 블록의 색을
				//
				// 원래 색→적색→회색
				//
				// 으로 바꾼다.

				float	rate;

				if(this.step_timer < vanish_time*0.1f) {

					rate = this.step_timer/(vanish_time*0.1f);

				} else if(this.step_timer < vanish_time*0.3f) {

					rate = 1.0f;

				} else if(this.step_timer < vanish_time*0.6f) {

					this.setColorType(COLOR_TYPE.RED);

					rate = (this.step_timer - vanish_time*0.3f)/(vanish_time*0.3f);

				} else {

					rate = 1.0f;
				}

				this.renderer.material.SetFloat("_BlendRate", rate);
			}
			break;

		}

		// -------------------------------------------------------------------------------- //
		// 네모칸 위의 위치(통상 고정), 회전은 0으로 초기화 

		this.transform.position = StackBlockControl.calcIndexedPosition(this.place);
		this.transform.rotation = Quaternion.identity;

		// -------------------------------------------- //
		// 슬라이드(오프셋 수정)

		if(this.step == STEP.FALL) {

			this.velocity.y += -9.8f*Time.deltaTime;

			this.position_offset.y += this.velocity.y*Time.deltaTime;

			if(this.position_offset.y < 0.0f) {

				this.position_offset.y = 0.0f;
			}

			// 아래에 있는 블록을 추월하지 않도록
			// （처리 순서가 아래→위로 한정하지 않으므로 엄격하지는 않다.)
			//
			if(this.place.y < StackBlockControl.BLOCK_NUM_Y - 1) {

				StackBlock	under = this.stack_control.blocks[this.place.x, this.place.y + 1];

				if(this.position_offset.y < under.position_offset.y) {

					this.position_offset.y = under.position_offset.y;
					this.velocity.y        = under.velocity.y;
				}
			}

		} else {

			float	position_offset_prev = this.position_offset.y;

			if(Mathf.Abs(this.position_offset.y) < 0.1f) {

				// 오프셋이 충분히 작아지면 종료 
	
				this.position_offset.y = 0.0f;
	
			} else {

				if(this.position_offset.y > 0.0f) {
	
					this.position_offset.y -=  OFFSET_REVERT_SPEED*Time.deltaTime;
					this.position_offset.y = Mathf.Max(0.0f, this.position_offset.y);
	
				} else {
	
					this.position_offset.y -= -OFFSET_REVERT_SPEED*Time.deltaTime;
					this.position_offset.y = Mathf.Min(0.0f, this.position_offset.y);
				}
			}

			// 위에서 떨어지는 블록이 충돌하는 경우를 피하기 위해 속도를 계산해 둔다.    
			this.velocity.y = (this.position_offset.y - position_offset_prev)/Time.deltaTime;
		}

		this.transform.Translate(this.position_offset);

		// -------------------------------------------- //
        // swap 동작

		this.swap_action.execute(this);

		// 케익은 회전하지 않는다.
		if(this.isCakeBlock()) {

			this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0.0f);

			this.transform.rotation = Quaternion.identity;
		}

		// -------------------------------------------- //
		// 색 변경   

		this.color_change_action.execute(this);

		if(this.color_change_action.is_active) {

			// 반정도 회전하여 색이 바뀐다.

			if(this.color_change_action.rate > 0.5f) {

				this.setColorType(this.color_change_action.target_color);
			}
		}

		// -------------------------------------------- //
		// 블록이 사라질 때의 진동.

		this.shake_execute();
	}
#if false
	// マウスボタンが押されたとき.
	// （使用するときは、StackBlockPrefab. の BoxCollider を有効にしてください）.
	void 	OnMouseDown()
	{
		// クリックされたら色が変わる（デバッグ用）.

		if(this.step == STEP.IDLE) {

			/*COLOR_TYPE	color = this.color_type;

			color = (COLOR_TYPE)(((int)color + 1)%Block.NORMAL_COLOR_NUM);

			this.setColorType(color);*/
			/*if(this.color_type == Block.COLOR_TYPE.PINK) {

				this.setColorType(Block.COLOR_TYPE.CYAN);

			} else {

				this.setColorType(Block.COLOR_TYPE.PINK);
			}*/
			this.stack_control.block_feeder.cake.x = this.place.x;
			this.stack_control.block_feeder.cake.is_enable = true;
			this.setColorType(Block.COLOR_TYPE.CAKE0);
		}
	}
#endif
	// 일반 동작을 시작한다.
	public void beginIdle(Block.COLOR_TYPE color_type)
	{
		this.setColorType(color_type);
		this.next_step = STEP.IDLE;
	}

	// 사라지는 동작을 시작한다.
	public void	beginVanishAction()
	{
		this.next_step = STEP.VANISHING;
	}

	// 낙하 동작을 시작한다.
	public void	beginFallAction(Block.COLOR_TYPE color_type)
	{
		this.setColorType(color_type);
		this.setVisible(true);

		this.position_offset.y = FALL_START_HEIGHT;
		this.velocity = Vector3.zero;

		this.next_step = STEP.FALL;
	}

	// 색이 바뀌는 동작을 시작한다.
	public void	beginColorChangeAction(Block.COLOR_TYPE	color_type)
	{
		this.color_change_action.target_color = color_type;
		this.color_change_action.start(RotateAction.TYPE.COLOR_CHANGE);
	}

	// 연쇄 체크의 대상이 되는가?
	public bool isConnectable()
	{
		bool	ret;

		ret = false;

		if(this.step == STEP.IDLE || this.next_step == STEP.IDLE) {

			ret = true;
		}

		if(this.color_type < Block.NORMAL_COLOR_FIRST || Block.NORMAL_COLOR_LAST < this.color_type) {

			ret = false;
		}

		// 최하단은 연쇄체크의 대상에서 제외(화면 밖으로 나오기 때문에)
		if(this.place.y >= StackBlockControl.BLOCK_NUM_Y - 1) {

			ret = false;
		}

		return(ret);
	}

	// 텅 빈 블록?(연쇄로 지워진 후)
	public bool isVacant()
	{
		bool	ret;

		do {

			ret = true;

			//

			if((this.step == STEP.VACANT || this.next_step == STEP.VACANT)) {

				break;
			}

			//

			ret = false;

		} while(false);

		return(ret);
	}

    // 텅 빈 블록?(연쇄로 지워진 후)
	public bool isCarriable()
	{
		bool	ret;

		do {

			ret = false;

			//

			if((this.step == STEP.VANISHING || this.next_step == STEP.VANISHING)) {

				break;
			}
			if((this.step == STEP.VACANT || this.next_step == STEP.VACANT)) {

				break;
			}

			//

			ret = true;

		} while(false);

		return(ret);
	}

	// 교체 동작중?
	public bool isNowSwapAction()
	{
		bool	ret = false;

		ret = this.swap_action.is_active;

		return(ret);
	}

	// 낙하중?
	public bool isNowFallAction()
	{
		bool	ret = (this.step == STEP.FALL || this.next_step == STEP.FALL);

		return(ret);
	}

	// 사라진 후?
	public bool	isVanishAfter()
	{
		bool	ret;

		do {

			ret = true;

			//

			if((this.step == STEP.VANISHING || this.next_step == STEP.VANISHING)) {

				break;
			}
			if((this.step == STEP.VACANT || this.next_step == STEP.VACANT)) {

				break;
			}

			//

			ret = false;

		} while(false);


		return(ret);
	}
	// 미사용중이 된다.
	public void	setUnused()
	{
		this.setColorType(Block.COLOR_TYPE.NONE);
		this.setVisible(false);
	}

	// ---------------------------------------------------------------- //

	// 진동시작   
	private void	shake_start()
	{
		this.shake_is_active = true;
		this.shake_timer = 0.0f;
	}

	// 진동 컨트롤  
	private void	shake_execute()
	{
		if(this.shake_is_active) {

			float	shake_time = 0.5f;

			float	t = this.shake_timer/shake_time;


			//

			float	amplitude = 0.05f*Mathf.Lerp(1.0f, 0.0f, t);

			// 옆에 있는 블록이 같은 동작을 하지 않도록
			// 약간 주기를 오프셋한다.

			float	t_offset = (float)((this.place.y*StackBlockControl.BLOCK_NUM_X + this.place.x)%(StackBlockControl.BLOCK_NUM_X - 1));

			t_offset /= (float)(StackBlockControl.BLOCK_NUM_X - 2);

			this.shake_offset.x = amplitude*Mathf.Cos(10.0f*(t + t_offset)*2.0f*Mathf.PI);

			//

			Vector3	p = this.transform.position;

			p.x += this.shake_offset.x;

			this.transform.position = p;

			//

			this.shake_timer += Time.deltaTime;

			if(this.shake_timer >= shake_time) {

				this.shake_is_active = false;
			}
		}
	}

}
