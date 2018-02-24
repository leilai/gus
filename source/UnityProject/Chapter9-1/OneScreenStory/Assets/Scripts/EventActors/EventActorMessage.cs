
using System;
using UnityEngine;


/// <summary>message  커맨드 이벤트 액터</summary>
class EventActorMessage : EventActor
{
	//==============================================================================================
    // 공개 메소드

    /// <summary>생성자</summary>
	public EventActorMessage( string message, BaseObject to, string[] parameters )
	{
		m_message    = message;
		m_to         = to;
		m_parameters = parameters;
	}

    /// <summary>액터가 파괴될 때까지 매 프래임 실행되는 메소드</summary>
	public override void execute( EventManager evman )
	{
		if ( !( m_to.updateMessage( m_message, m_parameters ) ) )
		{
			m_isDone = true;
		}
	}

    /// <summary>액터로 실행해야하는 처리를 끝낼지를 전송す</summary>
	public override bool isDone()
	{
		return m_isDone;
	}

    /// <summary>실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick(EventManager evman)
	{
		// 종료 타이밍은 메세지의 수신처가 관리.
		return false;
	}


	//==============================================================================================
    // 비공개 멤버 변수

	/// <summary>메세지의 종류</summary>
	private string m_message;

	/// <summary>메세지 수신처</summary>
	private BaseObject m_to;

	/// <summary>메세지의 파라미터</summary>
	private string[] m_parameters;

	/// <summary>액터 처리를 종료할지</summary>
	private bool m_isDone = false;


	//==============================================================================================
    // 정적 메소드

    /// <summary>이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorMessage CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 2 )
		{
			// 지정된 오브젝트를 찾는다. 
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				string[] messageParams = new string[ parameters.Length - 2 ];
				Array.Copy( parameters, 2, messageParams, 0, messageParams.Length );

                // 액터를 생성
				EventActorMessage actor = new EventActorMessage( parameters[ 1 ], bo, messageParams );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
