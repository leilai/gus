
using System;
using UnityEngine;


/// <summary>이벤트</summary>
class Event
{
	//==============================================================================================
	// 내부 데이터 형태

	/// <summary>액터의 실행상태</summary>
	private enum STEP
	{
		NONE = -1,
		EXEC_ACTOR = 0,  // 액터 실행중 
		WAIT_INPUT,      // 마우스 클릭 대기
		DONE,            // 종료
		NUM
	}


	//==============================================================================================
	// 공개 메소드

	/// <summary>생성자</summary>
	public Event( string[] targets, EventCondition[] conditions, string[][] actions, bool isPrologue, bool doContinue, string name )
	{
		Array.Sort( targets );	// 나중에 비교하기 위해서 사전에 분류해둔다.

		m_targets    = targets;
		m_conditions = conditions;
		m_actions    = actions;
		m_isPrologue = isPrologue;
		m_doContinue = doContinue;
		m_name       = name;
	}

	/// <summary>이벤트를 평가한다.</summary>
	public bool evaluate( string[] contactingObjects, bool isPrologue )
	{
		if ( isPrologue )
		{
			if ( !m_isPrologue ) return false;
		}
		else
		{
			// 발생 대상 오브젝트와 접촉 오브젝트 비교
			Array.Sort( contactingObjects );

			if ( m_targets.Length == contactingObjects.Length )
			{
				for ( int i = 0; i < m_targets.Length; ++i )
				{
					// "*" 이 겨우에는 누구라도 가능
					if ( m_targets[ i ] == "*" ) continue;

					if ( m_targets[ i ] != contactingObjects[ i ] )
					{
						return false;
					}
				}
			}
			else
			{
				return false;
			}
		}

		// 발생 조건 체크
		foreach ( EventCondition ec in m_conditions )
		{
			if ( !ec.evaluate() ) return false;
		}

		return true;
	}

	/// <summary>이벤트의 스타트업 메소드</summary>
	public void start()
	{
		m_step           = STEP.NONE;
		m_nextStep       = STEP.EXEC_ACTOR;
		m_currentActor   = null;
		m_nextActorIndex = 0;
	}

	/// <summary>이벤트의 매 프레임 갱신 메소드</summary>
	public void execute( EventManager evman )
	{
		// ------------------------------------------------------------ //

		switch ( m_step ) {
			case STEP.WAIT_INPUT:
			{
				if ( Input.GetMouseButtonDown( 0 ) )
				{
					m_currentActor = null;
					m_nextStep = STEP.EXEC_ACTOR;
				}
			}
			break;

			case STEP.EXEC_ACTOR:
			{
				if ( m_currentActor.isDone() )
				{
					// 입력대기를 하는가？
					if ( m_currentActor.isWaitClick( evman ) )
					{
						m_nextStep = STEP.WAIT_INPUT;
					}
					else
					{
						// 하지 않는 경우에는 바로 다음 액터로 
						m_nextStep = STEP.EXEC_ACTOR;
					}
				}
			}
			break;
		}

		// ------------------------------------------------------------ //

		while ( m_nextStep != STEP.NONE )
		{
			m_step     = m_nextStep;
			m_nextStep = STEP.NONE;

			switch ( m_step )
			{
				case STEP.EXEC_ACTOR:
				{
					m_currentActor = null;

					// 다음에 실행할 액터까지 스크립트 주사를 진행한다. 
					while ( m_nextActorIndex < m_actions.Length )
					{
						m_currentActor = createActor( evman, m_nextActorIndex );

						++m_nextActorIndex;

						if ( m_currentActor != null )
						{
							break;
						}
					}

					if ( m_currentActor != null )
					{
						m_currentActor.start( evman );
					}
					else
					{
						// 실행할 액터가 없다면 이벤트 종료 
						m_nextStep = STEP.DONE;
					}
				}
				break;
			}
		}

		// ------------------------------------------------------------ //

		switch ( m_step )
		{
			case STEP.EXEC_ACTOR:
			{
				m_currentActor.execute( evman );
			}
			break;
		}
	}

	/// <summary>이벤트의 GUI핸드링 메소드</summary>
	public void onGUI( EventManager evman )
	{
#if UNITY_EDITOR
		// 現在実行中のアクターのテキストファイル中の行番号を表示する
		if ( m_currentActor != null )
		{
			if ( m_actionLineNumbers != null )
			{
				string text = "";
				text += m_actionLineNumbers[ m_nextActorIndex - 1 ];
				text += " :";
				text += m_currentActor.ToString();

				GUI.Label( new Rect( 10, 40, 200, 20 ), text );
			}
		}
#endif

		switch ( m_step )
		{
			case STEP.EXEC_ACTOR:
			{
				if ( m_currentActor != null )
				{
					m_currentActor.onGUI( evman );
				}
			}
			break;
		}
	}

	/// <summary>이벤트가 완료하였는지 전송</summary>
	public bool isDone()
	{
		return m_step == STEP.DONE;
	}

	/// <summary>이벤트의 연속하여 평가・실행이 가능할지 전송</summary>
	public bool doContinue()
	{
		return m_doContinue;
	}

	/// <summary>이벤트의 이름을 취득한다. </summary>
	public string getEventName()
	{
		return m_name;
	}

	/// <summary>다음에 실행되는 액터의 종류를 취득한다. </summary>
	public string getNextKind()
	{
		string kind = "";

		if ( m_nextActorIndex < m_actions.Length )
		{
			kind = m_actions[ m_nextActorIndex ][ 0 ];
			kind = kind.ToLower();
		}

		return kind;
	}

    /// <summary>이벤트가 기술되어 있는 스크립트의 행번호를 취득한다. </summary>
	public int getLineNumber()
	{
		return m_lineNumber;
	}

    /// <summary>이벤트가 기술되어 있는 스크립트의 행번호를 설정한다.  </summary>
	public void setLineNumber( int n )
	{
		m_lineNumber = n;
	}

	/// <summary>이벤트의 각 액션이 기술되어 있는 스크립트의 행번호를 설정한다.</summary>>
	public void setActionLineNumbers( int[] numbers )
	{
		m_actionLineNumbers = numbers;
	}

    /// <summary>현재의 액터의 이름을 설정한다.</summary>
	public void setCurrentActorName( string name )
	{
		m_actorName = name;
	}

    /// <summary>액터가 생성중인 에러를 출력한다.</summary>
	public void debugLogError( string message )
	{
		Debug.LogError( m_actorName + ":" + message + " at " + m_actionLineNumbers[ m_actorIndex ] + "." );
	}

    /// <summary>이벤트 액터를 생성한다.</summary>
	public EventActor createActor( EventManager manager, int index )
	{
		string[] action     = m_actions[ index ];
		string   kind       = action[ 0 ];
		string[] parameters = new string[ action.Length - 1 ];
		Array.Copy( action, 1, parameters, 0, parameters.Length );

		m_actorName  = "";
		m_actorIndex = index;
		EventActor actor = null;

		switch ( kind.ToLower() )
		{
		// [evaluate-event]
		// 이벤트 종료시에 지정한 이벤트를 연속해서 실행한다.
		case "evaluate-event":
			actor = EventActorEvaluateEvent.CreateInstance( parameters, manager.gameObject );
			break;

		// [set]
		// 게임 내 변수에 문자열을 대입한다. 
		case "set":
			actor = EventActorSet.CreateInstance( parameters, manager.gameObject );
			break;

		// [move]
		// 오브젝트를 이동한다. 
		case "move":
			actor = EventActorMove.CreateInstance( parameters, manager.gameObject );
			break;

		// [hide]
		// 지정된 오브젝트를 비표시한다. 
		case "hide":
			actor = EventActorVisibility.CreateInstance( parameters, false, manager.gameObject );
			break;

		// [show]
		// 지정된 오브젝트를 표시한다. 
		case "show":
			actor = EventActorVisibility.CreateInstance( parameters, true, manager.gameObject );
			break;

		// [text]
		// 지면의 문장을 표시한다. 
		case "text":
			actor = EventActorText.CreateInstance( parameters, manager.gameObject );
			break;

		// [dialog]
		// 회화문을 표시한다. 
		case "dialog":
			actor = EventActorDialog.CreateInstance( parameters, manager.gameObject, this );
			break;

		// [choice]
		// 선택지를 표시하여 선택한 것에 따라 게임 내 변수에 값을 대입한다. 
		case "choice":
			actor = EventActorChoice.CreateInstance( parameters, manager.gameObject );
			break;

		// [play]
		// 사운드를 재생한다. 
		case "play":
			actor = EventActorPlay.CreateInstance( parameters, manager.gameObject, this );
			break;

		// [fadeout]
		// 페이드아웃을 실행한다. 
		case "fadeout":
			actor = EventActorFading.CreateInstance( parameters, false, manager.gameObject );
			break;

		// [fadein]
		// 페이드인을 실행한다.
		case "fadein":
			actor = EventActorFading.CreateInstance( parameters, true, manager.gameObject );
			break;

		// [load]
		// 스크립트를 로딩하여 이벤트를 교체한다. 
		case "load":
			// ToDo: load 코멘드가 있는 경우 연속평가를 중지할 것인가?(현재 중지되어 있지 않다.)
			actor = EventActorLoad.CreateInstance( parameters, manager.gameObject );
			break;

		// [call-event]
		// 코맨드가 실행되는 시점에서 다른 이벤트를 실행한다. 
		case "call-event":
			actor = EventActorCallEvent.CreateInstance( parameters, manager.gameObject );
			break;

		// [message]
		// 오브젝트에 메세지를 전송하여 고유의 처리를 실행한다. 
		case "message":
			actor = EventActorMessage.CreateInstance( parameters, manager.gameObject );
			break;

		default:
			Debug.LogError( "Invalid command \"" + kind + "\"" );
			break;
		}

		return actor;
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>이벤트의 발생대상이 되는 오브젝트</summary>
	private string[]         m_targets;

	/// <summary>이벤트 발생조건</summary>
	private EventCondition[] m_conditions;

	/// <summary>이벤트에서 실행할 액션</summary>
	private string[][]       m_actions;

	/// <summary>프롤로그 이벤트 플래그</summary>
	private bool             m_isPrologue;

	/// <summary>이벤트 연속평가 플래그</summary>
	private bool             m_doContinue;

	/// <summary>이벤트명</summary>
	private string           m_name;

	/// <summary>현재 액터의 실행상태</summary>
	private STEP             m_step = STEP.NONE;

	/// <summary>다음에 전환할 액터의 실행상태</summary>
	private STEP             m_nextStep = STEP.EXEC_ACTOR;

	/// <summary>현재 실행중인 액터</summary>
	private EventActor       m_currentActor = null;

	/// <summary>다음에 실행할 액터의 인덱스</summary>
	private int              m_nextActorIndex = 0;

	/// <summary>마지막에 생성을 테스트한 액터명</summary>
	private string           m_actorName = "";

	/// <summary>마지막에 생성을 테스트한 액터의 인덱스</summary>
	private int              m_actorIndex = 0;

	/// <summary>이벤트가 기술된 스크립트의 행번호</summary>
	private int              m_lineNumber = 0;

	/// <summary>이벤트의 각 액션이 기술된 스크립트의 행번호</summary>
	private int[]            m_actionLineNumbers = null;
}
