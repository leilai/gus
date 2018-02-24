
using UnityEngine;


/// <summary>call-event 커맨드 이벤트 액터 </summary>
class EventActorCallEvent : EventActor
{
	//==============================================================================================
	// 공개 메소드

	/// <summary>생성자</summary>
	public EventActorCallEvent( int eventIndex )
	{
		m_eventIndex = eventIndex;
	}

    /// <summary> 액터가 생성될 때에 가장 처음에 실행되는 메소드</summary>
	public override void start( EventManager evman )
	{
		evman.startEvent( m_eventIndex );
	}

    /// <summary> 실행 종료후에 클릭을 기다릴지 전송す</summary>
	public override bool isWaitClick( EventManager evman )
	{
        // 바로 종료
		return false;
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary> 이벤트 중에 불러올 이벤트의 인덱스 </summary>
    /// // 실제로는 되돌아오지 않기때문에 점프가 된다.
	private int m_eventIndex;


	//==============================================================================================
	// 정적 메소드

	/// <summary> 이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorCallEvent CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			int eventIndex = manager.GetComponent< EventManager >().getEventIndexByName( parameters[ 0 ] );

			// 액터를 생성
			EventActorCallEvent actor = new EventActorCallEvent( eventIndex );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
