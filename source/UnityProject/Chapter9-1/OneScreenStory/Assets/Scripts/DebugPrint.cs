
using System.Collections.Generic;
using UnityEngine;


/// <summary> 디버그 텍스트 표시 클래스</summary>
class DebugPrint : MonoBehaviour
{
	//==============================================================================================
    // 내부 데이터 형태

	/// <summary>텍스트 정보 표시</summary>
	private struct TextItem
	{
		public int x;
		public int y;
		public string text;
		public float lifetime;
	}


	//==============================================================================================
	// MonoBehaviour 관련 멤버 변수・메소드

	/// <summary>스타트업 메소드</summary>
	private void Start()
	{
		// 버퍼를 클리어
		clear();
	}

	/// <summary>GUI 핸드링메소드</summary>
	private void OnGUI()
	{
		// 버퍼에 저장된 텍스트를 표시한다.
		foreach ( TextItem item in m_textItems )
		{
			int x = item.x * CHARA_W;
			int y = item.y * CHARA_H;

			GUI.Label( new Rect( x, y, item.text.Length * CHARA_W, CHARA_H ), item.text );
		}

		// 버퍼를 클리어
		if ( UnityEngine.Event.current.type == EventType.Repaint )
		{
			clear();
		}
	}


	//==============================================================================================
	// 비공개 메소드

	/// <summary>버퍼를 클리어한다.</summary>
	private void clear()
	{
		m_locateX = m_locateY = 0;

		for ( int i = 0; i < m_textItems.Count; ++i )
		{
			TextItem item = m_textItems[ i ];

			if ( item.lifetime >= 0.0f )
			{
				item.lifetime -= Time.deltaTime;
				m_textItems[ i ] = item;  // 다시 쓰기

				if ( item.lifetime <= 0.0f )
				{
					m_textItems.Remove( m_textItems[ i ] );
				}
			}
		}
	}

	/// <summary>표시 위치를 설정한다.</summary>
	private void setLocatePrivate( int x, int y )
	{
		m_locateX = x;
		m_locateY = y;
	}

	/// <summary>텍스트를 추가한다. </summary>
	private void addText( string text, float lifetime = 0.0f )
	{
		TextItem item;
		item.x        = m_locateX;
		item.y        = m_locateY++;
		item.text     = text;
		item.lifetime = lifetime;

		m_textItems.Add( item );
	}


	//==============================================================================================
	// 비공개 멤버 변경

	/// <summary>가로 표시위치</summary>
	private int m_locateX;

	/// <summary>세로 표시위치</summary>
	private int m_locateY;

	/// <summary>텍스트 표시 정보 리스트</summary>
	private List< TextItem > m_textItems = new List< TextItem >();

	/// <summary>가로 표시위치의 단위 픽셀</summary>
	private const int CHARA_W = 20;

	/// <summary>세로 표시위치의 단위 픽셀</summary>
	private const int CHARA_H = 20;

	/// <summary>Singleton 인스턴스</summary>
	private static DebugPrint m_instance = null;


	//==============================================================================================
	// 정적 메소드

	/// <summary>이 클래스의 인스턴스를 취득한다. (Singleton)</summary>
	public static DebugPrint getInstance()
	{
		if ( m_instance == null )
		{
			// 클래스를 어태치할 오브젝트를 작성한다. 
			GameObject go = new GameObject( "DebugPrint" );
			// アタッチ
			m_instance = go.AddComponent< DebugPrint >();

			// 씬을 교체하더라도 오브젝트가 사라지지 않도록 한다.
			DontDestroyOnLoad( go );
		}

		return m_instance;
	}

	/// <summary>텍스트를 표시한다. </summary>
	public static void print( string text, float lifetime = 0.0f )
	{
		DebugPrint.getInstance().addText( text, lifetime );
	}

	/// <summary>표시위치를 설정한다. </summary>
	public static void setLocate( int x, int y )
	{
		DebugPrint.getInstance().setLocatePrivate( x, y );
	}
}
