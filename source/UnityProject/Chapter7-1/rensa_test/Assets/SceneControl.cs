using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	public static int	BLOCK_NUM_X = 9;
	public static int	BLOCK_NUM_Y = 5;

	public GameObject	BlockPrefab = null;

	public BlockControl[,]	blocks;

	private bool		toggle_checked = false;

	void	Start()
	{
		// 블록을 생성, 배치한다.

		this.blocks = new BlockControl[BLOCK_NUM_X, BLOCK_NUM_Y];

		int		color_index = 0;

		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				GameObject game_object = Instantiate(this.BlockPrefab) as GameObject;

				BlockControl	block = game_object.GetComponent<BlockControl>();

				this.blocks[x, y] = block;

				//

				Vector3	position = block.transform.position;

				position.x = -(BLOCK_NUM_X/2.0f - 0.5f) + x;
				position.y = -0.5f                      - y;

				block.transform.position = position;

				block.SetColor((BlockControl.COLOR)color_index);

				//

				color_index = Random.Range(0, (int)BlockControl.COLOR.NORMAL_COLOR_NUM);
			}
		}

		//
	}
	
	void	Update()
	{
		// 스페이스키를 누르면, 연결체크 
		if(Input.GetKeyDown(KeyCode.Space)) {

			// 블록을 처음의 상태로 되돌린다.

			for(int y = 0;y < BLOCK_NUM_Y;y++) {
	
				for(int x = 0;x < BLOCK_NUM_X;x++) {
	
					this.blocks[x, y].ToBeVanished(false);	
				}
			}

            // 블록을 처음의 상태로 되돌린다.

			this.ClearConnect();

			if(!this.toggle_checked) {

				// 연결체크   
				this.CheckConnect();
			}

			this.toggle_checked = !this.toggle_checked;
		}
	}

	public enum CONNECT_STATUS {

		NONE = -1,

		UNCHECKED = 0,
		CONNECTED,

		NUM,
	};

	public CONNECT_STATUS[,]		connect_status = null;

	public struct BlockIndex {

		public int		x;
		public int		y;
	};

	public BlockIndex[]	connect_block;

	public void	CheckConnect()
	{
		// 모든 블록을『미체크』로 한다.

		this.connect_status = new CONNECT_STATUS[BLOCK_NUM_X, BLOCK_NUM_Y];

		this.connect_block = new BlockIndex[BLOCK_NUM_X*BLOCK_NUM_Y];

		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				this.connect_status[x, y] = CONNECT_STATUS.UNCHECKED;
			}
		}

		//

		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				int connect_num = this.check_connect_recurse(x, y, BlockControl.COLOR.NONE, 0);

				// 같은 색이 4개 이상 나열되어 있다면, 연결된 것으로 간주한다.
				if(connect_num >= 4) {

					for(int i = 0;i < connect_num;i++) {

						BlockIndex index = this.connect_block[i];

						this.connect_status[index.x, index.y] = CONNECT_STATUS.CONNECTED;

						this.blocks[index.x, index.y].ToBeVanished(true);
					}
				}
			}
		}
	}

	private int		check_connect_recurse(int x, int y, BlockControl.COLOR previous_color, int connect_count)
	{
		BlockIndex	block_index;

		do {

			// 이미 다른 블록과 연결되어 있다면 스킵                     
			//
			if(this.connect_status[x, y] == CONNECT_STATUS.CONNECTED) {

				break;
			}

			//

			block_index.x = x;
			block_index.y = y;

			// 이미 체크가 종료되었다면 스킵

			bool	is_checked = false;

			for(int i = 0;i < connect_count;i++) {

				if(this.connect_block[i].Equals(block_index)) {

					is_checked = true;
					break;
				}
			}
			if(is_checked) {

				break;
			}

			//

			if(previous_color == BlockControl.COLOR.NONE) {

				// 1번째

				this.connect_block[0] = block_index;

				connect_count = 1;

			} else {

				// ２번째 이후는 앞의 블록과 같은 색인지를 체크한다.                     

				if(this.blocks[x, y].color == previous_color) {
	
					this.connect_block[connect_count] = block_index;

					connect_count++;
				}
			}

			// 같은 색이 연결되어 있다면, 또한 주변도 체크한다.

			if(previous_color == BlockControl.COLOR.NONE || this.blocks[x, y].color == previous_color) {

				// 좌.		
				if(x > 0) {
		
					connect_count = this.check_connect_recurse(x - 1, y, this.blocks[x, y].color, connect_count);
				}
				// 우
				if(x < BLOCK_NUM_X - 1) {
		
					connect_count = this.check_connect_recurse(x + 1, y, this.blocks[x, y].color, connect_count);
				}
				// 상
				if(y > 0) {
		
					connect_count = this.check_connect_recurse(x, y - 1, this.blocks[x, y].color, connect_count);
				}
				// 하
				if(y < BLOCK_NUM_Y - 1) {
		
					connect_count = this.check_connect_recurse(x, y + 1, this.blocks[x, y].color, connect_count);
				}
		
				// 대각선 방향으로 배열된 경우만 삭제하도록    
				/*if(x > 0 && y > 0) {
		
					connect_count = this.check_connect_recurse(x - 1, y - 1, this.blocks[x, y].color, connect_count);
				}
				if(x > 0 && y < BLOCK_NUM_Y - 1) {
		
					connect_count = this.check_connect_recurse(x - 1, y + 1, this.blocks[x, y].color, connect_count);
				}
				if(x < BLOCK_NUM_X - 1 && y > 0) {
		
					connect_count = this.check_connect_recurse(x + 1, y - 1, this.blocks[x, y].color, connect_count);
				}
				if(x < BLOCK_NUM_X - 1 && y < BLOCK_NUM_Y - 1) {
		
					connect_count = this.check_connect_recurse(x + 1, y + 1, this.blocks[x, y].color, connect_count);
				}*/
			}
	

		} while(false);

		return(connect_count);
	}

	// 모든 블록을 『연결하지 않음』의 상태로 되돌린다.
	public void	ClearConnect()
	{
		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				this.blocks[x, y].ToBeVanished(false);
			}
		}
	}

}
