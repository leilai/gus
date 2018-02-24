
using System;
using UnityEngine;


/// <summary>글로벌 파라미터 관리 클래스</summary>
class GlobalParam : MonoBehaviour
{
	//==============================================================================================
	// 공개 메소드

	/// <summary>게임 시작시에 로딩하는 스크립트파일을 가져온다. </summary>
	public string[] getStartScriptFiles()
	{
		return m_startScripts;
	}

	/// <summary>게임 시작시에 로딩할 스크립트 파일을 설정한다.</summary>
	public void setStartScriptFiles( params string[] files )
	{
		m_startScripts = new string[ files.Length ];
		Array.Copy( files, m_startScripts, files.Length );
	}

	//==============================================================================================
	// 비공개 메소드

	/// <summary>초기화</summary>
	private void create()
	{
		m_startScripts = new string[2];

		// Title 을 거치지 않고 인스펙터로 스크립트가 지정되어 있지 않는 경우에는 
		// 여기에서 지정된 것이 사용된다. 
		//
		m_startScripts[0] = "Events/c00_main.txt";
		m_startScripts[1] = "Events/c00_sub.txt";
	}

	//==============================================================================================
	// 비공개 멤버 변수   

	/// <summary>게임 시작시에 로딩할 스크립트파일</summary>
	private string[] m_startScripts;

	/// <summary>Singleton 인스턴스</summary>
	private static GlobalParam m_instance = null;


	//==============================================================================================
	// 정적 메소드   

	/// <summary>클래스의 인스턴스를 가져온다. (Singleton)</summary>
	public static GlobalParam getInstance()
	{
		if ( m_instance == null )
		{
			// 클래스를 어태치할 오브젝트를 작성한다. 
			GameObject go = new GameObject( "GlobalParam" );
			// 어태치
			m_instance = go.AddComponent< GlobalParam >();

			m_instance.create();

			// 씬을 교체하더라도 오브젝트가 사라지지 않도록 한다. 
			DontDestroyOnLoad( go );
		}

		return m_instance;
	}
}
