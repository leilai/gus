using UnityEngine;
using System.Collections;
//플레이어의 액션의 종류를 나타내는 열거
public enum PlayerActionEnum{
	None,
	HeadBanging,
	Jump
};
//플레이어의 액션
public class PlayerAction : MonoBehaviour {
	public AudioClip headBangingSoundClip_GOOD;
	public AudioClip headBangingSoundClip_BAD;
	//플레이어의 현재의 액션
	public PlayerActionEnum currentPlayerAction{
		get{ return m_currentPlayerAction; }
	}
	//플레이어가 마지막에 취한 액션
	public OnBeatActionInfo lastActionInfo{
		get{ return m_lastActionInfo; }
	}
	// Use this for initialization
	void Start () {
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
	}
	
	// Update is called once per frame
	void Update () {
		m_currentPlayerAction = m_newPlayerAction;
		m_newPlayerAction = PlayerActionEnum.None;
	}
	public void DoAction(PlayerActionEnum actionType){
		m_newPlayerAction = actionType;

		OnBeatActionInfo actionInfo = new OnBeatActionInfo();
		actionInfo.triggerBeatTiming = m_musicManager.beatCountFromStart;
		actionInfo.playerActionType = m_newPlayerAction;
		m_lastActionInfo = actionInfo;

		if(actionType == PlayerActionEnum. HeadBanging){
			gameObject.GetComponent<SimpleSpriteAnimation>().BeginAnimation(2, 1, false);
		}
		else if (actionType == PlayerActionEnum.Jump)
		{	
			gameObject.GetComponent<SimpleActionMotor>().Jump();
			gameObject.GetComponent<SimpleSpriteAnimation>().BeginAnimation(1, 1, false);
		}
	}
	//입력에 대응하는 액션을 실행한다.
	//Private variables
	MusicManager m_musicManager;
	OnBeatActionInfo m_lastActionInfo=new OnBeatActionInfo();
	PlayerActionEnum m_currentPlayerAction;
	PlayerActionEnum m_newPlayerAction;
}
