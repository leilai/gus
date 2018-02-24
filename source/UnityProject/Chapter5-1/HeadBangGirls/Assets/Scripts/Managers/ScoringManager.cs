using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//플레이어의 액션에서 스코어를 가점/감점 관리
public class ScoringManager : MonoBehaviour {
	public static float timingErrorToleranceGood = 0.22f;			// 타이밍에서 벗어난 정도가 이하라면  Good
    public static float timingErrorTorelanceExcellent = 0.12f;		// 타이밍에서 벗어난 정도가 이하라면 Excellent
	public static float missScore = -1.0f;
	public static float goodScore = 2.0f;
	public static float excellentScore = 4.0f;
	public static float failureScoreRate = 0.3f;// 도중 판정 포인트로 "실패"로 판단되는 득점율(득점/ 이론상의 최고 득점)
    public static float excellentScoreRate = 0.85f;//도중 판정 포인트로 "우수"로 판단되는 득점율(득점/ 이론상의 최고 득점)
	public static float missHeatupRate = -0.08f;
	public static float goodHeatupRate = 0.01f;
	public static float bestHeatupRate = 0.02f;
	public static float temperThreshold = 0.5f;//연출 변화의 유무 등을 나누는 수치
	public bool outScoringLog=true;
	//현재 합계 스코어
	public float score{
		get{ return m_score; }
	}
	private float m_score;

	//고조 상태의 수치화 0.0 - 1.0
	public float temper
	{
		get { return m_temper; }
		set { m_temper = Mathf.Clamp(value, 0, 1); }
	}
	float m_temper = 0;
	//현재 프레임에서의 스코어 변동 합계값
	public float scoreJustAdded{
		get{ return m_additionalScore; }
	}

	//현재 득점율(득점/ 이론상의 최고득점)
	public float scoreRate
	{
		get { return m_scoreRate; }
	}
	private float m_scoreRate = 0;

	//스코어 평가시작
	public void BeginScoringSequence(){
		m_scoringUnitSeeker.SetSequence(m_musicManager.currentSongInfo.onBeatActionSequence);
	}
	// Use this for initialization
    void Start()
    {
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_playerAction = GameObject.Find("PlayerAvator").GetComponent<PlayerAction>();
		m_bandMembers = GameObject.FindGameObjectsWithTag("BandMember");
		m_audiences = GameObject.FindGameObjectsWithTag("Audience");
		m_noteParticles = GameObject.FindGameObjectsWithTag("NoteParticle");
		m_phaseManager = GameObject.Find("PhaseManager").GetComponent<PhaseManager>();
		//GUI오브젝트는 Inactive한 가능성이 존재하므로 Find에서 직접 액세스할 수 없다.
		m_onPlayGUI    = m_phaseManager.guiList[1].GetComponent<OnPlayGUI>();
#if UNITY_EDITOR 
        m_logWriter = new StreamWriter("Assets/PlayLog/scoringLog.csv");
#endif
    }
	public void Seek(float beatCount){
		m_scoringUnitSeeker.Seek( beatCount );
		m_previousHitIndex=-1;
	}
	// 가장 가까운 ActionInfo 인덱스를 확인
	public int	GetNearestPlayerActionInfoIndex(){

		SongInfo	song = m_musicManager.currentSongInfo;
		int 		nearestIndex = 0;

		if(m_scoringUnitSeeker.nextIndex == 0) {

			// 탐색 위치가 선두에 있는 경우, 앞에 마커가 존재하지 않으므로 비교지 않는다.
			nearestIndex = 0;

		} else if(m_scoringUnitSeeker.nextIndex >= song.onBeatActionSequence.Count) {

			// 탐색 위치가 배열 크기보다 큰 경우(마지막 마커의 시각을 초과한 경우)

			nearestIndex = song.onBeatActionSequence.Count - 1;

		} else {

			// 전후 타이밍과의 차이를 비교.

			OnBeatActionInfo	crnt_action = song.onBeatActionSequence[m_scoringUnitSeeker.nextIndex];			// 탐색 위치.
			OnBeatActionInfo	prev_action = song.onBeatActionSequence[m_scoringUnitSeeker.nextIndex - 1];		// 탐색 위치 한 개 앞.

			float				act_timing = m_playerAction.lastActionInfo.triggerBeatTiming;

			if( crnt_action.triggerBeatTiming - act_timing < act_timing - prev_action.triggerBeatTiming) {

				// 탐색 위치（m_scoringUnitSeeker.nextIndex）의 쪽이 가깝다.
				nearestIndex = m_scoringUnitSeeker.nextIndex;

			} else {

				// 탐색 위치 한 개 앞（m_scoringUnitSeeker.nextIndex）의 쪽이 가깝다.
				nearestIndex = m_scoringUnitSeeker.nextIndex - 1;
			}
		}

		return(nearestIndex);
	}

	// Update is called once per frame
	void Update () {

		m_additionalScore = 0;

		float additionalTemper = 0;
		bool hitBefore = false;
		bool hitAfter = false;

		if( m_musicManager.IsPlaying() ){

			float	delta_count = m_musicManager.beatCount - m_musicManager.previousBeatCount;

			m_scoringUnitSeeker.ProceedTime(delta_count);
			// 플레이어가 입력한 타이밍의 직후 또는 직전(가까운 쪽)의 마커의
			// 인덱스를 받는다.
			if(m_playerAction.currentPlayerAction != PlayerActionEnum.None){
				int nearestIndex = GetNearestPlayerActionInfoIndex();

				SongInfo song = m_musicManager.currentSongInfo;

				OnBeatActionInfo marker_act = song.onBeatActionSequence[nearestIndex];
				OnBeatActionInfo player_act = m_playerAction.lastActionInfo;

				m_lastResult.timingError = player_act.triggerBeatTiming - marker_act.triggerBeatTiming;
				m_lastResult.markerIndex = nearestIndex;

				if (nearestIndex == m_previousHitIndex){
					// 한 번 판정이 종료된 마커를 다시 입력하는 경우.
					m_additionalScore = 0;

				} else {

					// 처음에 클릭한 마커.
					// 타이밍 판정을 한다.
					m_additionalScore = CheckScore(nearestIndex, m_lastResult.timingError, out additionalTemper);
				}

				if (m_additionalScore > 0){

					// 입력 성공

					// 같은 마커를 두 번 판정하지 않도록 마지막에 판정된
					// 마커를 기억해 둔다.
					m_previousHitIndex = nearestIndex;

					// 판정에 사용되는 것이
					// ・탐색 위치의 마커(hitAftere)
					// ・탐색 위치의 한 개 앞의 마커(hitBefore)
					// 판정한다.
					//
					if (nearestIndex == m_scoringUnitSeeker.nextIndex)
						hitAfter = true;
					else
						hitBefore = true;

					//성공시의 연출
					OnScoreAdded(nearestIndex);
				} else{

					// 입력 실패(타이밍이 크게 차이날 경우).

					//액션을 했지만 가산점이 없다면 감점
                    m_additionalScore = missScore;

					additionalTemper = missHeatupRate;
				}
				m_score += m_additionalScore;

				temper += additionalTemper;
				m_onPlayGUI.RythmHitEffect(m_previousHitIndex, m_additionalScore);
				// 디버그용 로그 출력.
				DebugWriteLogPrev();
				DebugWriteLogPost(hitBefore, hitAfter);
			}
			if (m_scoringUnitSeeker.nextIndex > 0)
				m_scoreRate = m_score / (m_scoringUnitSeeker.nextIndex * excellentScore);
		}
	}

	// 입력 결과를 판정한다(우수함/ 서투름/ 실패)
	float CheckScore(int actionInfoIndex, float timingError, out float heatup){

		float	score = 0;

		timingError = Mathf.Abs(timingError);

		do {

			// Good 의 범위보다 차이가 큰 경우 → 실패.
			if(timingError >= timingErrorToleranceGood) {

				score  = 0.0f;
				heatup = 0;
				break;
			}
			
			// Good 과 Excellent 사이의 경우 → Good.
			if(timingError >= timingErrorTorelanceExcellent) {

				score  = goodScore;
				heatup = goodHeatupRate;
				break;
			}

			// Excellent 의 범위의 경우 → Excellent.
			score  = excellentScore;
			heatup = bestHeatupRate;

		} while(false);

		return(score);
	}

	// 디버그용 로그 출력.
	private	void	DebugWriteLogPrev()
	{
#if UNITY_EDITOR
		if( m_scoringUnitSeeker.isJustPassElement ){
			if(outScoringLog){
				OnBeatActionInfo onBeatActionInfo
					= m_musicManager.currentSongInfo.onBeatActionSequence[m_scoringUnitSeeker.nextIndex-1];
				m_logWriter.WriteLine(
					onBeatActionInfo.triggerBeatTiming.ToString() + ","
					+ "IdealAction,,"
					+ onBeatActionInfo.playerActionType.ToString()
				);
				m_logWriter.Flush();
			}
		}
#endif
	}
	private void	OnScoreAdded(int nearestIndex){
		SongInfo song = m_musicManager.currentSongInfo;
		if (song.onBeatActionSequence[nearestIndex].playerActionType == PlayerActionEnum.Jump
			&& temper > temperThreshold)
		{
			foreach (GameObject bandMember in m_bandMembers)
			{
				bandMember.GetComponent<BandMember>().Jump();
			}
			foreach (GameObject audience in m_audiences)
			{
				audience.GetComponent<Audience>().Jump();
			}
			foreach (GameObject noteParticle in m_noteParticles)
			{
				noteParticle.GetComponent<ParticleSystem>().Emit(20);
			}
		}
		else if (song.onBeatActionSequence[nearestIndex].playerActionType == PlayerActionEnum.HeadBanging)
		{
			foreach (GameObject bandMember in m_bandMembers)
			{
				bandMember.GetComponent<SimpleSpriteAnimation>().BeginAnimation(1, 1);
			}
		}
	}
	// 디버그용 로그 출력.
	private void	DebugWriteLogPost(bool hitBefore, bool hitAfter)
	{
#if UNITY_EDITOR
		if(outScoringLog){
			string relation="";
			if(hitBefore){
				relation = "HIT ABOVE";
			}
			if(hitAfter){
				relation = "HIT BELOW";
			}
			string scoreTypeString = "MISS";
			if( m_additionalScore>=excellentScore )
				scoreTypeString = "BEST";
			else if( m_additionalScore>=goodScore )
				scoreTypeString = "GOOD";
			m_logWriter.WriteLine(
				m_playerAction.lastActionInfo.triggerBeatTiming.ToString() + ","
				+ " PlayerAction,"
				+ relation + " " + scoreTypeString + ","
				+ m_playerAction.lastActionInfo.playerActionType.ToString() + ","
				+ "Score=" + m_additionalScore
			);
			m_logWriter.Flush();
		}
#endif
	}

	//Private
	SequenceSeeker<OnBeatActionInfo> m_scoringUnitSeeker
		= new SequenceSeeker<OnBeatActionInfo>();
	float			m_additionalScore;
	MusicManager	m_musicManager;
	PlayerAction	m_playerAction;
	OnPlayGUI		m_onPlayGUI;
	int				m_previousHitIndex = -1;
	GameObject[]	m_bandMembers;
	GameObject[]    m_audiences;
	GameObject[]    m_noteParticles;
    TextWriter		m_logWriter;
	PhaseManager m_phaseManager;
	// 플레이어 입력 결과.
	public struct Result {

		public float	timingError;		// 타이밍 차이（마이너스…빠르다　플러스…느리다）
		public int		markerIndex;		// 비교되는 마커의 인덱스
    };

	// 직전의 플레이어 입력의 결과.
	public Result	m_lastResult;
}

