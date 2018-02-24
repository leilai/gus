
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


/// <summary>스크립트 파일 parser</summary>
class ScriptParser
{
	//==============================================================================================
	// 공개 메소드

    /// <summary>파일을 parser하여 이벤트 배열을 작성한다. </summary>
	public Event[] parseAndCreateEvents( string[] lines )
	{
		bool isInsideOfBlock = false;
		Regex tabSplitter = new Regex( "\\t+" );             // 복수의 탭으로 분할할 수 있도록 한다. 
		List< string > commandLines = new List< string >();  // 이벤트 커맨드행 데이터
		List< int > commandLineNumbers = new List< int >();  // 이벤트 커맨드행 데이터 파일 내의 행번호
		List< Event > events = new List< Event >();

		string eventName = "";
		int    lineCount = 0;
		int    beginLineCount = 0;

		foreach ( string line in lines )
		{
			// 텍스트파일 내의 현재행   
			lineCount++;

			// 커맨드 삭제
			int index = line.IndexOf( ";;" );	// "//" 이나 "--" 는 사용할 가능성이 있으므로 변경한다. 
			string str = index < 0 ? line : line.Substring( 0, index );
			// 전후 스페이스를 무시
			str = str.Trim();

			// 빈 행이 된 경우에는 다음으로 
			if ( str.Length < 1 ) continue;

			// [] 으로 되어 있는 것이 직후의 이벤트 이름
			if ( str.Length >= 3 )
			{
				if ( str[0] == '[' && str[ str.Length - 1 ] == ']' )
				{
					eventName = str.Substring( 1, str.Length - 2);
				}
			}

			switch ( str.ToLower() )
			{
			// 이벤트 블록 시작
			case "begin":
				if ( isInsideOfBlock )
				{
					Debug.LogError( "Unclosed Event ("  + beginLineCount + ")" );
					return new Event[ 0 ];  // begin  중복 에러
				}
				beginLineCount = lineCount;

				isInsideOfBlock = true;
				break;

			// 이벤트 블록 종료 
			case "end":
				// 커맨드행 데이터를 분석
				List< string[] > commands = new List< string[] >();
				foreach ( string cl in commandLines )
				{
					string[] tabSplit = tabSplitter.Split( cl );
					commands.Add( tabSplit );
				}
				// 커맨드행 데이터 클리어
				commandLines.Clear();

				// 이벤트를 작성하여 리스트에 추가
				Event ev = createEvent( commands.ToArray(), eventName, commandLineNumbers.ToArray(), beginLineCount);
				if ( ev != null )
				{
					ev.setLineNumber( beginLineCount );
					events.Add( ev );
				}
				// 이벤트 데이터를 초기화
				commandLineNumbers.Clear();
				eventName = "";

				isInsideOfBlock = false;
				break;

			// 그 외
			default:
				if ( isInsideOfBlock )
				{
					// 이벤트블록 내만 인식
					commandLines.Add( str );
					commandLineNumbers.Add( lineCount );
				}
				break;
			}
		}

		// Begin 이 닫혀 있지 않다. 
		if ( isInsideOfBlock )
		{
			Debug.LogError( "Unclosed Event ("  + beginLineCount + ")" );
		}

		return events.ToArray();
	}


	//==============================================================================================
	// 비공개 메소드

	/// <summary>이벤트의 커맨드 데이터에서 이벤트를 생성한다. </summary>
	private Event createEvent( string[][] commands, string eventName, int[] numbers, int beginLineCount )
	{
		List< string >         targets     = new List< string >();
		List< EventCondition > conditions  = new List< EventCondition >();
		List< string[] >       actions     = new List< string[] >();
		List< int >            lineNumbers = new List< int >();

		DebugManager           debug_manager = GameObject.FindGameObjectWithTag( "DebugManager" ).GetComponent< DebugManager >();

		bool                   isPrologue = false;
		bool                   doContinue = false;

		int                    i = 0;

		foreach ( string[] commandParams in commands )
		{
			switch ( commandParams[ 0 ].ToLower() )
			{
			// 타겟 오브젝트   
			case "target":
				if ( commandParams.Length >= 2 )
				{
					targets.Add( commandParams[ 1 ] );
				}
				else
				{
					Debug.LogError( "Failed to add a target." );
				}
				break;

			// 프롤로그 이벤트
			case "prologue":
				isPrologue = true;
				break;

			// 발생조건
			case "condition":
				if ( commandParams.Length >= 4 )
				{
					// 지정된 오브젝트를 찾는다. 
					// ToDo: 현재의 장치에서는 비표시 오브젝트가 발견되지 않으므로 대책을 검토한다. 
					GameObject go = GameObject.Find( commandParams[ 1 ] );
					BaseObject bo = go != null ? go.GetComponent< BaseObject >() : null;

					if ( bo != null )
					{
						EventCondition ec = new EventCondition( bo, commandParams[ 2 ], commandParams[ 3 ] );
						conditions.Add( ec );

                        // （디버그용）Watch할 조건 변수를 추가한다. 
						debug_manager.addWatchConditionVariable( commandParams[ 1 ], commandParams[ 2 ] );
					}
					else
					{
						Debug.LogError( "Failed to add a condition." );
					}
				}
				else
				{
					Debug.LogError( "Failed to add a condition." );
				}
				break;

			// 연속 평가
			case "continue":
				doContinue = true;
				break;

			// 그 외에는 액션(이 단계에서는 파라미터를 저장함)
			default:
				actions.Add( commandParams );
				lineNumbers.Add( numbers[ i ] );
				break;
			}

			++i;
		}

		if ( isPrologue )
		{
			// 프롤로그 이벤트는 타겟 오브젝트와 관계없기 때문에 클리어해 둔다.                     
			targets.Clear();
		}
		else
		{
			// 프롤로그 이벤트가 아닌 경우에는 타겟 오브젝트가 최저 2개가 아니면 성립되지 않는다.                 
			if ( targets.Count < 2 )
			{
				Debug.LogError( "Failed to create an event." );
				return null;
			}
		}

		if ( actions.Count < 1 )
		{
			// 이벤트 처리는 최저1개가 아니면 무의미함
			Debug.LogError( "Failed to create an event at " + beginLineCount + ".");
			return null;
		}

		Event ev = new Event( targets.ToArray(), conditions.ToArray(), actions.ToArray(), isPrologue, doContinue, eventName );
		ev.setActionLineNumbers( lineNumbers.ToArray() );

		return ev;
	}
}
