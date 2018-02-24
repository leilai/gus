
using UnityEngine;


/// <summary>play 커맨드 이벤트 액터</summary>
class EventActorPlay : EventActor
{
	//==============================================================================================
    // 공개 메소드

    /// <summary>생성자</summary>
	public EventActorPlay( AudioClip clip, bool isLoop )
	{
		m_clip   = clip;
		m_isLoop = isLoop;
	}

    /// <summary>액터가 생성될 때에 가장 처음에 실행되는 메소드</summary>
	public override void start( EventManager evman )
	{
		// オーディオクリップを再生
		evman.getSoundManager().playSE( m_clip, m_isLoop );
	}

    /// <summary>실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 바로 종료
		return false;
	}


	//==============================================================================================
    // 비공개 멤버 변수

	/// <summary>재생할 오디어 클립</summary>
	private AudioClip m_clip;

	/// <summary>반복 재생을 할 것인가</summary>
	private bool m_isLoop;


	//==============================================================================================
    // 정적 메소드

    /// <summary>이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorPlay CreateInstance( string[] parameters, GameObject manager, Event ev )
	{
		bool      isValid = false;
		bool      isLoop  = false;
		AudioClip clip    = null;

		// 이제부터 작성할 액터의 이름을 설정한다.(에러 메세지용)
		ev.setCurrentActorName( "EventActorPlay" );

		if ( parameters.Length >= 1 )
		{
			isValid = true;

			if ( parameters.Length >= 2 && parameters[ 1 ].ToLower() == "loop" )
			{
				isLoop = true;
			}

			// 사운드 매니저에서 오디오 클립을 취득
			clip = manager.GetComponent< EventManager >().getSoundManager().getAudioClip( parameters[ 0 ] );
			if ( clip == null )
			{
				ev.debugLogError( "Can't find audio clip \"" + parameters[ 0 ] + "\"" );
				isValid = false;
			}
		}

		if ( isValid )
		{
            // 액터를 생성
			EventActorPlay actor = new EventActorPlay( clip, isLoop );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
