#pragma strict

var audio_clip_engine : AudioClip = null;
var skidSound : AudioClip = null;

private var skidAudio : AudioSource = null;

function Awake () {

	audio.clip = audio_clip_engine;
	audio.loop = true;
	audio.Play();

	skidAudio = gameObject.AddComponent(AudioSource);
	skidAudio.loop = true;
	skidAudio.playOnAwake = true;
	skidAudio.clip = skidSound;
	skidAudio.volume = 1.0;
	skidAudio.Play();
	
}

function Update () {

	engine_sound_control();
}
// エンジン音.
function engine_sound_control()
{

	// スピードの応じてピッチを上げる.

	var	rate:float;
	var	pitch:float;

	var		speed0:float = 0.0f;
	var		speed1:float = 60.0f;

	var		pitch0:float = 1.0f;
	var		pitch1:float = 2.0f;

	rate = Mathf.InverseLerp(speed0, speed1, this.rigidbody.velocity.magnitude);
	rate = Mathf.Clamp01(rate);

	pitch = Mathf.Lerp(pitch0, pitch1, rate);

	this.audio.pitch = pitch;
}

function Skid(play : boolean, volumeFactor : float)
{
return;
	if(!skidAudio)
		return;
	if(play)
	{
Debug.Log("Skid");
		skidAudio.volume = Mathf.Clamp01(volumeFactor + 0.3);
	}
	else
		skidAudio.volume = 0.0;
}
