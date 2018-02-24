
using UnityEngine;


/// <summary>fadein/fadeout 커맨드 이벤트 액터</summary>
class EventActorFading : EventActor
{
	//==============================================================================================
    //  공개 메소드

    /// <summary>생성자</summary>
	public EventActorFading( FadeInOutManager manager, bool isFadeIn, float duration )
	{
		m_manager  = manager;
		m_isFadeIn = isFadeIn;
		m_duration = duration;
	}

    /// <summary>액터가 생성될 때에 가장 처음에 실행되는 메소드</summary>
	public override void start( EventManager evman )
	{
		if ( m_isFadeIn )
		{
			m_manager.fadeIn( m_duration );
		}
		else
		{
			m_manager.fadeOut( m_duration );
		}
	}

    /// <summary>액터로 실행해야하는 처리를 끝낼지를 전송</summary>
	public override bool isDone()
	{
		return !m_manager.isFading();
	}

    /// <summary>실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick( EventManager evman )
	{
		return false;
	}


	//==============================================================================================
    // 비공개 멤버 변수

	/// <summary> 화면의 암전을 관리하는 오브젝트</summary>
	private FadeInOutManager m_manager;

	/// <summary> 페이드인 할 것인가</summary>
	private bool m_isFadeIn;

	/// <summary> 페이드인/ 페이드아웃에 관련된 초수</summary>
	private float m_duration;


	//==============================================================================================
    // 정적 메소드

    /// <summary>이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorFading CreateInstance( string[] parameters, bool isFadeIn, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			FadeInOutManager fiom = manager.GetComponent< FadeInOutManager >();
			float duration;

			if ( fiom != null && float.TryParse( parameters[ 0 ], out duration ) )
			{
                // 액터를 생성
				EventActorFading actor = new EventActorFading( fiom, isFadeIn, duration );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
