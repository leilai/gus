
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>게임 내 이벤트 관리 클래스</summary>
class EventManager : MonoBehaviour
{
	//==============================================================================================
	// 내부 데이터 형태

	/// <summary>이벤트 실행상태</summary>
	private enum STEP
	{
		NONE = -1,
		LOAD_SCRIPT = 0,  // 스크립트 파일을 로딩한다. 
		WAIT_TRIGGER,     // 이벤트 발생대기
		START_EVENT,      // 이벤트 시작
		EXECUTE_EVENT,    // 이벤트 실행
		NUM
	}


	//==============================================================================================
	// MonoBehaviour 관련 메소드 ・멤버 변수

	/// <summary>게임 시작시에 로딩하는 스크립트 파일명</summary>
	public string[] m_firstScriptFiles = new string[ 0 ];

	/// <summary>스타트업 메소드</summary>
	private void Start()
	{
		// 인스펙터로 스크립트를 지정하지 않은 경우에는 타이틀 화면에서 선택한 것으로 설정
		if ( m_firstScriptFiles.Length == 0 )
		{
			m_firstScriptFiles = GlobalParam.getInstance().getStartScriptFiles();
		}

		// 멤버 초기화
		m_isPrologue = true;
		m_nextStep = STEP.LOAD_SCRIPT;
		m_nextScriptFiles = m_firstScriptFiles;
		m_nextEvaluatingEventIndex = -1;

		// 사운드매니저를 찾는다.  
		m_soundManager = GameObject.Find( "SoundManager" ).GetComponent< SoundManager >();
	}

	/// <summary>매 프레임 갱신 메소드</summary>
	private void Update()
	{
		// ------------------------------------------------------------ //

		if ( m_nextStep == STEP.NONE )
		{
			switch ( m_step )
			{
				case STEP.LOAD_SCRIPT:
				{
					// 스크립트 파일 로딩이 끝나면
					if ( m_hasCreatedEvents )
					{
						m_isExecuting = false;
						m_isPrologue = true;
						m_activeEvent = null;
						m_activeEventIndex = -1;
						m_nextEvaluatingEventIndex = -1;
						m_nextScriptFiles = null;

						m_nextStep = STEP.WAIT_TRIGGER;
					}
				}
				break;

				case STEP.WAIT_TRIGGER:
				{
					if ( m_isPrologue )
					{
						// 프롤로그 이벤트는 무조건 동작
						m_nextStep = STEP.START_EVENT;
					}
					else
					{
						if ( m_contactingObjects.Count > 0 )  // 접촉 오브젝트 존재
						{
							m_isStartedByContact = true;
							m_nextStep = STEP.START_EVENT;
						}
					}
				}
				break;

				case STEP.START_EVENT:
				{
					// 다음에 실행할 수 있는 이벤트를 찾는다. 

					// 배열에서 찾는다. 
					string[] contactingObjectsArray = m_contactingObjects.ToArray();

					// 전회에서 실행된 다음의 이벤트에서 검색을 시작한다.                  
					int i;
					for ( i = m_activeEventIndex + 1; i < m_events.Length; ++i )
					{
						Event ev = m_events[i];

						if ( ev.evaluate( contactingObjectsArray, m_isPrologue ) )
						{
							break;
						}
					}

					if ( i < m_events.Length )
					{
						// 다음 이벤트를 찾았다.

						m_activeEvent      = m_events[ i ];
						m_activeEventIndex = i;
						m_nextStep         = STEP.EXECUTE_EVENT;

						// 이벤트 시작SE (접촉에 따라 시작되는 이벤트의 경우에만)
						if ( m_isStartedByContact )
						{
							m_soundManager.playSE( "rpg_system05" );
						}
					}
					else
					{
						// 다음 이벤트를 찾을 수 없다. 

						m_activeEvent      = null;
						m_activeEventIndex = -1;

						// 대략 실행이 종료되면 프롤로그 종료
						m_isPrologue = false;

						if ( m_nextScriptFiles != null )
						{
							m_nextStep = STEP.LOAD_SCRIPT;
						}
						else
						{
							m_nextStep = STEP.WAIT_TRIGGER;
						}
					}
				}
				break;

				case STEP.EXECUTE_EVENT:
				{
					if ( m_activeEvent.isDone() )
					{
						// 지면의 문장・회화문을 지운다. 
						GetComponent< TextManager >().hide();

						do
						{
							// 계속해서 평가할 이벤트가 지정된 경우 (evaluate-event)
							if ( m_nextEvaluatingEventIndex >= 0 )
							{
								Event ev = m_events[ m_nextEvaluatingEventIndex ];
								if ( ev.evaluate( m_contactingObjects.ToArray(), m_isPrologue ) )
								{
									m_activeEvent      = ev;
									m_activeEventIndex = m_nextEvaluatingEventIndex;
									m_nextStep         = STEP.EXECUTE_EVENT;
									break;
								}
							}

							if ( !m_activeEvent.doContinue() ) m_activeEventIndex = m_events.Length;

							m_nextStep = STEP.START_EVENT;
						}
						while ( false );

						m_nextEvaluatingEventIndex = -1;
					}
				}
				break;
			}
		}

		// ------------------------------------------------------------ //

		while ( m_nextStep != STEP.NONE )
		{
			m_step     = m_nextStep;
			m_nextStep = STEP.NONE;

			switch ( m_step )
			{
				case STEP.LOAD_SCRIPT:
				{
					m_isExecuting = false;
					m_hasCreatedEvents = false;

                    // Coroutine으로 파일 로딩을 시작한다. 
					StartCoroutine( "createEventsFromFile", m_nextScriptFiles );
				}
				break;

				case STEP.WAIT_TRIGGER:
				{
					m_isExecuting = false;

					// 리스트는 클리어 
					m_contactingObjects.Clear();
				}
				break;

				case STEP.EXECUTE_EVENT:
				{
					m_isExecuting = true;
					m_isStartedByContact = false;
					m_activeEvent.start();
				}
				break;
			}
		}

		// ------------------------------------------------------------ //

		switch ( m_step )
		{
			case STEP.EXECUTE_EVENT:
			{
				if ( m_activeEvent != null )
				{
					m_activeEvent.execute( this );
				}
			}
			break;
		}
	}

	/// <summary>GUI 핸드링 메소드</summary>
	private void OnGUI()
	{
#if UNITY_EDITOR
		if ( m_activeEvent != null )
		{
			GUI.Label( new Rect( 10, 10, 200, 20 ), m_activeEvent.getLineNumber().ToString() );
		}
#endif //UNITY_EDITOR

		switch ( m_step )
		{
			case STEP.EXECUTE_EVENT:
			{
				if ( m_activeEvent != null )
				{
					m_activeEvent.onGUI( this );
				}
			}
			break;
		}
	}


	//==============================================================================================
	// 공개메소드

	/// <summary>접촉 오브젝트를 추가한다. </summary>
	public void addContactingObject( BaseObject baseObject )
	{
		string name = baseObject.name;
		if ( !m_contactingObjects.Contains( name ) )
		{
			m_contactingObjects.Add( name );
		}
	}

	/// <summary>이벤트가 실행중인지를 전송한다. </summary>
	public bool isExecutingEvents()
	{
		return m_isExecuting;
	}

	/// <summary>실행중인 이벤트를 가져온다. </summary>
	public Event getActiveEvent()
	{
		return m_activeEvent;
	}

	/// <summary>이벤트 인덱스를 가져온다. </summary>
	public int getEventIndexByName( string eventName )
	{
		return Array.FindIndex( m_events, x => x.getEventName() == eventName );
	}

	/// <summary>다음에 읽어올 스크립트 파일을 설정한다. </summary>
	public void setNextScriptFiles( string[] fileNames )
	{
		m_nextScriptFiles = fileNames;
	}

	/// <summary>이벤트 종료후에 계소해서 평가할 이벤트의 인덱스를 설정한다.  (evaluate-event)</summary>
	public void setNextEvaluatingEventIndex( int eventIndex )
	{
		m_nextEvaluatingEventIndex = eventIndex;
	}

	/// <summary>이벤트를 강제로 시작한다.  (call-event)</summary>
	public void startEvent( int eventIndex )
	{
		m_activeEvent      = m_events[ eventIndex ];
		m_activeEventIndex = eventIndex;
		m_nextStep         = STEP.EXECUTE_EVENT;
	}

	/// <summary>사운드매니저를 전송한다. </summary>
	public SoundManager getSoundManager()
	{
		return m_soundManager;
	}


	//==============================================================================================
	// 비공개 메소드

	/// <summary>파일에서 이벤트를 생성한다.</summary>
	private IEnumerator createEventsFromFile( string[] fileNames )
	{
#if UNITY_EDITOR
		if ( fileNames.Length > 0 )
		{
#endif //UNITY_EDITOR

			// 모든 파일의 행 데이터를 저장하는 리스트
			List< string > linesOfAllFiles = new List< string >();

			foreach ( string file in fileNames )
			{
                // 파일 데이터를 읽을 Coroutin을 체인
				yield return StartCoroutine( loadFile( file, linesOfAllFiles ) );
			}

            // parser하여 이벤트 배열을 작성한다. 
			ScriptParser parser = new ScriptParser();
			m_events = parser.parseAndCreateEvents( linesOfAllFiles.ToArray() );
			Debug.Log( "Created " + m_events.Length.ToString() + " events." );

#if UNITY_EDITOR
		}
		else
		{
			// イベントを空にしておく
			m_events = new Event[ 0 ];
		}
#endif //UNITY_EDITOR

		// 이벤트 작성 완료
		m_hasCreatedEvents = true;
	}

	/// <summary>파일을 로딩한다. </summary>
	private IEnumerator loadFile( string fileName, List< string > allLines )
	{
		string[] lines;

		if ( Application.isWebPlayer )
		{
			// WebPlayer에서는 File.ReadAllLines() 를 사용할 수 없기 때문에 
			// WWW 오브젝트를 사용하여 이벤트 기술파일을 로딩한다.

			WWW www = new WWW( fileName );
			yield return www;	// 응답이 오면 실행이 재개된다. 

			// 개행으로 구분한다. 
			lines = www.text.Split( '\r', '\n' );
		}
		else
		{
			if ( !File.Exists( fileName ) )
			{
				DebugPrint.setLocate( 10, 10 );
				DebugPrint.print( "File Open Error " + fileName, -1.0f );
			}

			// WebPlayer 이외에는  System.IO.File  클래스를 사용하여 로딩한다. 
			lines = File.ReadAllLines( fileName );
		}

		allLines.AddRange( lines );
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>이벤트 작성완료 플래그</summary>
	private bool m_hasCreatedEvents = false;

	/// <summary>이벤트 정보를 저장할 오브젝트 </summary>
	private Event[] m_events = new Event[ 0 ];

	/// <summary>프롤로그 이벤트의 평가・실행 플래그</summary>
	private bool m_isPrologue = false;

	/// <summary>접촉 오브젝트 리스트 </summary>
	private List< string > m_contactingObjects = new List< string >();

	/// <summary>이벤트를 실행중인가. </summary>
	private bool m_isExecuting = false;

	/// <summary>현재의 상태</summary>
	private STEP m_step = STEP.NONE;

	/// <summary>다음으로 전환되는 상태</summary>
	private STEP m_nextStep = STEP.NONE;

	/// <summary>실행중인 이벤트</summary>
	private Event m_activeEvent = null;

	/// <summary>실행중인 이벤트의 인덱스</summary>
	private int m_activeEventIndex = -1;

	/// <summary>다음에 로딩할 스크립트 파일</summary>
	private string[] m_nextScriptFiles = null;

	/// <summary>접촉에 따라 시작되는 이벤트인가</summary>
	private bool m_isStartedByContact = false;

	/// <summary>이벤트 종료후에 계속해서 평가할 이벤트 인덱스 (evaluate-event)</summary>
	private int m_nextEvaluatingEventIndex = -1;

	/// <summary>사운드 매니저 오브젝트</summary>
	private SoundManager m_soundManager = null;
}
