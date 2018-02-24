using UnityEngine;
using System.Collections;

public class GUIControl : MonoBehaviour {

	public SceneControl		scene_control = null;
	public ScoreControl		score_control = null;
	
	// 평가 문자.
    private float gui_eval_scale = 1.0f;		// scale.
    private float gui_eval_alpha = 1.0f;		// alpha.

	public static float	EVAL_ZOOM_TIME = 0.4f;
	
	// 『시작』 문자.
	public float	start_texture_x		= 0.0f;
	public float	start_texture_y		= 50.0f;
	
	// 평가 문자.
	public float	defeat_base_texture_x	=    0.0f;
	public float	defeat_base_texture_y	=   70.0f;
	public float	defeat_texture_x		=   70.0f;
	public float	defeat_texture_y		=   70.0f;
	public float	eval_base_texture_x		=    0.0f;
	public float	eval_base_texture_y		=  -40.0f;
	public float	eval_texture_x			=   70.0f;
	public float	eval_texture_y			=  -40.0f;
	public float	total_texture_x			=    0.0f;
	public float	total_texture_y			=    0.0f;
	
	public Texture	defeat_base_texture = null;
	public Texture	eval_base_texture = null;
	
	public Texture	result_excellent_texture = null;		// 『우수함』
	public Texture	result_good_texture = null;				// 『좋음』
	public Texture	result_nomal_texture = null;			// 『통과』
	public Texture	result_bad_texture = null;				// 『실패』
	public Texture	result_mini_excellent_texture = null;	// 『우수함』
	public Texture	result_mini_good_texture = null;		// 『좋음』
	public Texture	result_mini_nomal_texture = null;		// 『통과』
	public Texture	result_mini_bad_texture = null;			// 『실패』

	
	// 『되돌아가기』 문자.
	public float	return_texture_x	= 0.0f;
	public float	return_texture_y	= -150.0f;	
	// -------------------------------------------------------------------------------- //

	void	Start()
	{
		this.scene_control = GetComponent<SceneControl>();
		this.score_control = GetComponent<ScoreControl>();
		
		this.score_control.setNumForce( this.scene_control.result.oni_defeat_num );
	}
	
	void	Update()
	{
		//if(this.scene_control.IsDrawScore()) {
			
			this.score_control.setNum( this.scene_control.result.oni_defeat_num );
		//}
	}

	void	OnGUI()
	{
		//스코어
		if(this.scene_control.IsDrawScore()) {
			
			this.score_control.draw();
		}
		
		// 『시작』 문자.
		//this.scene_control.drawTitle();
		
		// 『시작』 문자.
		if(this.scene_control.step == SceneControl.STEP.START) {

			TitleSceneControl.drawTexture(this.scene_control.StartTexture, start_texture_x,  start_texture_y, 1.0f, 1.0f, 0.0f, 1.0f);
		}

		// 공격한 도깨비의 수에 대한 평가 표시
		if(this.scene_control.step == SceneControl.STEP.RESULT_DEFEAT) {
		
			TitleSceneControl.drawTexture(defeat_base_texture, defeat_base_texture_x, defeat_base_texture_y, this.gui_eval_scale, this.gui_eval_scale, 0.0f, this.gui_eval_alpha);
			TitleSceneControl.drawTexture(GetDefeatRankTexture(), defeat_texture_x,  defeat_texture_y, this.gui_eval_scale, this.gui_eval_scale, 0.0f, this.gui_eval_alpha);
		}
		
		// 공격한 도깨비의 수와 도깨비를 공격한 타이밍에 대한 평가 표시.
		if(this.scene_control.step == SceneControl.STEP.RESULT_EVALUATION) {
		
			TitleSceneControl.drawTexture(defeat_base_texture, defeat_base_texture_x, defeat_base_texture_y, 1.0f, 1.0f, 0.0f, 1.0f);
			TitleSceneControl.drawTexture(GetDefeatRankTexture(), defeat_texture_x,  defeat_texture_y, 1.0f, 1.0f, 0.0f, 1.0f);
			TitleSceneControl.drawTexture(eval_base_texture, eval_base_texture_x, eval_base_texture_y, this.gui_eval_scale, this.gui_eval_scale, 0.0f, this.gui_eval_alpha);
			TitleSceneControl.drawTexture(GetEvalRankTexture(), eval_texture_x,	eval_texture_y, this.gui_eval_scale, this.gui_eval_scale, 0.0f, this.gui_eval_alpha);
		}
		
		// 최종 평가 표시.
		if(this.scene_control.step >= SceneControl.STEP.RESULT_TOTAL) {
		
			TitleSceneControl.drawTexture(GetTotalRankTexture(), total_texture_x,	total_texture_y, this.gui_eval_scale, this.gui_eval_scale, 0.0f, this.gui_eval_alpha);
		}

		// 『되돌아가기』문자.
		if(this.scene_control.step >= SceneControl.STEP.GAME_OVER) {

			TitleSceneControl.drawTexture(this.scene_control.ReturnButtonTexture, return_texture_x, return_texture_y);
		}
		
		// ---------------------------------------------------------------- //
		// 디버그 용
#if fasle 
		SceneControl	scene = this.scene_control;

		GUI.color  = Color.white; 
		GUI.matrix = Matrix4x4.identity;

		float	x = 100;
		float	y = 100;

		float	dy = 16;

		GUI.Label(new Rect(x, y, 100, 100), scene.attack_time.ToString());
		y += dy;

		GUI.Label(new Rect(x, y, 100, 100), scene.evaluation.ToString());
		y += dy;

		if(this.scene_control.level_control.is_random) {

			GUI.Label(new Rect(x, y, 150, 100), "RANDOM(" + scene.level_control.group_type_next.ToString() + ")");

		} else {

			GUI.Label(new Rect(x, y, 150, 100), scene.level_control.group_type_next.ToString());
		}
		//this.scene_control.GetEvaluationTexture();
		y += dy;

		//GUI.Label(new Rect(x, y, 100, 100), this.game_timer.ToString());
		//y += 20;

		//

		SceneControl.IS_AUTO_ATTACK = GUI.Toggle(new Rect(x, y, 100, 20), SceneControl.IS_AUTO_ATTACK, "auto");
		y += 50;
		/*
		if(GUI.Toggle(new Rect(x, y, 100, 100), this.evaluation_auto_attack == EVALUATION.GREAT, "great")) {

			this.evaluation_auto_attack = EVALUATION.GREAT;
		}
		y += 20;

		if(GUI.Toggle(new Rect(x, y, 100, 100), this.evaluation_auto_attack == EVALUATION.GOOD, "good")) {

			this.evaluation_auto_attack = EVALUATION.GOOD;
		}
		y += 20;*/

		scene.evaluation_auto_attack = (SceneControl.EVALUATION)GUI.Toolbar(new Rect(x, y, 200, 20), (int)scene.evaluation_auto_attack, SceneControl.evaluation_str);
		y += dy;

		// リザルト.

		x = 300;
		y = 100;

		GUI.Label(new Rect(x, y, 100, 100), scene.result.oni_defeat_num.ToString());
		y += dy;

		for(int i = 0;i < (int)SceneControl.EVALUATION.NUM;i++) {

			GUI.Label(new Rect(x, y, 100, 100), ((SceneControl.EVALUATION)i).ToString() + " " + scene.result.eval_count[i].ToString());
			y += dy;
		}

		GUI.Label(new Rect(x, y, 100, 100), "rank " + scene.result.rank.ToString());
		y += dy;

		if(0 <= (int)scene.evaluation_auto_attack && (int)scene.evaluation_auto_attack <= 2) {

			scene.result.rank = (int)scene.evaluation_auto_attack;
		}
#endif

	}
	
	// 공격한 수에 대한 평가 텍스처 처리.
	private Texture GetDefeatRankTexture()
	{
		return GetResultMiniTexture( this.scene_control.result_control.getDefeatRank() );
	}
	
	// 공격에 대한 평가 텍스처 처리.
	private Texture GetEvalRankTexture()
	{
		return GetResultMiniTexture( this.scene_control.result_control.getEvaluationRank() );
	}
	
	private Texture GetResultMiniTexture( int idx )
	{
		Texture[]	texList = { result_mini_bad_texture, result_mini_nomal_texture, result_mini_good_texture, result_mini_excellent_texture };
		return texList[idx];
	}
	
	// 최종 스코어 텍스처 처리.
	public Texture GetTotalRankTexture()
	{
		Texture[]	texList = { result_bad_texture, result_nomal_texture, result_good_texture, result_excellent_texture };
		
		return texList[this.scene_control.result_control.getTotalRank()];
	}
	
	// 평가 문자의 애니메이션.
	public void	updateEval(float time)
	{
		float	zoom_in_time = GUIControl.EVAL_ZOOM_TIME;
		float	rate;

		if(time < zoom_in_time) {

			rate = time/zoom_in_time;
			rate = Mathf.Pow(rate, 2.5f);
			this.gui_eval_scale = Mathf.Lerp(1.5f, 1.0f, rate);

		} else {

			this.gui_eval_scale = 1.0f;
		}

		if(time < zoom_in_time) {

			rate = time/zoom_in_time;
			rate = Mathf.Pow(rate, 2.5f);
			this.gui_eval_alpha = Mathf.Lerp(0.0f, 1.0f, rate);

		} else {

			this.gui_eval_alpha = 1.0f;
		}
	}

}
