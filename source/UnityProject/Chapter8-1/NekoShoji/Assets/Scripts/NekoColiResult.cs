using UnityEngine;
using System.Collections;

public class NekoColiResult  {

	public NekoControl	neko = null;

	public static float	THROUGH_GAP_LIMIT = 0.4f;			//!< 격자나무의 중심에서 벗어나면 문틀에 부딪힌다.
	public static float UNLOCK_DISTANCE = 3.0f;				//!< 유도를 해제하는 거리

	// ---------------------------------------------------------------- //

	// 미닫이문의 문틀에 부딪힌 경우의 여러가지 정보
	//
	public struct ShojiHitInfo {

		public bool			is_enable;

		public ShojiControl	shoji_control;

		public ShojiControl.HoleIndex	hole_index;
	};

	public ShojiHitInfo		shoji_hit_info;
	public ShojiHitInfo		shoji_hit_info_first;

	// 미닫이문의 구멍에 통과한 경우의 여러가지 정보.
	//
	public struct HoleHitInfo {

		public SyoujiPaperControl	paper_control;
	};

	public StaticArray<HoleHitInfo>	hole_hit_infos;

	// 장지문, 미닫이문의 문틀에 닿은 경우의 여러가지 정보.
	//
	public struct ObstacleHitInfo {

		public bool			is_enable;

		public GameObject	go;
		public bool			is_steel;
	};

	public ObstacleHitInfo	obstacle_hit_info;

	// 유도를 하는 미닫이문의 격자 
    //
	public struct LockTarget {

		public bool						enable;

		public ShojiControl.HoleIndex	hole_index;

		public Vector3					position;
	};

	public LockTarget	lock_target;

	public bool			is_steel = false;

	// ---------------------------------------------------------------- //

	public void create()
	{
		this.hole_hit_infos = new StaticArray<HoleHitInfo>(4);

		this.shoji_hit_info.is_enable       = false;
		this.shoji_hit_info_first.is_enable = false;

		this.obstacle_hit_info.is_enable = false;

		this.lock_target.enable = false;
	}

	// 앞 프레임의 콜리전 결과를 체크한다.                    

	public void	resolveCollision()
	{
		// 「격자 구멍」을 통과하여 일정거리 이동하면 유도를 해제한다.                    
		if(this.lock_target.enable) {

			if(this.neko.transform.position.z > this.lock_target.position.z + UNLOCK_DISTANCE) {

				this.lock_target.enable = false;
			}
		}

		if(!this.lock_target.enable) {

			this.resolve_collision_sub();
		}
	}

	private void	resolve_collision_sub()
	{
		bool	is_collied_obstacle = false;

		this.is_steel = false;

		// 장지문/철판에 닿았는지를 처음에 점검한다.
		//
        // 장지문/철판에 닿은 경우라도 구멍을 통과한 경우에는 실패로 간주하지 않으므로

		if(this.obstacle_hit_info.is_enable) {

			is_collied_obstacle = true;

			this.is_steel = this.obstacle_hit_info.is_steel;
		}

		//

		if(this.shoji_hit_info.is_enable) {

			// 문틀과 닿았다?.

			ShojiControl			shoji_control = this.shoji_hit_info.shoji_control;
			ShojiControl.HoleIndex	hole_index    = this.shoji_hit_info.hole_index;

			if(shoji_control.isValidHoleIndex(hole_index)) {

				SyoujiPaperControl		paper_control = shoji_control.papers[hole_index.x, hole_index.y];

				if(paper_control.isSteel()) {

					// 격자 구멍이 철판인 경우.

					is_collied_obstacle = true;

					this.is_steel = true;

				} else  {

					// 격자 구멍이 「종이」「찢어진 종이」인 경우.

					// 「격자 구멍」으로 유도할 때의 목표위치
					Vector3	position = NekoColiResult.get_hole_homing_position(shoji_control, hole_index);

					//
	
					Vector3	diff = this.neko.transform.position - position;
	
					//Debug.Log(diff.x.ToString() + " " + diff.y);
	
					if(Mathf.Abs(diff.x) < THROUGH_GAP_LIMIT && Mathf.Abs(diff.y) < THROUGH_GAP_LIMIT) {
	
						// 구멍의 중심에서 어느 정도 가까운 위치를 통과하면 격자 구멍을 통과한 것으로 간주.
						// （유도）.

						is_collied_obstacle = false;

						this.lock_target.enable     = true;
						this.lock_target.hole_index = hole_index;
						this.lock_target.position   = position;


						// 「격자 구멍」모델에게 플레이어가 충돌한 것을 알린다.
						paper_control.onPlayerCollided();

					} else {

						// 구멍의 중심에서 멀리 벗어난 경우에는 격자에 부딪힌 것으로 간주한다. 

						is_collied_obstacle = true;
					}
				}

			} else {

				// 미닫이문의 격자 구멍 이외의 장소에 부딪힌 경우.

				is_collied_obstacle = true;
			}

		} else {

			// 문틀과 충돌하지 않은 경우에는 두 개 이상의 「격자 구멍」과 충돌하는 경우는 존재하지 않는다.
            // （두 개의「격자 구멍」과 충돌할 때에는 그 사이에 있는 나무틀에도 충돌하기 때문에）
			// 때문에 나무틀과 충돌하지 않은 경우에는 this.hole_hit_infos[0]만 점검해도 충분하다.
			if(this.hole_hit_infos.size() > 0) {

				// 「격자 구멍」에만 충돌한다.

				HoleHitInfo			hole_hit_info = this.hole_hit_infos[0];
				SyoujiPaperControl	paper_control = hole_hit_info.paper_control;
				ShojiControl		shoji_control = paper_control.shoji_control;

				paper_control.onPlayerCollided();

				// 「격자 구멍」을 통과한다.
				// （유도）.

                // lock한다.（유도한 목표위치에 설정한다）.

				// 「격자 구멍」의 중심을 구한다.

				ShojiControl.HoleIndex	hole_index = paper_control.hole_index;

				Vector3	position = NekoColiResult.get_hole_homing_position(shoji_control, hole_index);

				this.lock_target.enable = true;
				this.lock_target.hole_index = hole_index;
				this.lock_target.position   = position;
			}
		}


		if(is_collied_obstacle) {

			// 장애물（미닫이문의 문틀, 장지문)에 충돌하였다.

			if(this.neko.step != NekoControl.STEP.MISS) {

				this.neko.beginMissAction(this.is_steel);
			}
		}

	}


	// 「격자 구멍」에 유도할 때의 목표위치.
	private static Vector3	get_hole_homing_position(ShojiControl shoji_control, ShojiControl.HoleIndex hole_index)
	{
		Vector3		position;
	
		position = shoji_control.getHoleWorldPosition(hole_index.x, hole_index.y);
	
		// 콜리전의 중심에서 오브젝트의 원점에 오프셋.
		// 콜리전 중심이 「격자 구멍」의 중심을 통과하도록 한다.
		position += -NekoControl.COLLISION_OFFSET;

		return(position);
	}

}
