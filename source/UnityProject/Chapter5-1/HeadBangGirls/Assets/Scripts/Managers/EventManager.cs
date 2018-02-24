using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//스테이 연출 관련 이벤트 관리
public class EventManager : MonoBehaviour {
	// Use this for initialization
	void Start(){
		m_musicManager=GameObject.Find("MusicManager").GetComponent<MusicManager>();
	}
	public void BeginEventSequence(){
		m_seekUnit.SetSequence(m_musicManager.currentSongInfo.stagingDirectionSequence);
	}
	public void Seek(float beatCount){
		m_seekUnit.Seek( beatCount );
		m_previousIndex=m_seekUnit.nextIndex;
		//탐색시에는 현재 실행중인 리스트 클리어
		for ( LinkedListNode<StagingDirection> it = m_activeEvents.First; it != null; it = it.Next) {
			it.Value.OnEnd();
			m_activeEvents.Remove(it);
		}
	}
	void Update () {

		SongInfo	song = m_musicManager.currentSongInfo;

		if( m_musicManager.IsPlaying() )
		{
			//앞 프레임에서 현재 프레임 사이에 히트(명중)한 스테이지 연출 취득

			m_previousIndex = m_seekUnit.nextIndex;

			m_seekUnit.ProceedTime(m_musicManager.beatCount - m_musicManager.previousBeatCount);

			// 「직전의 탐색 위치」와 「갱신 후의 탐색 위치」의 사이에 존재하는 이벤트 실행 시작.
			for(int i = m_previousIndex;i < m_seekUnit.nextIndex;i++){

				// 이벤트 데이터를 복사한다.
				StagingDirection clone = song.stagingDirectionSequence[i].GetClone() as StagingDirection;

				clone.OnBegin();

				// 「샐행 중인 이벤트 리스트」에 추가.
				m_activeEvents.AddLast(clone);
			}
		}

		// 「실행 중인 이벤트」 실행.
		for ( LinkedListNode<StagingDirection> it = m_activeEvents.First; it != null; it = it.Next) {

			StagingDirection	activeEvent = it.Value;

			activeEvent.Update();

			// 실행이 종료되었다？.
			if(activeEvent.IsFinished()) {

				activeEvent.OnEnd();

				// 「실행 중인 이벤트 리스트」에서 삭제한다.
				m_activeEvents.Remove(it);
			}
		}
	}

	//private variables

	MusicManager m_musicManager;

	// 탐색 유니트.
	SequenceSeeker<StagingDirection> m_seekUnit
		= new SequenceSeeker<StagingDirection>();

	// 실행 중인 이벤트.
	LinkedList<StagingDirection> m_activeEvents
		= new LinkedList<StagingDirection>();

	int		m_previousIndex=0;			// 직전의 탐색 위치.
}

