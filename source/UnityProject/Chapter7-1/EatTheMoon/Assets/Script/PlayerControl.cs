using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public int	lx = 0;

	// 들어올리기 중인 블록의 프리팹
	public 	GameObject	CarryBlockPrefab = null;

	public	GameObject	effect;

	// テクスチャー.
	public Texture[]	textures_normal = null;		// 통상시
	public Texture[]	textures_carry  = null;		// 블록 들어올리기 중
	public Texture[]	textures_eating = null;		// 케익 먹는중
	public Texture		texture_hungry  = null;

	public AudioClip	audio_walk;
	public AudioClip	audio_pick;

	// ---------------------------------------------------------------- //

	// 들어올리기 중의 블록
	public CarryBlock	carry_block = null;

	public SceneControl	scene_control = null;


	// 표시용 간이 스트라이프
	public SimpleSprite	sprite = null;


	// ---------------------------------------------------------------- //
	// 라이프

	public static int	LIFE_MIN = 0;				// 최대값
	public static int	LIFE_MAX = 100;				// 최대값
	public static int	LIFE_ADD_CAKE = LIFE_MAX;	// 케익을 먹을 때에 증가하는 값
	public static int	LIFE_SUB = -2;

	public int	life;								// 현재값

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		NORMAL = 0,			// 통상
		CARRY,				// 블록 들어올리기중
		EATING,				// 먹는 중
		HUNGRY,				// 배고픈 상태(실패)

		GOAL_ACT,			// 목표지점 연출

		NUM,
	};

	public STEP			step;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	public bool			is_controlable = true;

	// ---------------------------------------------------------------- //

	void 	Start()
	{
		this.SetLinedPosition(StackBlockControl.BLOCK_NUM_X/2);

		GameObject game_object = Instantiate(this.CarryBlockPrefab) as GameObject;

		this.carry_block = game_object.GetComponent<CarryBlock>();

		this.carry_block.player             = this;
		this.carry_block.transform.position = this.transform.position + new Vector3(0.0f, 1.0f, 0.0f);
		this.carry_block.renderer.enabled   = false;

		//

		this.sprite = this.gameObject.AddComponent<SimpleSprite>();

		this.sprite.SetTexture(this.textures_normal[0]);

		//

		this.life = LIFE_MAX;

		this.is_controlable = true;
	}

	void Update ()
	{
		StackBlockControl	stack_control = this.scene_control.stack_control;

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
#if false
		// "3" を押すと、エネルギー減少.
		if(Input.GetKey(KeyCode.Keypad3)) {

			this.addLife(-100);
		}
		// "4" を押すと、エネルギー減少.
		if(Input.GetKey(KeyCode.Keypad4)) {

			this.addLife(1);
		}
#endif

		// 배고픈 상태가 되면 게임오버
		if(this.life <= LIFE_MIN) {

			this.next_step = STEP.HUNGRY;
		}

		//
		// 다음 상태로 이동할지 체크한다.
		switch(this.step) {

			case STEP.NORMAL:
			case STEP.EATING:
			{
				// 들어올리기

				if(this.next_step == STEP.NONE) {

					do {
	
						if(!this.is_carry_input()) {
						
							break;
						}
	
						// 발밑의 블록
						StackBlock	ground_block = stack_control.blocks[this.lx, StackBlockControl.GROUND_LINE];
	
						// 회색 블록은 들어올릴 수 없다.
						if(!ground_block.isCarriable()) {
	
							break;
						}

                        // Swap동작 중에는 들어올릴 수 없다.
						if(ground_block.isNowSwapAction()) {
	
							break;
						}
	
						//
	
						// 올길 블록을 발밑의 블록과 같은 색과 같은 색이 되도록 한다.
						this.carry_block.setColorType(ground_block.color_type);
						this.carry_block.startCarry(this.lx);
	
						stack_control.pickBlock(this.lx);
	
						//

						this.audio.PlayOneShot(this.audio_pick);

						this.next_step = STEP.CARRY;
	
					} while(false);
				}

				if(this.next_step == STEP.NONE) {

					if(this.step == STEP.EATING) {

						if(this.step_timer > 3.0f) {
		
							this.next_step = STEP.NORMAL;
						}
					}
				}
			}
			break;

			case STEP.CARRY:
			{
				if(this.is_carry_input()) {

					// 버리기.

					if(this.carry_block.isCakeBlock()) {

						// 들어올리는 것이 케익이라면
						// 먹기& 색 체인지.

						this.carry_block.startHide();

						stack_control.onEatCake();

						this.addLife(LIFE_ADD_CAKE);

						this.audio.PlayOneShot(scene_control.audio_clips[(int)SceneControl.SE.EATING]);

						//

						this.next_step = STEP.EATING;

					} else {

						// 들어올린 것이 보통 블록이라면 버리기.

						this.drop_block();

						this.addLife(LIFE_SUB);

						this.next_step = STEP.NORMAL;
					}
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환될 때 초기화

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.NORMAL:
				{
				}
				break;

				case STEP.HUNGRY:
				{
				}
				break;

				case STEP.GOAL_ACT:
				{
					this.SetHeight(-1);
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행처리

		switch(this.step) {

			case STEP.NORMAL:
			case STEP.CARRY:
			case STEP.EATING:
			{
				int		lx = this.lx;
		
				// 좌우이동
		
				do {

					// 들어올리기, 버리기를 할 때에는 좌우이동할 수 없다.
					//
					// 익숙해지면, 조금이라도 움직일 수 없을 때에는 스트레스를 받을 수 있으므로
					// 봉인
					//
					/*if(this.carry_block.isMoving()) {
		
						break;
					}*/
		
					//

					if(!this.is_controlable) {

						break;
					}
		
					if(Input.GetKeyDown(KeyCode.LeftArrow)) {
			
						lx--;
			
					} else if(Input.GetKeyDown(KeyCode.RightArrow)) {
			
						lx++;

					} else {

						break;
					}
			
					lx = Mathf.Clamp(lx, 0, StackBlockControl.BLOCK_NUM_X - 1);
			
					this.audio.PlayOneShot(this.audio_walk);

					this.SetLinedPosition(lx);
		
				} while(false);
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 텍스처 패턴 컨트롤    

		switch(this.step) {

			default:
			case STEP.NORMAL:
			{
				// 왼쪽→눈을 감는다→오른쪽→눈을 감는다→반복.

				int		texture_index;

				texture_index = (int)(this.step_timer*8.0f);
				texture_index %= 4;

				if(texture_index%2 == 0) {

					// 눈을 감는다.
					texture_index = 0;

				} else {

					// 좌, 우
					texture_index = (texture_index/2)%2 + 1;
				}

				this.sprite.SetTexture(this.textures_normal[texture_index]);

			}
			break;

			case STEP.CARRY:
			{
				int		texture_index;

				texture_index = (int)(this.step_timer*8.0f);
				texture_index %= 4;

				if(texture_index%2 == 0) {

					texture_index = 0;

				} else {

					texture_index = (texture_index/2)%2 + 1;
				}

				this.sprite.SetTexture(this.textures_carry[texture_index]);
			}
			break;

			case STEP.EATING:
			{
				int		texture_index = ((int)(this.step_timer/0.1f))%this.textures_eating.Length;

				this.sprite.SetTexture(this.textures_eating[texture_index]);
			}
			break;

			case STEP.HUNGRY:
			{
				this.sprite.SetTexture(this.texture_hungry);
			}
			break;

			case STEP.GOAL_ACT:
			{
				const float		time0 = 0.5f;
				const float		time1 = 0.5f;

				float	time_all = time0 + time1;

				float	t = Mathf.Repeat(this.step_timer, time_all);

				if(t < time0) {

					this.sprite.SetTexture(this.textures_carry[1]);

				} else {

					t -= time0;

					int		texture_index = ((int)(t/0.1f))%this.textures_eating.Length;

					this.sprite.SetTexture(this.textures_eating[texture_index]);
				}
			}
			break;
		}
	}

	// 블록을 버리고
	public void	dropBlock()
	{
		if(this.step == STEP.CARRY) {

			this.drop_block();

			this.next_step = STEP.NORMAL;
		}
	}
	private void	drop_block()
	{
		this.carry_block.startDrop(this.lx);
	
		this.scene_control.stack_control.dropBlock(this.lx, this.carry_block.color_type, this.carry_block.org_place.x);
	}

	// 위치를 설정한다.
	public void	SetLinedPosition(int lx)
	{
		this.lx = lx;

		Vector3		position = this.transform.position;

		position.x = -(StackBlockControl.BLOCK_NUM_X/2.0f - 0.5f) + this.lx;

		this.transform.position = position;
	}

	// 높이를 설정한다.
	public void	SetHeight(int height)
	{
		StackBlock.PlaceIndex place;

		place.x = this.lx;
		place.y = StackBlockControl.GROUND_LINE - 1 + height;

		this.transform.position = StackBlockControl.calcIndexedPosition(place);
	}

	// 생명을 증감시킨다.
	public void		addLife(int val)
	{
		this.life += val;
	
		this.life = Mathf.Min(Mathf.Max(LIFE_MIN, this.life), LIFE_MAX);
	}

	// 라이프의 현재값(비율)을 설정한다.
	public float	getLifeRate()
	{
		float	rate = Mathf.InverseLerp((float)LIFE_MIN, (float)LIFE_MAX, (float)this.life);

		return(rate);
	}

	// 생명이 0이 되었는가?
	public bool	isHungry()
	{
		bool	ret = (this.life <= LIFE_MIN);

		return(ret);
	}

	// 조잘할 수 없도록 한다.
	public void	setControlable(bool sw)
	{
		this.is_controlable = sw;
	}

	// 목표 지점시의 연출을 시작하여.
	public void	beginGoalAct()
	{
		this.next_step = STEP.GOAL_ACT;
	}

	// 실패 후 부활.
	public void	revive()
	{
		this.life = LIFE_MAX;

		this.next_step = STEP.NORMAL;
	}

	// 들어올리기/ 버리기의 키 입력을 한다?
	private bool	is_carry_input()
	{
		bool	ret;

		if(this.is_controlable) {

			ret = (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow));

		} else {

			ret = false;
		}

		return(ret);
	}

}
