using UnityEngine;
using System.Collections;

// 다음에 등장할 블록의 색을 정한다.
public class BlockFeeder {

	public StackBlockControl	control = null;

	private StaticArray<int>				connect_num = null;
	private StaticArray<Block.COLOR_TYPE>	candidates  = null;		// 출현할 색 후보.

	// 케익 등장
	public struct Cake {

		public bool				is_enable;
		public int				x;
		public Block.COLOR_TYPE	color_type;		// 컬러 타입(케익은 현재 2종류 존재)
    };

	public Cake			cake;				// 대기 중인 케익
	private int			cake_count = 0;		// 출현하는 케익
	private int			cake_request = 0;

	// ---------------------------------------------------------------- //

	public void	create()
	{
		this.connect_num = new StaticArray<int>(Block.NORMAL_COLOR_NUM);
		this.connect_num.resize(this.connect_num.capacity());

		this.candidates = new StaticArray<Block.COLOR_TYPE>(Block.NORMAL_COLOR_NUM);

		this.cake.is_enable  = false;
	}

    // 올라가는 배열（같은 색이 4개 나열되어 있다)의 작성가능 수
	public int	connect_arrow_num = 1;

	// 다음 블록의 색을 취득한다.(게임 시작시에 전부 채울 때의 용도)
	public Block.COLOR_TYPE	getNextColorStart(int x, int y)
	{
#if false
		Block.COLOR_TYPE	color_type;

		color_type = (Block.COLOR_TYPE)Random.Range((int)Block.NORMAL_COLOR_FIRST, (int)Block.NORMAL_COLOR_LAST + 1);

		return(color_type);
#else
		StackBlock[,]		blocks          = this.control.blocks;
		ConnectChecker		connect_checker = this.control.connect_checker;
		Block.COLOR_TYPE	org_color;
		int					sel;

		//

		org_color = blocks[x, y].color_type;

		// 『출현할 색 후보 리스트』를 초기화한다.
		// （리스트에 색 전부를 저장하도록 한다.）.
		this.init_candidates();

		// 각 색을 배치한 후 같은 색이 몇 개 배열되어 있는지를 점검한다.

		for(int i = 0;i < (int)Block.NORMAL_COLOR_NUM;i++) {

			blocks[x, y].setColorType((Block.COLOR_TYPE)i);

			connect_checker.clearAll();

			this.connect_num[i] = connect_checker.checkConnect(x, y);
		}

		if(this.connect_arrow_num > 0) {

			// 아직 올라가는 배열이 만들어지더라도 괜찮은 경우.

			// connect_num[]  중 최대값（최대 max_num 개 같은 수의 색 블록이 나열된다.)
			int		max_num = this.get_max_connect_num();

			// max_num 가 아닌 것을 삭제한다.（최대값인 경우만 후보로 남긴다.）.
			this.erase_candidate_if_not(max_num);

			sel = Random.Range(0, candidates.size());

			// 같은 색이 4개 배열되면, 올라가는 배열의 나머지 수를 줄인다.
			if(this.connect_num[(int)candidates[sel]] >= 4) {

				this.connect_arrow_num--;
			}

		} else {

			// 더 이상 올라가는 배열을 만들 수 없는 경우.

			// 같은 색이 4개 배열되는 색을 후보에서 삭제한다.
			for(int i = candidates.size() - 1;i >= 0;i--) {
	
				if(this.connect_num[(int)candidates[i]] >= 4) {

					candidates.erase_by_index(i);
				}
			}

			if(candidates.size() == 0) {

				this.init_candidates();
				Debug.Log("give up");
			}

			// connect_num[]  중 최대값（최대 max_num 개 같은 색 블록이 나열된다.)
			int		max_num = this.get_max_connect_num();

            // max_num 이 아닌 경우를 삭제한다.（최대값인 경우만 후보로 남긴다.）.
			this.erase_candidate_if_not(max_num);

			sel = Random.Range(0, candidates.size());
		}


		//

		blocks[x, y].setColorType(org_color);

		return((Block.COLOR_TYPE)candidates[sel]);
#endif
	}

	// 후보 리스트를 초기화한다.(모든 색을 후보에 저장한다.)
	private void	init_candidates()
	{
		this.candidates.resize(0);

		for(int i = 0;i < this.candidates.capacity();i++) {

			if(!this.control.is_color_enable[i]) {

				continue;
			}

			this.candidates.push_back((Block.COLOR_TYPE)i);
		}
	}

	// 같은 색의 블록이 배열되는 수가 가장 많은 색의 배열수를 취득한다.
	private int		get_max_connect_num()
	{
		int		sel = 0;

		for(int i = 1;i < candidates.size();i++) {

			if(this.connect_num[(int)this.candidates[i]] > this.connect_num[(int)this.candidates[sel]]) {

				sel = i;
			}
		}

		return(this.connect_num[(int)this.candidates[sel]]);
	}

	// 같은 색이 배열되어 있는 수가 connect_num 가 아닌 색을 삭제한다.
	private void	erase_candidate_if_not(int connect_num)
	{
		for(int i = candidates.size() - 1;i >= 0;i--) {
	
			if(this.connect_num[(int)candidates[i]] != connect_num) {
	
				candidates.erase_by_index(i);
			}
		}
	}

	// 지정한 색을 후보에서 삭제한다.
	private void	erase_color_from_candidates(Block.COLOR_TYPE color)
	{
		for(int i = candidates.size() - 1;i >= 0;i--) {

			if(candidates[i] == color) {
				
				candidates.erase_by_index(i);
			}
		}
	}

	// 다음 블록 색을 선택한다.(화면에서 내려오는 블록).
	public Block.COLOR_TYPE[] getNextColorsAppearFromTop(int y)
	{
		Block.COLOR_TYPE[]	colors = new Block.COLOR_TYPE[StackBlockControl.BLOCK_NUM_X];

		for(int i = 0;i < StackBlockControl.BLOCK_NUM_X;i++) {

			colors[i] = this.get_next_color_appear_from_top_sub(i, y, colors);
		}

		// 대기 중인 케익이 있는 경우, 케익을 표시한다.
		if(this.cake_request > 0) {

			this.cake.is_enable  = true;
			this.cake.x          = Random.Range(0, StackBlockControl.BLOCK_NUM_X);
			this.cake.color_type = (Block.COLOR_TYPE)((int)Block.CAKE_COLOR_FIRST + this.cake_count%2);

			colors[this.cake.x] = this.cake.color_type;

			this.cake_count++;

			this.cake_request = Mathf.Max(this.cake_request - 1, 0);
		}

		return(colors);
	}
	private Block.COLOR_TYPE	get_next_color_appear_from_top_sub(int x, int y, Block.COLOR_TYPE[] colors)
	{
		StackBlock[,]		blocks     = this.control.blocks;
		Block.COLOR_TYPE	color_type = Block.NORMAL_COLOR_FIRST;
		int					sel;

		this.init_candidates();

		// 먼저 왼쪽과 아래의 블록과 같은 색이 되지 않도록

		this.erase_color_from_candidates(blocks[x, y + 1].color_type);

		if(x > 0) {

			this.erase_color_from_candidates(colors[x - 1]);
		}

		//

		sel = Random.Range(0, candidates.size());

		color_type = this.candidates[sel];

		return(color_type);
	}

	// 다음 블록의 색을 선택한다.(하면 아래에서 새롭게 출현하는 블록)
	public Block.COLOR_TYPE	getNextColorAppearFromBottom(int x)
	{
		StackBlock[,]		blocks     = this.control.blocks;
		Block.COLOR_TYPE	color_type = Block.NORMAL_COLOR_FIRST;

		this.init_candidates();

		//

		int		y;

		for(y = (int)StackBlockControl.BLOCK_NUM_Y - 1 - 1;y >= 0;y--) {

			StackBlock	block = blocks[x, y];

			if(block.isConnectable()) {

				break;
			}
		}

		if(y >= 0) {

			Block.COLOR_TYPE	erase_color = blocks[x, y].color_type;

			for(int i = 0;i < candidates.size();i++) {

				if(candidates[i] == erase_color) {

					candidates.erase_by_index(i);
					break;
				}
			}
		}

		color_type = candidates[Random.Range(0, candidates.size())];

#if false
		int[]	block_num = new int[(int)Block.COLOR_TYPE.NORMAL_COLOR_NUM];

		for(int i = 0;i < (int)Block.COLOR_TYPE.NORMAL_COLOR_NUM;i++) {

			block_num[i] = 0;
		}

		//

		for(int i = -1;i <= 1;i++) {

			if(x + i < 0 || StackBlockControl.BLOCK_NUM_X <= x + i) {

				continue;
			}

			for(int y = 0;y < (int)StackBlockControl.BLOCK_NUM_Y;y++) {
	
				StackBlock	block = blocks[x, y];
	
				if(!block.isConnectable()) {
	
					continue;
				}
	
				block_num[(int)block.color_type]++;
			}
		}

		//

		int		sel = 0;

		for(int i = 1;i < (int)Block.COLOR_TYPE.NORMAL_COLOR_NUM;i++) {

			if(block_num[i] < block_num[sel]) {

				sel = i;
			}
		}

		//

		StaticArray<Block.COLOR_TYPE> candidates = new StaticArray<Block.COLOR_TYPE>();

		candidates.create((int)Block.COLOR_TYPE.NORMAL_COLOR_NUM);

		for(int i = 0;i < (int)Block.COLOR_TYPE.NORMAL_COLOR_NUM;i++) {

			if(block_num[i] <= block_num[sel]) {

				candidates.push_back((Block.COLOR_TYPE)i);
			}
		}

		color_type = candidates[Random.Range(0, candidates.size())];
#endif
		return(color_type);
	}

	// ---------------------------------------------------------------- //

	//블록이(플레이어의 조작으로) 화면 아래에서 등장하는 경우의 처리.
	public void	onDropBlock(StackBlock block)
	{
#if true
		do {

			if(!block.isCakeBlock()) {

				break;
			}

			if(!this.cake.is_enable) {

				break;
			}

			//

			this.clearCake();

		} while(false);

#else
		// ケーキが（プレイヤーの操作によって）画面下に押し出されたときは
		// ケーキの出現数を増やしておく.
		// （そうしないといつまでたってもケーキが画面に出てこなくなる時があるので）.
		if(block.isCakeBlock()) {

			if(this.cake.is_enable) {

				this.cake.is_enable  = true;
				this.cake.is_active  = false;
				this.cake.x          = block.place.x;
				this.cake.color_type = block.color_type;
			}
		}

		if(this.cake.is_enable && !this.cake.is_active) {

			this.cake.out_count++;
		}
#endif
	}

	// 케익이 나오고
	public void	requestCake()
	{
		if(!this.cake.is_enable) {

			this.cake_request++;
		}
	}

	// 케익을 먹었다.
	public void	onEatCake()
	{
		this.clearCake();
	}

	// 케익 출현중?
	public bool	isCakeAppeared()
	{
		return(this.cake.is_enable);
	}

	// 케익 대기중?
	public bool	isCakeRequested()
	{
		return(this.cake_request > 0);
	}

	// 케익 출현정보를 클리어한다.
	public void	clearCake()
	{
		this.cake.is_enable  = false;
		this.cake.x          = -1;
		this.cake.color_type = Block.COLOR_TYPE.NONE;
	}
}
