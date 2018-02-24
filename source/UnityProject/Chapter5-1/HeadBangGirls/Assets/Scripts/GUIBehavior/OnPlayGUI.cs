using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//게임 플레이 중에 표시할 GUI 작동
public class OnPlayGUI : MonoBehaviour {
	public Texture messageTexture_Best;
	public Texture messageTexture_Good;
	public Texture messageTexture_Miss;
	public Texture headbangingIcon;
	public Texture beatPositionIcon;
	public Texture hitEffectIcon;
	public Texture temperBar;
	public Texture temperBarFrame;

	public static float markerEnterOffset = 2.5f;	// 마커 표시를 시작할 타이밍(몇 비트 후에 액션이 출현하는가)
	public static float markerLeaveOffset =-1.0f;	// 마커 표시를 종료할 타이밍(몇 비트 후에 액션이 출현하는가)

	public static int rythmHitEffectShowFrameDuration = 20;
	public static int messatShowFrameDuration = 40;

	public bool isDevelopmentMode=false;
	public Vector2 		markerOrigin = new Vector2(20.0f, 300.0f);

	public GUISkin	guiSkin;

	public void BeginVisualization(){
		m_musicManager=GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager=GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
		m_seekerBack.SetSequence(m_musicManager.currentSongInfo.onBeatActionSequence);
		m_seekerFront.SetSequence(m_musicManager.currentSongInfo.onBeatActionSequence);
		m_seekerBack.Seek(markerLeaveOffset);
		m_seekerFront.Seek(markerEnterOffset);
	}
    public void RythmHitEffect(int actionInfoIndex, float score)
    {	m_lastInputScore = score;
		m_rythmHitEffectCountDown = rythmHitEffectShowFrameDuration;
		m_messageShowCountDown=messatShowFrameDuration;
		if(score<0){
			m_playerAvator.GetComponent<AudioSource>().clip
				= m_playerAvator.GetComponent<PlayerAction>().headBangingSoundClip_BAD;
			messageTexture = messageTexture_Miss;
		}
		else if(score<=ScoringManager.goodScore){
			m_playerAvator.GetComponent<AudioSource>().clip
				= m_playerAvator.GetComponent<PlayerAction>().headBangingSoundClip_GOOD;
			messageTexture = messageTexture_Good;
		}
		else{
			m_playerAvator.GetComponent<AudioSource>().clip
				= m_playerAvator.GetComponent<PlayerAction>().headBangingSoundClip_GOOD;
			messageTexture = messageTexture_Best;
		}
		m_playerAvator.GetComponent<AudioSource>().Play();
    }

	// Use this for initialization
	void Start(){
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager = GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
		m_playerAvator = GameObject.Find("PlayerAvator");
	}
	public void Seek(float beatCount){
		m_seekerBack.Seek(beatCount + markerLeaveOffset);
		m_seekerFront.Seek(beatCount + markerEnterOffset);
	}
	// Update is called once per frame
	void Update () {
		if(m_musicManager.IsPlaying()){
			m_seekerBack.ProceedTime( m_musicManager.beatCountFromStart - m_musicManager.previousBeatCountFromStart);
			m_seekerFront.ProceedTime( m_musicManager.beatCountFromStart -m_musicManager.previousBeatCountFromStart);
		}
	}
	void OnGUI(){
		//스코어 표시
		GUI.Box(new Rect(15,5,100,30),"");
		GUI.Label(new Rect(20,10,90,20),"Score: " + m_scoringManager.score);
		// 높은 스코어일 경우의 색상
		if (m_scoringManager.temper > ScoringManager.temperThreshold)
		{
			m_blinkColor.g = m_blinkColor.b
				= 0.7f + 0.3f * Mathf.Abs(
					Time.frameCount % Application.targetFrameRate - Application.targetFrameRate / 2
				) / (float)Application.targetFrameRate;
			GUI.color=m_blinkColor;
		}
		//고조 게이지 표시
		Rect heatBarFrameRect=new Rect(180.0f, 20.0f, 100.0f, 20.0f);
		Rect heatBarRect = heatBarFrameRect;
		Rect heatBarLabelRect = heatBarFrameRect;
		heatBarRect.width *= m_scoringManager.temper;
		heatBarLabelRect.y = heatBarFrameRect.y-20;
		GUI.Label(heatBarLabelRect, "Temper");
		GUI.Box( heatBarFrameRect,"" );
		GUI.DrawTextureWithTexCoords( 
			heatBarRect, temperBar, new Rect(0.0f, 0.0f, 1.0f * m_scoringManager.temper, 1.0f)
		);

		GUI.color = Color.white;

		//이와 같은 아이콘과 액션 타이밍의 아이콘이 중복할 때에 입력
		float 	markerSize = ScoringManager.timingErrorToleranceGood * m_pixelsPerBeats;

		Graphics.DrawTexture(
			new Rect(markerOrigin.x - markerSize / 2.0f, markerOrigin.y - markerSize / 2.0f, markerSize, markerSize)
			, beatPositionIcon
		);

		if( m_musicManager.IsPlaying() ){

			SongInfo			song =  m_musicManager.currentSongInfo;

			// 표시를 시작하는 마커.
			int	begin = m_seekerBack.nextIndex;

			// 표시를 종료하는 마커.
			int end   = m_seekerFront.nextIndex;
			float x_offset;

			//액션 타이밍을 가리키는 아이콘을 표시.
			for ( int drawnIndex = begin; drawnIndex < end; drawnIndex++) {
				OnBeatActionInfo	info = song.onBeatActionSequence[drawnIndex];

				float size = ScoringManager.timingErrorToleranceGood * m_pixelsPerBeats;
				//득점이 높을 때에 점프액션의 마커를 늘린다.
                if (m_scoringManager.temper > ScoringManager.temperThreshold && info.playerActionType == PlayerActionEnum.Jump)
				{
					size *= 1.5f;
				}

				// 표시위치까지의 X좌표의 오프셋을 구한다.
				x_offset = info.triggerBeatTiming - m_musicManager.beatCount;

				x_offset *= m_pixelsPerBeats;


				Rect drawRect = new Rect(
					markerOrigin.x - size/2.0f + x_offset,
					markerOrigin.y - size/2.0f ,
					size,
					size
				);
			
				GUI.DrawTexture( drawRect, headbangingIcon );
				GUI.color = Color.white;

				// 텍스트 파일 내의 행 번호를 표시한다.
				if( isDevelopmentMode ){
					GUI.skin = this.guiSkin;
					GUI.Label(new Rect(drawRect.x, drawRect.y - 10.0f, 50.0f, 30.0f), info.line_number.ToString());
					GUI.skin = null;
				}
			}

			// 액션 타이밍의 히트 효과
			if( m_rythmHitEffectCountDown>0  ){
				float scale=2.0f - m_rythmHitEffectCountDown / (float)rythmHitEffectShowFrameDuration;
				if( m_lastInputScore >= ScoringManager.excellentScore){
					scale *= 2.0f;
				}
				else if( m_lastInputScore > ScoringManager.missScore){
					scale *= 1.2f;
				}
				else{
					scale *= 0.5f;
				}
				float baseSize = 32.0f;
				Rect drawRect3 = new Rect(
					markerOrigin.x - baseSize * scale / 2.0f,
					markerOrigin.y - baseSize * scale / 2.0f,
					baseSize * scale,
					baseSize * scale
				);
				Graphics.DrawTexture(drawRect3, hitEffectIcon);
				m_rythmHitEffectCountDown--;
			}
			//메세지를 표시
			if( m_messageShowCountDown > 0 ){
				GUI.color=new Color(1, 1, 1, m_messageShowCountDown/40.0f );
				GUI.DrawTexture(new Rect(20,230,150,50),messageTexture,ScaleMode.ScaleAndCrop, true);
				GUI.color=Color.white;
				m_messageShowCountDown--;
			}
		}
	}
	//private Variables
	float	m_pixelsPerBeats = Screen.width * 1.0f/markerEnterOffset;
	int		m_messageShowCountDown=0;
	int		m_rythmHitEffectCountDown = 0;
	float	m_lastInputScore = 0;
	Color	m_blinkColor = new Color(1,1,1);


	// 시간적으로 앞서 있는 탐색 유닛（표시종료위치)
	SequenceSeeker<OnBeatActionInfo> m_seekerFront = new SequenceSeeker<OnBeatActionInfo>();

	// 시간적으로 뒤쳐져 있는 탐색 유닛(표시 시작 위치)
	SequenceSeeker<OnBeatActionInfo> m_seekerBack = new SequenceSeeker<OnBeatActionInfo>();

	MusicManager	m_musicManager;
	ScoringManager	m_scoringManager;
	GameObject      m_playerAvator;
	Texture 		messageTexture;
}
