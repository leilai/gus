using UnityEngine;
using System.Collections;

// 쌓인 블록 전체 제어
public class StackBlockControl {

	public GameObject	StackBlockPrefab = null;

	//낙하하는 블록 배열의 수
	public static int	FALL_LINE_NUM = 3;
	public static int	BLOCK_NUM_X = 9;
	public static int	BLOCK_NUM_Y = 7 + FALL_LINE_NUM;

	// 지면의 블록의 배열이 몇행째?
	//
	// ０～GROUND_LINE + 1 행까지는 비어있는가
	// GROUND_LINE ～ BLOCK_NUM_Y - 1 행은 지면 아래
	public static int	GROUND_LINE = FALL_LINE_NUM;

	public StackBlock[,]	blocks;

	public	ConnectChecker	connect_checker = null;		// 연쇄 체크  
	public	BlockFeeder		block_feeder = null;		// 다음에 등장하는 블록의 색을 정한다.
	public 	SceneControl	scene_control = null;

	private bool	is_has_swap_block = false;			// swap 동작중인 블록이 존재하는가?
    private bool is_swap_end_frame = false;		     	// swap 동작이 끝난 순간에만 true.

	public int		fall_request = 0;					// 낙하를 원하는 라인수(리퀘스트가 축적한 수)
	private int		fall_count = 0;						// 낙하중인 배열의 수

	public bool[]	is_color_enable = null;				// 각 색의 블록이 출현하는가?

	public bool		is_scroll_enable = true;
	public bool		is_connect_check_enable = true;

	// ---------------------------------------------------------------- //

	public struct Combo {

		public bool	is_now_combo;

		// 연쇄 횟수
		public int	combo_count_last;		// 직전
		public int	combo_count_current;	// 현재（연쇄중인 경우

	};

	public Combo combo;

	public int		eliminate_count;		// 지워진 블록의 수
	public int		eliminate_to_fall;
	public int		eliminate_to_cake;		// 케익이 나올 때까지 지우는 블록 수의 나머지

	public static int	ELIMINATE_TO_FALL_INIT = 5;
	public static int	ELIMINATE_TO_CAKE_INIT = 5;
	public static int	ELIMINATE_TO_CAKE_INIT_2ND = 25;
	
	// ---------------------------------------------------------------- //

	public int		score = 0;
	public int		continuous_connect_num = 0;

	// 색 변경의 경우 변화후의 색
	private int	change_color_index0 = -1;
	private int	change_color_index1 = -1;

	// ---------------------------------------------------------------- //

	public void	create()
	{
		//

		this.blocks = new StackBlock[BLOCK_NUM_X, BLOCK_NUM_Y];

		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				GameObject	game_object = GameObject.Instantiate(this.StackBlockPrefab) as GameObject;

				StackBlock	block = game_object.GetComponent<StackBlock>();

				block.place.x = x;
				block.place.y = y;

				this.blocks[x, y] = block;

				block.setUnused();

				block.stack_control = this;
			}
		}

		//

		this.is_color_enable = new bool[Block.NORMAL_COLOR_NUM];

		for(int i = 0;i < this.is_color_enable.Length;i++) {

			this.is_color_enable[i] = true;
		}

		// 핑크는 봉인
		this.is_color_enable[(int)Block.COLOR_TYPE.PINK] = false;

		//

		this.connect_checker = new ConnectChecker();

		this.connect_checker.stack_control = this;
		this.connect_checker.blocks = this.blocks;
		this.connect_checker.create();

		this.block_feeder = new BlockFeeder();
		this.block_feeder.control = this;
		this.block_feeder.create();

		//

		this.setColorToAllBlock();

		//

		this.combo.is_now_combo        = false;
		this.combo.combo_count_last    = 0;
		this.combo.combo_count_current = 0;

		this.eliminate_count = 0;
		this.eliminate_to_fall = ELIMINATE_TO_FALL_INIT;
		this.eliminate_to_cake = ELIMINATE_TO_CAKE_INIT;


		this.is_scroll_enable = true;
		this.is_connect_check_enable = true;
	}

	// 모든 블록의 색을 선택한다.
	public void		setColorToAllBlock()
	{
		// places ... 색을 선택하는 블록의 순서를 저장하기 위한 배열.
		//
		// place[0] 처음에 색을 선택하는 블록(의 장소).
		// place[1] ２번째로 색을 선택하는 블록.
        // place[2] ３번째로 색을 선택하는 블록.
		//            :
		//

		var	places = new StaticArray<StackBlock.PlaceIndex>(BLOCK_NUM_X*(BLOCK_NUM_Y - GROUND_LINE));

		// 먼저 왼쪽 위부터 순서대로 나열한다.    

		for(int y = GROUND_LINE;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				StackBlock.PlaceIndex	place;

				place.x = x;
				place.y = y;

				places.push_back(place);
			}
		}
	#if true
		// 순서를 셔플한다. 
		// 이곳을 코멘트아웃하면, 시작시의 블록의 배열이             
		// 왼쪽 위부터 순서대로 나열된 것처럼 된다.
		for(int i = 0;i < places.size() - 1;i++) {

			int j = Random.Range(i + 1, places.size());

			places.swap(i, j);
		}
	#endif
		this.block_feeder.connect_arrow_num = 1;

		foreach(StackBlock.PlaceIndex place in places) {

			StackBlock	block = this.blocks[place.x, place.y];

			block.setColorType(this.block_feeder.getNextColorStart(place.x, place.y));
			block.setVisible(true);
		}
	}

	public void		update()
	{

		this.is_swap_end_frame = false;

		// ---------------------------------------------------------------- //
	#if false
		// "0" を押すと、上からブロックが降ってくる.
		if(Input.GetKeyDown(KeyCode.Keypad0)) {

			this.blockFallRequest();
		}


		// "1" を押すと、カラーチェンジ.
		if(Input.GetKeyDown(KeyCode.Keypad1)) {

			this.startColorChange();
		}

		// "2" を押すと、リスタート.
		if(Input.GetKeyDown(KeyCode.Keypad2)) {

			for(int y = 0;y < BLOCK_NUM_Y;y++) {
	
				for(int x = 0;x < BLOCK_NUM_X;x++) {

					StackBlock	block = this.blocks[x, y];

					block.setUnused();					
				}
			}

			this.setColorToAllBlock();
		}

		// "8" を押すと、メガクラッシュ.
		if(Input.GetKeyDown(KeyCode.Keypad8)) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {
	
				for(int y = GROUND_LINE - this.fall_count;y < BLOCK_NUM_Y;y++) {

					StackBlock	block = this.blocks[x, y];

					block.beginVanishAction();
				}
			}
		}

		if(Input.GetKeyDown(KeyCode.Keypad9)) {

			//this.CheckConnect();
			//this.block_feeder.beginFeeding();
			
			for(int	x = 0;x < BLOCK_NUM_X;x++) {

				StackBlock	block = this.blocks[x, BLOCK_NUM_Y - 1];

				block.beginIdle(this.block_feeder.getNextColorAppearFromBottom(x));
			}
		}
	#endif

		// ---------------------------------------------------------------- //
        // 비어있는 블록(연쇄 후의 회색)이 있다면 Swap 동작을 시작한다.                                               

		for(int x = 0;x < BLOCK_NUM_X;x++) {

			for(int y = GROUND_LINE;y < BLOCK_NUM_Y - 1;y++) {

				StackBlock	upper_block = this.blocks[x, y];
				StackBlock	under_block = this.blocks[x, y + 1];

				do {

					if(!(upper_block.isVacant() && !under_block.isVacant())) {

						break;
					}

					if(upper_block.isNowSwapAction() || under_block.isNowSwapAction()) {

						break;
					}

					//

					StackBlock.beginSwapAction(upper_block, under_block);

					this.scene_control.playSe(SceneControl.SE.SWAP);

				} while(false);

			}
		}

		// ---------------------------------------------------------------- //
		// 비어 있는 블록이 가장 아래에 도달하면 새롭게 색을 설정한다.

		for(int x = 0;x < BLOCK_NUM_X;x++) {

			StackBlock	block = this.blocks[x, BLOCK_NUM_Y - 1];

			if(!block.isVacant()) {

				continue;
			}

			if(block.isNowSwapAction()) {

				continue;
			}

			block.beginIdle(this.block_feeder.getNextColorAppearFromBottom(x));
		}


		// ---------------------------------------------------------------- //
        // Swap동작이 종료한 순간을 점검한다.

		// 『종료한 순간』을 파악하기 위해, 앞 프레임의 결과를 저장한다                        

		bool	is_has_swap_block_prev = this.is_has_swap_block;

		this.is_has_swap_block = false;

		foreach(StackBlock block in this.blocks) {

			if(block.isVanishAfter()) {

				this.is_has_swap_block = true;
				break;
			}
		}

		if(is_has_swap_block_prev && !this.is_has_swap_block) {

			this.is_swap_end_frame = true;
		}

		// ---------------------------------------------------------------- //
		// 위에서 블록이 내려온다.

		do {

			if(this.fall_request <= 0) {

				break;
			}

			if(this.fall_count >= FALL_LINE_NUM) {

				break;
			}

			int		y = GROUND_LINE - 1 - this.fall_count;

			Block.COLOR_TYPE[] colors = this.block_feeder.getNextColorsAppearFromTop(y);

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				StackBlock	block = this.blocks[x, y];

				block.beginFallAction(colors[x]);
			}

			this.fall_count++;
			this.fall_request--;

		} while(false);

		// ---------------------------------------------------------------- //
		// 낙하가 종료되면 전체를 스크롤

		this.scroll_control();

		// ---------------------------------------------------------------- //
		// 연쇄 체크

		if(this.is_swap_end_frame) {
	
			this.CheckConnect();
		}

		// ---------------------------------------------------------------- //
		// 케익 위에 블록이 낙하한 경우에는 케익이 올라온다.                        
		// （친절 설계）.

		if(this.block_feeder.cake.is_enable) {

			for(int y = StackBlockControl.GROUND_LINE + 1;y < StackBlockControl.BLOCK_NUM_Y;y++) {

				int	x = this.block_feeder.cake.x;

				if(!this.blocks[x, y].isCakeBlock()) {

					continue;
				}

				// 드롭한 블록이 착지할 때까지 가장 위의 블록은 비표시하게 된다.
				// 비표시 구간동안은 통과.
				if(!this.blocks[x, y - 1].isVisible()) {

					continue;
				}

                // 연쇄후의 블록은 그냥 두더라도 Swap된다 
				// （ VanishAction 의 도중에 색이 바뀌기 때문에 스킵하지 않으면 안된다.)
				// 　
				if(this.blocks[x, y - 1].isVanishAfter()) {

					continue;
				}

				//

				StackBlock.beginSwapAction(this.blocks[x, y - 1], this.blocks[x, y]);
			}
		}
	}

	// 블록의 낙하가 종료되었을 경우의 스크롤 제어.              
	private void	scroll_control()
	{
		do {

			if(this.fall_count <= 0) {

				break;
			}

			//

			bool	is_has_fall_block = false;

			for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

				StackBlock	block = this.blocks[x, StackBlockControl.GROUND_LINE - 1];

				if(block.isNowFallAction()) {

					is_has_fall_block = true;
					break;
				}
			}

			if(!is_has_fall_block) {

				if(this.is_scroll_enable) {

					// 케익이 스크롤아웃되었을 경우의 처리.
					if(this.block_feeder.cake.is_enable) {

						StackBlock	block = this.blocks[this.block_feeder.cake.x, StackBlockControl.BLOCK_NUM_Y - 1];
	
						if(block.isCakeBlock()) {
	
							this.block_feeder.onDropBlock(block);
						}
					}

					// 지면의 블록은 1열씩 아래로 내린다.
					for(int y = StackBlockControl.BLOCK_NUM_Y - 1;y >= StackBlockControl.GROUND_LINE;y--) {
		
						for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
		
							this.blocks[x, y].relayFrom(this.blocks[x, y - 1]);
						}
					}
	
					// 낙하중인 라인이 복수인 경우, 낙하중인 라인을 한 개씩 내린다.
					if(this.fall_count >= 2) {
		
						for(int y = StackBlockControl.GROUND_LINE - 1;y > StackBlockControl.GROUND_LINE - 1 - (this.fall_count - 1);y--) {
			
							for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
			
								this.blocks[x, y].relayFrom(this.blocks[x, y - 1]);
							}
						}
					}
					
					// 낙하중인 라인의 가장위의 라인을 비표시한다.
					for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
		
						this.blocks[x, StackBlockControl.GROUND_LINE - 1 - (this.fall_count - 1)].setUnused();
					}
				}

				this.fall_count--;

				this.scene_control.heightGain();

				this.scene_control.playSe(SceneControl.SE.JUMP);
				this.scene_control.playSe(SceneControl.SE.LANDING);
			}

		} while(false);
	}

	// 연쇄 체크  
	public bool	CheckConnect()
	{
		bool	ret = false;

		if(this.is_connect_check_enable) {

			ret = this.check_connect_sub();
		}

		return(ret);
	}

	private bool	check_connect_sub()
	{

		bool	is_connect = false;

		int		connect_num = 0;

		this.connect_checker.clearAll();

		for(int y = GROUND_LINE;y < StackBlockControl.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

				if(!this.blocks[x, y].isConnectable()) {

					continue;
				}

				// 같은 색으로 나열된 블록의 수를 체크한다.   

				int connect_block_num = this.connect_checker.checkConnect(x, y);

				// 같은 색의 배열이 4개 이하인 경우 지울 수 없다.

				if(connect_block_num < 4) {

					continue;
				}

				connect_num++;

				// 연결된 블록을 지운다.

				for(int i = 0;i < connect_block_num;i++) {

					StackBlock.PlaceIndex index = this.connect_checker.connect_block[i];

					this.blocks[index.x, index.y].beginVanishAction();
				}

				//

				this.eliminate_count += connect_block_num;
				is_connect = true;

				//

				this.continuous_connect_num++;
				this.score += this.continuous_connect_num*connect_block_num;
			}
		}

		//

		if(is_connect) {

			if(this.combo.is_now_combo) {

				this.combo.combo_count_current++;

				// 연쇄되면 위에서 블록을 내린다.
				this.fall_request++;

				this.scene_control.playSe(SceneControl.SE.COMBO);

			} else {

				this.combo.is_now_combo = true;
				this.combo.combo_count_current  = 1;
			}

			this.scene_control.playSe(SceneControl.SE.DROP_CONNECT);

			// 블록을 일정 개수 지울 때마다 케익을 출현시킨다.
			//
			do {

				// 케익이 출현중이거나 출현대기중에는 카운트다운하지 않는다.
				if(this.block_feeder.isCakeAppeared()) {

					break;
				}
				if(this.block_feeder.isCakeRequested()) {

					break;
				}

				this.eliminate_to_cake -= connect_num;

				if(this.eliminate_to_cake <= 0) {

					this.block_feeder.requestCake();
					this.eliminate_to_cake = ELIMINATE_TO_CAKE_INIT_2ND;

					// 바로 블록이 내려오게 한다.
					if(this.fall_request == 0) {

						this.fall_request++;
					}
				}

			} while(false);

			this.eliminate_to_fall -= connect_num;

		} else {

			// 연쇄가 끝났다.

			// 연쇄하지 않더라도 블록을 일정수 지우면 위에서 블록이 내려온다.              
			if(this.combo.combo_count_current > 1) {

				// 연쇄（＝ブ블록이 내려온다)되었으므로, 나머지 개수를 재설정.

				this.eliminate_to_fall = ELIMINATE_TO_FALL_INIT;

			} else {

				// 연쇄하지 않았다.


				// 블록을 일정수 지우고 위에서 블록을 내린다.

				if(this.eliminate_to_fall <= 0) {

					if(this.fall_request == 0) {

						this.fall_request++;
					}

					this.eliminate_to_fall = ELIMINATE_TO_FALL_INIT;
				}
			}

			this.combo.is_now_combo = false;
			this.combo.combo_count_last = this.combo.combo_count_current;
			this.combo.combo_count_current = 0;

			this.continuous_connect_num = 0;
		}

		// 목표까지의 나머지 이상으로 블록을 내리지 않는다..
		if(SceneControl.MAX_HEIGHT_LEVEL - this.scene_control.height_level < this.fall_request) {

			this.fall_request = SceneControl.MAX_HEIGHT_LEVEL - this.scene_control.height_level;
		}

		return(is_connect);
	}

	// 블록을 들어올렸을 때에 할 일
	public void	pickBlock(int x)
	{
		for(int y = GROUND_LINE;y < BLOCK_NUM_Y - 1;y++) {

			this.blocks[x, y].relayFrom(this.blocks[x, y + 1]);
		} 

		// 만일에 대비해 가장 위의 블록을 표시 ON으로 해둔다.
		// （버리기 직후에 비표시가 된 경우도 있기 때문에）.
		this.blocks[x, GROUND_LINE].setVisible(true);

		// 가장 아래에 새롭게 블록을 발생시킨다.         
		this.blocks[x, BLOCK_NUM_Y - 1].setColorType(this.block_feeder.getNextColorAppearFromBottom(x));

		this.blocks[x, BLOCK_NUM_Y - 1].swap_action.init();
	}

	// 블록을 버릴 때 할 일  
	public void	dropBlock(int x, Block.COLOR_TYPE color, int org_x)
	{
		// 블록이 (플레이어의 조작으로) 화면 아래로 밀렸을 때의 처리
		this.block_feeder.onDropBlock(this.blocks[x, BLOCK_NUM_Y - 1]);

		// 한 개씩 아래로 내린다.
		for(int y = BLOCK_NUM_Y - 1;y > GROUND_LINE;y--) {

			this.blocks[x, y].relayFrom(this.blocks[x, y - 1]);
		} 

		// 가장 위의 블록을 버려진 블록과 같은 색으로 설정한다.   
		this.blocks[x, GROUND_LINE].beginIdle(color);

		// 옮기는 블록이 착지할 때까지 비표시한다.
		this.blocks[x, GROUND_LINE].setVisible(false);
		this.blocks[x, GROUND_LINE].swap_action.is_active = false;
		this.blocks[x, GROUND_LINE].color_change_action.is_active = false;
		this.blocks[x, GROUND_LINE].position_offset.y = this.blocks[x, GROUND_LINE + 1].position_offset.y;

		// 위에서 내려오는 블록이 충돌하는 경우 막기 위해 속도를 적절히 초기화해둔다.
		this.blocks[x, GROUND_LINE].velocity.y = -StackBlock.OFFSET_REVERT_SPEED;

		if(color == Block.COLOR_TYPE.RED) {

			this.blocks[x, GROUND_LINE].step = StackBlock.STEP.VACANT;
		}
	}

	// 옮기는 블록이 낙하가 끝난(착지한) 경우의 처리.
	// （옮기는 블록＝플레이어가 들고 있는 블록）.
	//
	public void	endDropBlockAction(int x)
	{
		if(this.is_has_swap_block) {

            // swap 중인 블록이 있는 경우에는 연쇄가 일어나지 않는다.               

		} else {

			// 연쇄 체크
			this.CheckConnect();
		}

		// 떨어뜨린 위치에 있는 쌓아져있던 블록의 표시를 ON으로 설정한다.
		// （옮기는 블록이 비표시로 되어 있으므로)
		//
		this.blocks[x, GROUND_LINE].setVisible(true);

		this.scene_control.playSe(SceneControl.SE.DROP);
	}


	// 장소 인덱스로 좌표를 구한다.
	public static Vector3	calcIndexedPosition(StackBlock.PlaceIndex place)
	{
		Vector3		position;

		position.x = (-(BLOCK_NUM_X/2.0f - 0.5f) + place.x)*Block.SIZE_X;

		position.y = (-0.5f - (place.y - GROUND_LINE))*Block.SIZE_Y;

		position.z = 0.0f;

		return(position);
	}

	// 케익을 먹을 때의 처리
	public void	onEatCake()
	{
		this.block_feeder.onEatCake();

		this.startColorChange();
	}

	// 블록의 색이 바뀐다(특정 색이 된다.)
	// 케익을 먹을 때의 효과.
	public void	startColorChange()
	{
		int		color_index = 0;

		var		after_color = new Block.COLOR_TYPE[2];

		// ------------------------------------------------ //
		// 되도록 전와 달리 랜덤하게 색을 두 개 선택한다.

		var		candidates = new StaticArray<int>(Block.NORMAL_COLOR_NUM);

		for(int i = 0;i < Block.NORMAL_COLOR_NUM;i++) {

			// 전회와 같은 색은 후보에서 제외한다.
			if(i == this.change_color_index0 || i == this.change_color_index1) {

				continue;
			}
			if(!this.is_color_enable[i]) {

				continue;
			}

			//

			candidates.push_back(i);
		}

		// 0 ～ N - 1 의 난수를 중복없도록 두 개 선택하기 위해
		//
		// color0 = 0 ～ N - 2 의 범위의 난수
        // color1 = color0 ～ N - 1의 범위의 난수
		//
		// 를 선택한다.
		this.change_color_index0 = Random.Range(0, candidates.size() - 1);
		this.change_color_index1 = Random.Range(this.change_color_index0 + 1, candidates.size());

		this.change_color_index0 = candidates[this.change_color_index0];
		this.change_color_index1 = candidates[this.change_color_index1];

		// ------------------------------------------------ //
		// 블록의 색을 바꾼다.

		after_color[0] = (Block.COLOR_TYPE)change_color_index0;
		after_color[1] = (Block.COLOR_TYPE)change_color_index1;

		for(int x = 0;x < BLOCK_NUM_X;x++) {

			for(int y = GROUND_LINE - this.fall_count;y < BLOCK_NUM_Y;y++) {

				StackBlock	block = this.blocks[x, y];

				if(block.isVacant()) {

					continue;
				}

				// 처음 부분이 색 변경후의 색인 경우에는 스킵
				if(System.Array.Exists(after_color, c => c == block.color_type)) {

					continue;
				}

				if(!block.isNormalColorBlock()) {

					continue;
				}

				// 색 변경을 시작한다.

				Block.COLOR_TYPE	target_color;

				target_color = after_color[color_index%after_color.Length];

				block.beginColorChangeAction(target_color);

				color_index++;
			}
		}
	}

	// 위에서 블록을 내리게하여(목표점 연출)
	public void		blockFallRequest()
	{
		this.fall_request++;

		//this.is_fall_request = true;
	}
	
	// N번째의 유효한 색을 설정한다.
	public	Block.COLOR_TYPE	getNthEnableColor(int n)
	{
		Block.COLOR_TYPE	color = Block.COLOR_TYPE.NONE;

		int		sum = 0;

		for(int i = 0;i < (int)Block.COLOR_TYPE.NUM;i++) {

			if(!this.is_color_enable[i]) {

				continue;
			}

			if(sum == n) {

				color = (Block.COLOR_TYPE)i;
				break;
			}

			sum++;
		}

		return(color);
	}

}
