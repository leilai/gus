
using System;
using UnityEngine;


/// <summary>choice 커맨드 이벤트 액터</summary>
class EventActorChoice : EventActor
{
	//==============================================================================================
	// 공개 메소드

	/// <summary>생성자</summary>
	public EventActorChoice( BaseObject baseObject, string name, string[] choices )
	{
		m_object  = baseObject;
		m_name    = name;
		m_choices = choices;
	}

    /// <summary> 액터가 파괴될 때까지  GUI의 표시 타이밍에서 실행되는 메소드</summary>
	public override void onGUI( EventManager evman )
	{
        // font와 padding을 설정
		GUIStyle style = new GUIStyle( "button" );  // button はボタンのデフォルトスタイル
		style.font = evman.gameObject.GetComponent< TextManager >().m_text.font;
		style.padding = new RectOffset( 50, 50, 8, 8 );

		// 화면 중심에 버튼을 표시
		GUILayout.BeginArea( new Rect( 0, 0, Screen.width, Screen.height ) );
			GUILayout.FlexibleSpace();	// 위
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();	// 중앙 왼쪽
				GUILayout.BeginVertical();

					for ( int i = 0; i < m_choices.Length; ++i )
					{
						// 선택지 표시
						if ( GUILayout.Button( m_choices[ i ], style ) )
						{
							// 클릭한 선택지의 인덱스를 게임내 변수에 설정하여 종료
							// (처음 선택지는 1이 된다.)
							m_object.setVariable( m_name, ( i + 1 ).ToString() );
							m_isDone = true;
						}
					}

				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();	// 중앙 오른쪽
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();	// 아래
		GUILayout.EndArea();
	}

    /// <summary> 액터로 실행해야하는 처리를 끝낼지를 전송</summary>
	public override bool isDone()
	{
		return m_isDone;
	}

    /// <summary> 실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 선택지에 클릭 대기가 들어있으므로 여기서는 기다리지 않는다.
		return false;
	}


	//==============================================================================================
    // 비공개 멤버 변수

	/// <summary> 게임 내 변수를 조작하는 오브젝트</summary>
	private BaseObject m_object;

	/// <summary> 게임 내 변수명</summary>
	private string m_name;

	/// <summary> 선택지 일람</summary>
	private string[] m_choices;

	/// <summary> 액터 처리를 종료할지</summary>
	private bool m_isDone = false;


	//==============================================================================================
    // 정적 메소드

    /// <summary> 이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorChoice CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 3 )
		{
			// 지정된 오브젝트를 찾는다.
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				string[] choices = new string[ parameters.Length - 2 ];
				Array.Copy( parameters, 2, choices, 0, choices.Length );

				// 액터를 생성
				EventActorChoice actor = new EventActorChoice( bo, parameters[ 1 ], choices );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
