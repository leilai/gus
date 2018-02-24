using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	// 블록 타입(색)
	public enum COLOR_TYPE {

		NONE = -1,

		CYAN = 0,
		YELLOW,
		ORANGE,
		MAGENTA,
		GREEN,
		PINK,

		RED,			// 연쇄 동작 후
        GRAY,			// 연쇄 동작 후
		CAKE0,			// 케익
		CAKE1,			// 케익

		NUM,

	};

	public static int			NORMAL_COLOR_NUM   = (int)COLOR_TYPE.RED;
	public static COLOR_TYPE	NORMAL_COLOR_FIRST = COLOR_TYPE.CYAN;
	public static COLOR_TYPE	NORMAL_COLOR_LAST  = COLOR_TYPE.PINK;
	public static COLOR_TYPE	CAKE_COLOR_FIRST = COLOR_TYPE.CAKE0;
	public static COLOR_TYPE	CAKE_COLOR_LAST  = COLOR_TYPE.CAKE1;
	
	public COLOR_TYPE	color_type = (COLOR_TYPE)0;

	public static float	SIZE_X = 1.0f;
	public static float	SIZE_Y = 1.0f;

    // 각 색의 Material（실체는 SceneControl.cs）.
	public static Material[]	materials;

	// ---------------------------------------------------------------- //

	// 일반 색의 블록?
	public bool isNormalColorBlock()
	{
		bool	ret;

		do {
			
			ret = false;

			//

			if((int)this.color_type < (int)Block.NORMAL_COLOR_FIRST) {

				break;
			}
			if((int)this.color_type > (int)Block.NORMAL_COLOR_LAST) {

				break;
			}

			//

			ret = true;

		} while(false);

		return(ret);
	}

	// 케익? 
	public bool isCakeBlock()
	{
		bool	ret;

		do {
			
			ret = false;

			//

			if((int)this.color_type < (int)Block.CAKE_COLOR_FIRST) {

				break;
			}
			if((int)this.color_type > (int)Block.CAKE_COLOR_LAST) {

				break;
			}

			//

			ret = true;

		} while(false);

		return(ret);
	}

	public void	setColorType(COLOR_TYPE type)
	{
		this.color_type = type;

		switch(this.color_type) {

			case COLOR_TYPE.RED:
			{
				this.renderer.material = Block.materials[(int)COLOR_TYPE.RED];
				this.renderer.material.SetFloat("_BlendRate", 0.0f);
			}
			break;

			case COLOR_TYPE.GRAY:
			{
				this.renderer.material = Block.materials[(int)COLOR_TYPE.GRAY];
				this.renderer.material.SetFloat("_BlendRate", 1.0f);
			}
			break;

			case COLOR_TYPE.CAKE0:
			{
				this.renderer.material = Block.materials[(int)COLOR_TYPE.CAKE0];
			}
			break;

			default:
			{
				if(0 <= (int)this.color_type && (int)this.color_type < Block.materials.Length) {
		
					this.renderer.material = Block.materials[(int)this.color_type];
					this.renderer.material.SetFloat("_BlendRate", 0.0f);
				}
			}
			break;
		}
	}

	public void setVisible(bool is_visible)
	{
		this.renderer.enabled = is_visible;
	}

	public bool	isVisible()
	{
		return(this.renderer.enabled);
	}

	public static COLOR_TYPE	getNextNormalColor(COLOR_TYPE color)
	{
		int	next = (int)color;

		next++;

		if(next > (int)NORMAL_COLOR_LAST) {

			next = (int)NORMAL_COLOR_FIRST;
		}

		return((COLOR_TYPE)next);
	}
}
