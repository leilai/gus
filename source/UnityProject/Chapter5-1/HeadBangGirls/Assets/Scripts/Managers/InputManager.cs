using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//입력 정보를 받아 플레이어의 동작 등을 결정한다.
public class InputManager : MonoBehaviour {

	void Awake(){
		Application.targetFrameRate = 60;
	}

	// Use this for initialization
	void Start () {
		m_musicManager=GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_playerAction=GameObject.Find("PlayerAvator").GetComponent<PlayerAction>();
		m_scoringManager=GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
	}

	// Update is called once per frame
	void Update () {
		//비트 카운트의 기록 타이밍을 Input의 Update 타이밍에서 실행한다.
		//MusicManager의 Update로 비트 카운트 기록을 실행하면 입력과 비트 카운트가 
		//최대 1프레임분량 늦어진다.
		//연주 중에 화면 클릭으로 플리에어 액션
		if( Input.GetMouseButtonDown(0) && m_musicManager.IsPlaying() ){
			PlayerActionEnum actionType;
			if (m_scoringManager.temper < ScoringManager.temperThreshold){
				actionType=PlayerActionEnum.HeadBanging;
			}
			else{
				actionType
					=m_musicManager.currentSongInfo.onBeatActionSequence[
						m_scoringManager.GetNearestPlayerActionInfoIndex()
					].playerActionType;
			}
			m_playerAction.DoAction(actionType);
		}
	}

	//privaete variables
	MusicManager m_musicManager;
	PlayerAction m_playerAction;
	ScoringManager m_scoringManager;
}
