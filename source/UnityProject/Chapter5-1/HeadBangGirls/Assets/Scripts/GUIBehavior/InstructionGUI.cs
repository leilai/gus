using UnityEngine;
using System.Collections;
//게임 룰 등의 설명화면 작동.
public class InstructionGUI : MonoBehaviour {
	public string title="title";
	public string instruction="How to Play";
	public string bandMemberLabel="bandMember";
	public string guageLabel="bandMember";
	public string playerAvatorLabel="playerAvator";
	public string actionMarkerLabel="actionMarker";
	public string targetMarkerLabel="targetmarker";
	public GUISkin guiStyle;
	public SimpleSpriteAnimation sampleBandMemberAniamtion;
	public SimpleSpriteAnimation playerAvatorAnimation;
	public Texture actinoMarker;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		//일정 시간마다 캐릭터를 애니메이션화시킨다.
		animationCounter+=Time.deltaTime;
		if( animationCounter > 1.0f){
			sampleBandMemberAniamtion.BeginAnimation(1,1,false);
			playerAvatorAnimation.BeginAnimation(2,1,false);
			animationCounter=0;
		}
		//클릭으로 다음으로 이동한다.
		if( Input.GetMouseButton(0) ){
			GameObject.Find("PhaseManager").GetComponent<PhaseManager>().SetPhase("Play");
		}
	}
	float animationCounter=0;
	void OnGUI(){
		GUI.skin = guiStyle;
		GUI.Label( new Rect( 20, 60, 100, 40 ), bandMemberLabel );
		GUI.Label( new Rect( 150, 40, 180, 40 ), guageLabel );
		GUI.Label( new Rect( 60, 210, 150, 40 ), playerAvatorLabel );
		GUI.Label( new Rect( 5, 260, 210, 80 ), targetMarkerLabel );
		GUI.Label( new Rect( 200, 260, 210, 80 ), actionMarkerLabel );
		GUI.DrawTexture( new Rect( 200, 285, actinoMarker.width, actinoMarker.height ), actinoMarker);
		GUI.Box( new Rect( 20, 370, Screen.width-20, 150 ), instruction );
		
	}
	
}
