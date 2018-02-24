using UnityEngine;
using System.Collections;

public class StepSoundControl : MonoBehaviour {

	public AudioClip 	stepSE1 = null;
	public AudioClip 	stepSE2 = null;

	public int			step_sound_sel = 0;

	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {
	
		//

		AnimationEvent ev = new AnimationEvent();

		ev.time         = this.animation["M02_nekodash"].clip.length / 2.0f;;
		ev.functionName = "PlayStepSound";

		this.animation["M02_nekodash"].clip.AddEvent(ev);
	}
	
	public void PlayStepSound(AnimationEvent ev)
	{
		if(this.step_sound_sel == 0) {

			this.audio.PlayOneShot(this.stepSE1);

		} else {

			this.audio.PlayOneShot(this.stepSE2);
		}

		this.step_sound_sel = (this.step_sound_sel + 1)%2;
	}


	// Update is called once per frame
	void Update () {
	
	}
}
