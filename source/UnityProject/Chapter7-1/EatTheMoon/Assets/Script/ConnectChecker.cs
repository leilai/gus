using UnityEngine;
using System.Collections;

public class ConnectChecker {

	public StackBlockControl	stack_control = null;

	public StackBlock[,]	blocks;

	public enum CONNECT_STATUS {

		NONE = -1,

		UNCHECKED = 0,
		CONNECTED,

		NUM,
	};

	public CONNECT_STATUS[,]		connect_status = null;

	public StackBlock.PlaceIndex[]	connect_block;

	// ---------------------------------------------------------------- //

	public void create()
	{
		this.connect_status = new CONNECT_STATUS[StackBlockControl.BLOCK_NUM_X, StackBlockControl.BLOCK_NUM_Y];

		this.connect_block = new StackBlock.PlaceIndex[StackBlockControl.BLOCK_NUM_X*StackBlockControl.BLOCK_NUM_Y];
	}

	public void clearAll()
	{
		for(int y = 0;y < StackBlockControl.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

				this.connect_status[x, y] = CONNECT_STATUS.UNCHECKED;
			}
		}

	}

	// (x, y) 의 위치에 연결된 블록을 체크한다.
	public int	checkConnect(int x, int y)
	{
		//

		int connect_num = this.check_connect_recurse(x, y, Block.COLOR_TYPE.NONE, 0);

		for(int i = 0;i < connect_num;i++) {

			StackBlock.PlaceIndex index = this.connect_block[i];

			this.connect_status[index.x, index.y] = CONNECT_STATUS.CONNECTED;
		}

		return(connect_num);
	}
	private bool	is_error_printed = false;

	private int		check_connect_recurse(int x, int y, Block.COLOR_TYPE previous_color, int connect_count)
	{
		StackBlock.PlaceIndex	block_index;

		do {

			// 무한 루프 방지 체크
			if(connect_count >= StackBlockControl.BLOCK_NUM_X*StackBlockControl.BLOCK_NUM_Y) {

				if(!this.is_error_printed) {

					Debug.LogError("Suspicious recursive call");
					this.is_error_printed = true;
				}
				break;
			}

			// 연결 대상이 아니다(공중에 있거나, 비표시 중임)
			if(!this.blocks[x, y].isConnectable()) {

				break;
			}

			// 이미 다른 블록과 연결되어 있다면 통과         
			//
			if(this.connect_status[x, y] == CONNECT_STATUS.CONNECTED) {

				break;
			}

			//

			block_index.x = x;
			block_index.y = y;

			// 이미 체크가 끝났다면 통과    
			if(this.is_checked(block_index, connect_count)) {

				break;
			}

			//

			if(previous_color == Block.COLOR_TYPE.NONE) {

				// 가장 처음 1번째

				this.connect_block[0] = block_index;

				connect_count = 1;

			} else {

				// 2번째 이후는 앞의 블록과 같은 색인지 체크

				if(this.blocks[x, y].color_type == previous_color) {
	
					this.connect_block[connect_count] = block_index;

					connect_count++;
				}
			}

			// 같은 색블록으로 연결되어 있다면, 주변도 체크한다.

			if(previous_color == Block.COLOR_TYPE.NONE || this.blocks[x, y].color_type == previous_color) {
	
				// 좌
				if(x > 0) {
		
					connect_count = this.check_connect_recurse(x - 1, y, this.blocks[x, y].color_type, connect_count);
				}
				// 우
				if(x < StackBlockControl.BLOCK_NUM_X - 1) {
		
					connect_count = this.check_connect_recurse(x + 1, y, this.blocks[x, y].color_type, connect_count);
				}
				// 상
				if(y > 0) {
	
					connect_count = this.check_connect_recurse(x, y - 1, this.blocks[x, y].color_type, connect_count);
				}
				// 하
				if(y < StackBlockControl.BLOCK_NUM_Y - 1) {
	
					connect_count = this.check_connect_recurse(x, y + 1, this.blocks[x, y].color_type, connect_count);
				}
			}

		} while(false);

		return(connect_count);
	}

	// 모두 체크 종료?
	private bool	is_checked(StackBlock.PlaceIndex place, int connect_count)
	{
		bool	is_checked = false;

		for(int i = 0;i < connect_count;i++) {

			if(this.connect_block[i].Equals(place)) {

				is_checked = true;
				break;
			}
		}

		return(is_checked);
	}

}
