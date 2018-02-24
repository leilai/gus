
using UnityEngine;


/// <summary>set 커맨드 이벤트 액터</summary>
class EventActorSet : EventActor
{
	//==============================================================================================
    // 공개 메소드

    /// <summary>생성자</summary>
	public EventActorSet( BaseObject baseObject, string name, string value )
	{
		m_object = baseObject;
		m_name   = name;
		m_value  = value;
	}

    /// <summary>액터가 생성될 때에 가장 처음에 실행되는 메소드</summary>
	public override void start( EventManager evman )
	{
		// 개임 내 변수를 설정
		m_object.setVariable( m_name, m_value );
	}

    /// <summary>실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 바로 종료
		return false;
	}


	//==============================================================================================
    // 비공개 멤버 변수

	/// <summary>게임 내 변수를 조작하는 오브젝트</summary>
	private BaseObject m_object;

	/// <summary>게임 내 변수명</summary>
	private string m_name;

	/// <summary>값</summary>
	private string m_value;


	//==============================================================================================
    // 정적 메소드

    /// <summary>이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorSet CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 3 )
		{
            // 지정된 오브젝트를 찾는다. 
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
                // 액터를 생성
				EventActorSet actor = new EventActorSet( bo, parameters[ 1 ], parameters[ 2 ] );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
