using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

//시퀀스를 스캐닝하여 가장 근접한 요소의 인덱스를 취득하는 클래스
public class SequenceSeeker<ElementType>
	where ElementType: MusicalElement
{	//스캐닝할 시퀀스 데이터를 설정
	public void SetSequence( List<ElementType> sequence ){
		m_sequence = sequence;
		m_nextIndex=0;
		m_currentBeatCount=0;
		m_isJustPassElement=false;
	}
	//가장 가까운 다음 요소를 나타내는 인덱스 번호
	public int nextIndex{
			get{return m_nextIndex;}
	}
	//요소의 trigger 위치를 통과하는 경우에 true
	public bool isJustPassElement{
			get{return m_isJustPassElement;}
	}

	//매 프레임의 처리
	public void ProceedTime(float deltaBeatCount){

		// 현재 시각을 지난다.
		m_currentBeatCount += deltaBeatCount;
		// 「탐색 위치를 지났다」순간을 나타내는 플래그를 삭제한다.
		m_isJustPassElement = false;

		int		index = find_next_element(m_nextIndex);

		// 다음 요소가 발견되었다.
		if(index!=m_nextIndex){

			// 탐색 위치를 갱신
			m_nextIndex = index;

			// 「탐색 위치를 지났다」순간을 나타내는 플레그를 설정.
			m_isJustPassElement=true;
		}
	}
	//스캐닝
	public void Seek(float beatCount){

		m_currentBeatCount = beatCount;

		int		index = find_next_element(0);

		m_nextIndex = index;
	}

	// m_currentBeatCount 의 직후에 존재하는 요소를 찾는다.
	//
	private int	find_next_element(int start_index)
	{
		// 『마지막 마커의 시각을 통과하였다.』를 나타내는 값으로 초기화한다.
		int ret = m_sequence.Count;

		for (int i = start_index;i < m_sequence.Count; i++)
		{
			// m_currentBeatCount 보다 뒤에 존재하는 마켜였다＝발견했다.
			if(m_sequence[i].triggerBeatTiming > m_currentBeatCount)
			{
				ret = i;
				break;
			}
		}

		return(ret);
	}

//private variables
	int		m_nextIndex = 0;				//탐색 위치（＝현재 시각에서 보면, 다음에 ㅈ노재하는 요소의 인덱스）.
	float	m_currentBeatCount = 0;			//현재 시각
	bool	m_isJustPassElement = false;	//탐색 위치를 지난 프레임만 true가 된다.

	List<ElementType> m_sequence;			//탐색할 시퀀스 데이터                  
}

