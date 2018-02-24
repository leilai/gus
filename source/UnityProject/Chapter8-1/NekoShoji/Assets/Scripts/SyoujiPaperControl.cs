using UnityEngine;
using System.Collections;

public class SyoujiPaperControl : MonoBehaviour {

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		PAPER = 0,			//
		BROKEN,
		STEEL,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	// ---------------------------------------------------------------- //

	private SceneControl	scene_control = null;

	public ShojiControl	shoji_control = null;

	public ShojiControl.HoleIndex	hole_index;

	public GameObject	paper_object = null;	// 종이
	public GameObject	broken_object = null;	// 찢어진 종이
	public GameObject	steel_object = null;	// 철판

	// Audio
	public AudioClip SUCCESS_SOUND = null;

	public int		through_count = 0;			// 구멍이 뚫린후에 통과하는 횟수

	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {

		this.scene_control = GameObject.FindWithTag("MainCamera").GetComponent<SceneControl>();

		for(int i = 0;i < this.transform.GetChildCount();i++) {

			GameObject	child = this.transform.GetChild(i).gameObject;

			switch(child.tag) {

				case "SyoujiPaper":
				{
					this.paper_object = child;
				}
				break;
				case "SyoujiPaperBroken":
				{
					this.broken_object = child;
				}
				break;
				case "SyoujiSteel":
				{
					this.steel_object = child;
				}
				break;

			}
		}

		this.paper_object.active  = false;
		this.broken_object.active = false;
		this.steel_object.active  = false;

		this.next_step = STEP.PAPER;
	}
	
	// Update is called once per frame
	void Update () {
	
		// ---------------------------------------------------------------- //

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크한다.

		if(this.next_step == STEP.NONE) {

			switch(this.step) {

				case STEP.PAPER:
				{
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환될 때의 초기화.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.PAPER:
				{
					this.paper_object.active  = true;
					this.broken_object.active = false;
					this.steel_object.active  = false;
				}
				break;

				case STEP.BROKEN:
				{
					this.paper_object.active  = false;
					this.broken_object.active = true;
					this.steel_object.active  = false;
	
					this.audio.PlayOneShot(SUCCESS_SOUND);
				}
				break;

				case STEP.STEEL:
				{
					this.paper_object.active  = false;
					this.broken_object.active = false;
					this.steel_object.active  = true;
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

			case STEP.PAPER:
			{
			}
			break;

		}
	}

	// 철판으로 설정한다.
	public void	beginSteel()
	{
		this.next_step = STEP.STEEL;
	}

	// 철판?
	public bool	isSteel()
	{
		bool	ret;

		ret = (this.step == STEP.STEEL);

		return(ret);
	}

	// 공중 점프 카운터(찢어진 후에 통과하는 횟수)를 재설정한다.               
	public void	resetThroughCount()
	{
		this.through_count = 0;
	}

	// 플레이어가 충돌한(통과한) 때에 불러온다.
	public void	onPlayerCollided()
	{
		switch(this.step) {

			case STEP.PAPER:
			{
				this.next_step = STEP.BROKEN;

				this.scene_control.addComboCount();

				this.shoji_control.onPaperBreak();

				// 紙をやぶったときのエフェクト.
				this.scene_control.neko_control.effect_control.createBreakEffect(this, this.scene_control.neko_control);
			}
			break;

			case STEP.BROKEN:
			{
				this.through_count++;

				this.scene_control.clearComboCount();
			}
			break;
		}
	}
}
