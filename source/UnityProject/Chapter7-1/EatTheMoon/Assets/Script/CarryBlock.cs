using UnityEngine;
using System.Collections;

public class CarryBlock : Block {

	public Vector3		position_offset;

	public PlayerControl	player = null;

	// 버릴 때의 위치
	public StackBlock.PlaceIndex	place;

	public StackBlock.PlaceIndex	org_place;

	public enum STEP {

		NONE = -1,

		HIDE = 0,				// 비표시
		CARRY_UP,				// 들어올리는 중(이동중).
		CARRY,					// 들고 있는 중(이동 종료)           
		DROP_DOWN,				// 버리는 중

		NUM,
	};

	public STEP		step       = STEP.NONE;
	public STEP		next_step  = STEP.NONE;

	public float	step_timer = 0.0f;

	// ---------------------------------------------------------------- //

	public bool	isMoving()
	{
		bool	ret = false;

		switch(this.step) {

			case STEP.CARRY_UP:
			case STEP.DROP_DOWN:
			{
				ret = true;
			}
			break;
		}

		return(ret);
	}

	void 	Start()
	{
		this.position_offset = Vector3.zero;

		this.next_step = STEP.HIDE;
	}
	
	void 	Update()
	{
		this.step_timer += Time.deltaTime;

		// 상태 전환 체크

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.CARRY_UP:
				{
					if(this.position_offset.y == 0.0f) {
	
						this.next_step = STEP.CARRY;
					}
				}
				break;
	
				case STEP.DROP_DOWN:
				{
					if(this.position_offset.y == 0.0f) {
	
						this.player.scene_control.stack_control.endDropBlockAction(this.place.x);
	
						this.next_step = STEP.HIDE;
					}
				}
				break;
			}
		}

		// 상태 전환 초기화

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.HIDE:
				{
					this.renderer.enabled = false;
				}
				break;

				case STEP.CARRY_UP:
				{
					// 비표시 상태에서 시작된 경우에는 현재위치를 구해둔다.
					if(this.step == STEP.HIDE) {

						this.transform.position = StackBlockControl.calcIndexedPosition(this.place);
					}

					Vector3	base_position = this.player.transform.position;

					base_position.y += Block.SIZE_Y;

					this.position_offset = this.transform.position - base_position;
			
					this.setVisible(true);
				}
				break;

				case STEP.DROP_DOWN:
				{
					Vector3	base_position = StackBlockControl.calcIndexedPosition(this.place);

					this.position_offset = this.transform.position - base_position;
				}
				break;
			}

			this.step = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// 각 상태 실행

		Vector3		position = this.transform.position;

		switch(this.step) {

			case STEP.CARRY:
			case STEP.CARRY_UP:
			{
				position.x = this.player.transform.position.x;
				position.y = this.player.transform.position.y + Block.SIZE_Y;
				position.z = 0.0f;
			}
			break;

			case STEP.DROP_DOWN:
			{
				position = StackBlockControl.calcIndexedPosition(this.place);
			}
			break;
		}

		// 오프셋을 설정한다.

		if(Mathf.Abs(this.position_offset.y) < 0.1f) {

			this.position_offset.y = 0.0f;

		} else {

			const float		speed = 0.2f;

			if(this.position_offset.y > 0.0f) {

				this.position_offset.y -=  speed*(60.0f*Time.deltaTime);

				this.position_offset.y = Mathf.Max(this.position_offset.y, 0.0f);

			} else {

				this.position_offset.y -= -speed*(60.0f*Time.deltaTime);

				this.position_offset.y = Mathf.Min(this.position_offset.y, 0.0f);
			}
		}

		position.y += this.position_offset.y;

		this.transform.position = position;
	}

	// 들어올리기 동작을 시작한다.
	public void		startCarry(int place_index_x)
	{
		// 드롭 중에 블록을 들어올린 경우에는 일단 착지한 경우에
		// 처리를 실행한다.
		// 그렇지 않으면 최상단의 블록이 비표시되게 된다.
		// （드롭중에는 옮기는 블록이 착지할 때까지 최상단의 블록은
		// 　비표시 상태로 되어 있으므로）.
		if(this.step == STEP.DROP_DOWN) {

			this.player.scene_control.stack_control.endDropBlockAction(this.place.x);
		}

		this.place.x = place_index_x;
		this.place.y = StackBlockControl.GROUND_LINE;

		this.org_place = this.place;

		this.next_step = STEP.CARRY_UP;
	}

	// 버리기 동작을 시작한다.
	public void		startDrop(int place_index_x)
	{
		this.place.x = place_index_x;
		this.place.y = StackBlockControl.GROUND_LINE;

		this.next_step = STEP.DROP_DOWN;
	}

	// 비표시한다.
	// （케익은 먹은 후)
	public void		startHide()
	{
		this.next_step = STEP.HIDE;
	}
}
