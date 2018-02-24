
using UnityEngine;


/// <summary>load 커맨드 이벤트 액터</summary>
class EventActorLoad : EventActor
{
	//==============================================================================================
    // 공개 메소드

    /// <summary>생성자</summary>
	public EventActorLoad( string[] fileNames )
	{
		m_fileNames = fileNames;
	}

    /// <summary>액터가 생성될 때에 가장 처음에 실행되는 메소드</summary>
	public override void start( EventManager evman )
	{
		// 이벤트 매니저에 다음 스크립트 파일을 지정한다. 
		// (실제로 로딩되는 것은 일련의 이벤트 실행이 종료된 후)
		evman.setNextScriptFiles( m_fileNames );
	}

    /// <summary>실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 바로 종료
		return false;
	}


	//==============================================================================================
    // 비공개 멤버 변수

	/// <summary> 스크립트 파일명</summary>
	private string[] m_fileNames;


	//==============================================================================================
    // 정적 메소드

    /// <summary>이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorLoad CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
            // 액터를 생성
			EventActorLoad actor = new EventActorLoad( parameters );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
