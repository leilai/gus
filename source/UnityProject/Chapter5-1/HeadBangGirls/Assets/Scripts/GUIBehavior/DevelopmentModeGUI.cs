using UnityEngine;
using System.Collections;
// 탐색 기능부 개발모드의 GUI 작동
public class DevelopmentModeGUI : MonoBehaviour {
	//연주 시작시의 처리
	public void BeginVisualization()
	{
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_actionInfoRegionSeeker.SetSequence(m_musicManager.currentSongInfo.onBeatActionRegionSequence);
		m_actionInfoRegionSeeker.Seek(0);

	}
	public void Seek(float beatCount)
	{
		m_actionInfoRegionSeeker.Seek(beatCount);
	}
	// Use this for initialization
	void Start () {
		m_musicManager=GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager=GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
		m_eventManager=GameObject.Find("EventManager").GetComponent<EventManager>();
		//GUI오브젝트는 Inactive한 가능성이 있기 때문에 Find으로 직접 액세스할 수 없다.
		m_onPlayGUI = GameObject.Find("PhaseManager").GetComponent<PhaseManager>().guiList[1].GetComponent<OnPlayGUI>();
		m_playerAction=GameObject.Find("PlayerAvator").GetComponent<PlayerAction>();
		m_seekSlider.is_now_dragging    = false;
		m_seekSlider.dragging_poisition = 0.0f;
	}

	// Update is called once per frame
	void Update () {
		m_actionInfoRegionSeeker.ProceedTime(
			m_musicManager.beatCountFromStart - m_musicManager.previousBeatCountFromStart
		);

		m_seekSlider.is_button_down = Input.GetMouseButton(0);
	}

	void OnGUI(){

		GUI.Label(new Rect( 5, 100, 150, 40 ),"Current");

        // SeekSliderControl
		SeekSliderControl();

		GUI.TextArea(
			new Rect( 250, 100, 200, 40 ),
			((int)m_musicManager.beatCountFromStart).ToString() + "/" + ((int)m_musicManager.length).ToString()
		);

		// 탐색 중에만 탐색바의 위치를 표시한다.
		if(this.m_seekSlider.is_now_dragging) {

			GUI.Label(new Rect( 252, 120, 200, 40 ), ((int)this.m_seekSlider.dragging_poisition).ToString());
		}

		//
		if( GUI.Button( new Rect( (Screen.width - 150)/2.0f, 350, 150, 40 ), "End" ) ){
			GameObject.Find("PhaseManager").GetComponent<PhaseManager>().SetPhase("Restart");
		}

		// 입력 타이밍이 어느 정도 차이가 나는지를 표시한다.
		GUI.Label(new Rect( 5, 400, 150, 40 ),"Input Gap:" + m_scoringManager.m_lastResult.timingError);

		GUI.Label(
			new Rect( 5, 420, 150, 40 ),
			"Previous Input:"
			+ m_playerAction.lastActionInfo.triggerBeatTiming.ToString());
		GUI.Label(new Rect( 5, 440, 150, 40 ),
			"Nearest(beat):"
			+ m_musicManager.currentSongInfo.onBeatActionSequence[m_scoringManager.m_lastResult.markerIndex].triggerBeatTiming.ToString());
		GUI.Label(
			new Rect( 150, 440, 150, 40 ),
			"Nearest(index):"
			+ m_musicManager.currentSongInfo.onBeatActionSequence[m_scoringManager.m_lastResult.markerIndex].line_number.ToString());
		
		// 관련 파트 명을 표시.
		if( m_musicManager.currentSongInfo.onBeatActionRegionSequence.Count>0 ){
			// 현재 파트 인덱스를 확인
			int currentReginIndex = m_actionInfoRegionSeeker.nextIndex - 1;
			if (currentReginIndex < 0)
				currentReginIndex = 0;
			// 전회 입력시의 파트를 표시.
			if (m_playerAction.currentPlayerAction != PlayerActionEnum.None)
			{	
				previousHitRegionName
					= m_musicManager.currentSongInfo.onBeatActionRegionSequence[currentReginIndex].name;
			}
			GUI.Label(new Rect(150, 420, 250, 40), "region ...:" + previousHitRegionName);
			//현재 파트를 표시.
			GUI.Label(new Rect(5, 460, 150, 40), "Current:" + m_musicManager.beatCountFromStart);
			GUI.Label(new Rect(150, 460, 250, 40), "region ...:" + m_musicManager.currentSongInfo.onBeatActionRegionSequence[currentReginIndex].name);
		}

	}

    // SeekSliderControl
	private void	SeekSliderControl()
	{
		Rect	slider_rect = new Rect( (Screen.width - 100)/2.0f, 100, 130, 40 );

		if(!m_seekSlider.is_now_dragging) {

			float	new_position 
				= GUI.HorizontalSlider( slider_rect, m_musicManager.beatCount, 0, m_musicManager.length );

			// 드래그 시작.
			if(new_position != m_musicManager.beatCount) {

				m_seekSlider.dragging_poisition = new_position;
				m_seekSlider.is_now_dragging = true;
			}


		} else {

			m_seekSlider.dragging_poisition 
				= GUI.HorizontalSlider( slider_rect, m_seekSlider.dragging_poisition, 0, m_musicManager.length );

			// 버튼을 떼었다(드래그 종료)
			if(!m_seekSlider.is_button_down) {

				m_musicManager.Seek( m_seekSlider.dragging_poisition );

				m_eventManager.Seek( m_seekSlider.dragging_poisition );
				m_scoringManager.Seek( m_seekSlider.dragging_poisition );
				m_onPlayGUI.Seek( m_seekSlider.dragging_poisition );

				Seek(m_seekSlider.dragging_poisition);

				// 드래그 종료
				m_seekSlider.is_now_dragging = false;
			}
		}
	}


	SequenceSeeker<SequenceRegion> m_actionInfoRegionSeeker = new SequenceSeeker<SequenceRegion>();
	MusicManager 	m_musicManager;
	ScoringManager	m_scoringManager;
	EventManager	m_eventManager;
	OnPlayGUI		m_onPlayGUI;
	PlayerAction	m_playerAction;
	string	previousHitRegionName = "";

    //  SeekSlider
	private struct SeekSlider {

		public bool		is_now_dragging;		// 드래그 중?.
		public float	dragging_poisition;		// 드래그 위치.
		public bool		is_button_down;			// 마우스 왼쪽 버튼.Input.GetMouseButton(0)의 결과
												// document로
												// Note also that the Input flags are not reset until "Update()", 
												// so its suggested you make all the Input Calls in the Update Loop
												// 라고 존재하므로 만일에 대비함.
	};
	private SeekSlider	m_seekSlider;

}
