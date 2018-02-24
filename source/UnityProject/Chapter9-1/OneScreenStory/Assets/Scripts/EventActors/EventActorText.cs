
using System;
using UnityEngine;


/// <summary>text 커맨드 이벤트 액터</summary>
class EventActorText : EventActor
{
	//==============================================================================================
    // 공개 메소드

    /// <summary>생성자</summary>
	public EventActorText( string text )
	{
		m_text = text;
	}

    /// <summary>액터가 생성될 때에 가장 처음에 실행되는 메소드</summary>
	public override void start( EventManager evman )
	{
		// 지면의 문장을 표시
		TextManager tad = evman.gameObject.GetComponent< TextManager >();
		tad.showText( m_text, new Vector2( 320.0f, 50.0f ), 50.0f, 10.0f, 15.0f );
	}

    /// <summary>실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 다음 액터가 선택지 choice인 경우에는 클릭을 기다리지 않는다.
		Event ev = evman.getActiveEvent();
		if ( ev != null && ev.getNextKind() == "choice" )
		{
			return false;
		}

		// 그렇지 않으면 대기
		return true;
	}


	//==============================================================================================
    // 비공개 멤버 변수

	/// <summary>표시할 텍스트</summary>
	private string m_text;


	//==============================================================================================
    // 정적 메소드

    /// <summary>이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorText CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
            //액터를 생성
			EventActorText actor = new EventActorText( String.Join( "\n", parameters ) );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
