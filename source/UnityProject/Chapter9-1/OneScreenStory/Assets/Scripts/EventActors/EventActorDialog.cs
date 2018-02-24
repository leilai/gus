
using System;
using UnityEngine;


/// <summary>dialog 커맨드 이벤트 액터</summary>
class EventActorDialog : EventActor
{
	//==============================================================================================
    // 공개 메소드

    /// <summary>생성자</summary>
	public EventActorDialog( BaseObject baseObject, string text )
	{
		m_object = baseObject;
		m_text   = text;
	}

    /// <summary> 액터가 생성될 때에 가장 처음에 실행되는 메소드</summary>
	public override void start( EventManager evman )
	{
		// 회화문을 표시
		TextManager textman = evman.GetComponent< TextManager >();
		textman.showDialog( m_object, m_text, 50.0f, 10.0f, 15.0f );
	}

    /// <summary> 실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 다음 액터가 선택지 choice의 경우에는 클릭을 기다리지 않는다.
		Event ev = evman.getActiveEvent();
		if ( ev != null && ev.getNextKind() == "choice" )
		{
			return false;
		}

		// 그렇지 않으면 대기
        return true;
	}


	//==============================================================================================
    //  비공개 멤버 변수

	/// <summary> 회화문 표시 대상 오브젝트</summary>
	private BaseObject m_object;

	/// <summary> 표시할 텍스트</summary>
	private string m_text;


	//==============================================================================================
    // 정적 메소드

    /// <summary> 이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorDialog CreateInstance( string[] parameters, GameObject manager, Event ev )
	{
		// 이제부터 만들 액터의 이름을 설정한다.(에러 메시지용)
		ev.setCurrentActorName( "EventActorDialog" );

		if ( parameters.Length >= 2 )
		{
			// 지정된 오브젝트를 찾는다.
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				// 텍스트를 분리 
				string[] text = new string[ parameters.Length - 1 ];
				Array.Copy( parameters, 1, text, 0, text.Length );

				// 액터를 생성
				EventActorDialog actor = new EventActorDialog( bo, String.Join( "\n", text ) );
				return actor;
			}
			else
			{
				ev.debugLogError( "Can't find BaseObject(" + parameters[ 0 ] + ")" );
				return null;
			}
		}

		ev.debugLogError( "Out of Parameter" );
		return null;
	}
}
