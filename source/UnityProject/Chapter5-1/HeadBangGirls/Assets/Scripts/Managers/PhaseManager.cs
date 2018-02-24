using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
//게임 페이스 변화를 관리하는 클래스
public class PhaseManager : MonoBehaviour {
	public string currentPhase{
		get{ return m_currentPhase; }
	}
	public GameObject[] guiList;
	// Use this for initialization
	void Start () {
		m_musicManager   = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager = GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
	}
	
	// Update is called once per frame
	void Update () {
		switch (currentPhase){
		case "Play" :
			if( m_musicManager.IsFinished() ){
				SetPhase("GameOver");
			}
			break;
		}
	}
	public void SetPhase(string nextPhase){
		switch(nextPhase){
		//시작 메뉴  
		case "Startup":
			DeactiveateAllGUI();
			ActivateGUI("StartupMenuGUI");
			break;
		/설명
		case "OnBeginInstruction":
			DeactiveateAllGUI();
			ActivateGUI("InstructionGUI");
			ActivateGUI("OnPlayGUI");
			break;
		//메인 게임
		case "Play":
		{
			DeactiveateAllGUI();
			ActivateGUI("OnPlayGUI");
			//csv에서 곡 데이터를 읽는다.
			TextReader textReader
				= new StringReader(
					System.Text.Encoding.UTF8.GetString((Resources.Load("SongInfo/songInfoCSV") as TextAsset).bytes )
				);
			SongInfo songInfo = new SongInfo();
			SongInfoLoader loader=new SongInfoLoader();
			loader.songInfo=songInfo;
			loader.ReadCSV(textReader);
			m_musicManager.currentSongInfo = songInfo;

			foreach (GameObject audience in GameObject.FindGameObjectsWithTag("Audience"))
			{
				audience.GetComponent<SimpleActionMotor>().isWaveBegin = true;
			}
			//이벤트(스테이지 연출 등) 시작
			GameObject.Find("EventManager").GetComponent<EventManager>().BeginEventSequence();
			//스코어 평가 시작
			m_scoringManager.BeginScoringSequence();
			//리듬 시퀀스 표시 시작
			OnPlayGUI onPlayGUI = GameObject.Find("OnPlayGUI").GetComponent<OnPlayGUI>();
			onPlayGUI.BeginVisualization();
			onPlayGUI.isDevelopmentMode = false;
			//연주시작
			m_musicManager.PlayMusicFromStart();
		}
			break;
		case "DevelopmentMode":
		{
			DeactiveateAllGUI();
			ActivateGUI("DevelopmentModeGUI");
			ActivateGUI("OnPlayGUI");
			//csv에서 곡 데이터를 읽는다.
			TextReader textReader
				= new StringReader(
					System.Text.Encoding.UTF8.GetString((Resources.Load("SongInfo/songInfoCSV") as TextAsset).bytes )
				);
			SongInfo songInfo = new SongInfo();
			SongInfoLoader loader=new SongInfoLoader();
			loader.songInfo=songInfo;
			loader.ReadCSV(textReader);
			m_musicManager.currentSongInfo = songInfo;

			foreach (GameObject audience in GameObject.FindGameObjectsWithTag("Audience"))
			{
				audience.GetComponent<SimpleActionMotor>().isWaveBegin = true;
			}
			//이벤트(스테이지 연출 등) 시작
			GameObject.Find("EventManager").GetComponent<EventManager>().BeginEventSequence();
			//스코어 평가 시작
			m_scoringManager.BeginScoringSequence();
			//리듬 시퀀스 표시 시작
			OnPlayGUI onPlayGUI = GameObject.Find("OnPlayGUI").GetComponent<OnPlayGUI>();
			onPlayGUI.BeginVisualization();
			onPlayGUI.isDevelopmentMode = true;
			//develop모드 전영GUI시퀀스 표시 시작
			GameObject.Find("DevelopmentModeGUI").GetComponent<DevelopmentModeGUI>().BeginVisualization();
			//연주시작
			m_musicManager.PlayMusicFromStart();
		}
			break;
		case "GameOver":
		{
			DeactiveateAllGUI();
			ActivateGUI("ShowResultGUI");
			ShowResultGUI showResult = GameObject.Find("ShowResultGUI").GetComponent<ShowResultGUI>();
			//스코어 의뢰 메세지를 표시
			Debug.Log( m_scoringManager.scoreRate );
			Debug.Log(ScoringManager.failureScoreRate);
			if (m_scoringManager.scoreRate <= ScoringManager.failureScoreRate)
			{
				showResult.comment = showResult.comment_BAD;
				GameObject.Find("Vocalist").GetComponent<BandMember>().BadFeedback();
				
			}
			else if (m_scoringManager.scoreRate >= ScoringManager.excellentScoreRate)
			{
				showResult.comment = showResult.comment_EXCELLENT;
				GameObject.Find("Vocalist").GetComponent<BandMember>().GoodFeedback();
				GameObject.Find("AudienceVoice").GetComponent<AudioSource>().Play();
			}
			else
			{
				showResult.comment = showResult.comment_GOOD;
				GameObject.Find("Vocalist").GetComponent<BandMember>().GoodFeedback();
			}
		}
			break;
		case "Restart":
		{
			Application.LoadLevel("Main");
		}
			break;
		default:
			Debug.LogError("unknown phase: " + nextPhase);
			break;
		}//end of switch

		m_currentPhase = nextPhase;
	}
	private void DeactiveateAllGUI(){
		foreach( GameObject gui in guiList ){
			gui.active = false;
		}
	}
	private void ActivateGUI(string guiName)
	{
		foreach (GameObject gui in guiList)
		{
			if (gui.name == guiName) gui.active = true;
		}
	}
	//private Variables
	MusicManager m_musicManager;
	ScoringManager m_scoringManager;
	string m_currentPhase = "Startup";
}
