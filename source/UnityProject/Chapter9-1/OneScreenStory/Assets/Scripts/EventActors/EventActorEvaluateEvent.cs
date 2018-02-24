
using UnityEngine;


/// <summary>evaluate-event  커맨드 이벤트 액터</summary>
class EventActorEvaluateEvent : EventActor
{
	//==============================================================================================
    // 공개 메소드

    /// <summary>생성자</summary>
	public EventActorEvaluateEvent( int eventIndex )
	{
		m_eventIndex = eventIndex;
	}

    /// <summary>액터가 생성될 때에 가장 처음에 실행되는 메소드</summary>
	public override void start( EventManager evman )
	{
		evman.setNextEvaluatingEventIndex( m_eventIndex );
	}

    /// <summary>실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 바로 종료
		return false;
	}


	//==============================================================================================
    // 비공개 멤버 변수

	/// <summary> 이벤트 종료후에 연속하여 평가할 이벤트의 인덱스</summary>
	private int m_eventIndex;


	//==============================================================================================
    // 정적 메소드

    /// <summary>이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorEvaluateEvent CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			int eventIndex = manager.GetComponent< EventManager >().getEventIndexByName( parameters[ 0 ] );

            // 액터를 생성
			EventActorEvaluateEvent actor = new EventActorEvaluateEvent( eventIndex );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
