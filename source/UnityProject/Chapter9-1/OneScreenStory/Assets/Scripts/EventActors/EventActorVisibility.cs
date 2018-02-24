
using UnityEngine;


/// <summary>hide/show 커맨드 이벤트 액터</summary>
class EventActorVisibility : EventActor
{
	//==============================================================================================
    // 공개 메소드

    /// <summary>생성자</summary>
	public EventActorVisibility( BaseObject baseObject, bool doShow )
	{
		m_object = baseObject;
		m_doShow = doShow;
	}

    /// <summary>액터가 생성될 때에 가장 처음에 실행되는 메소드</summary>
	public override void start( EventManager evman )
	{
		ObjectManager om = evman.GetComponent< ObjectManager >();

		// 교체 시 효과를 재생
		om.playSwitchingEffect( m_object );

		// 표시/ 비표시
		if ( m_doShow )
		{
			om.activate( m_object );
		}
		else
		{
			om.deactivate( m_object );
		}
	}

    /// <summary>실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 바로 종료
		return false;
	}


	//==============================================================================================
    // 비공개 멤버 변수

	/// <summary> 표시・비표시 오브젝트</summary>
	private BaseObject m_object;

	/// <summary> 오브젝트를 표시할 것인가.</summary>
	private bool m_doShow;


	//==============================================================================================
    // 정적 메소드

    /// <summary>이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorVisibility CreateInstance( string[] parameters, bool doShow, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
            //지정된 오브젝트를 찾는다. 
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
                // 액터를 생성
				EventActorVisibility actor = new EventActorVisibility( bo, doShow );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
