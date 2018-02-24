
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary> 게임 관련「오브젝트」의 기본 단위</summary>
class BaseObject : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 관련 멤버변수・메소드  

	/// <summary>이벤트 매니저 오브젝트</summary>
	public EventManager m_eventManager;

	/// <summary>회화문의 배경색</summary>
	/// 회화문을 표시하지 않는 오브젝트의 경우에는 설정 불필요
	public Color m_dialogBackground = new Color32( 0, 128, 255, 160 );

	/// <summary>y 좌표의 상위값</summary>
	/// 회화문의 말풍선이나 DraggableObject의 매직핸드 등의 표시 위치 관련 조정에 사용되는 값
	public float m_yTop = 0.0f;

	/// <summary>y 좌표의 하위값</summary>
    /// 회화문의 말풍선이나 DraggableObject의 매직핸드 등의 표시 위치 관련 조정에 사용되는 값
	public float m_yBottom = 0.0f;

#if UNITY_EDITOR
	/// <summary>インスペクタでゲーム内変数の内容を確認するためのメンバ変数</summary>
	public string[] m_debug_variables;
#endif //UNITY_EDITOR

	/// <summary>스타트업 메소드</summary>
	private void Awake()
	{
        // Terrain LayerIndex를 검색
		m_terrainLayerIndex = LayerMask.NameToLayer( "Terrain" );
	}

	/// <summary>매 프레임 갱신 메소드</summary>
	private void Update()
	{
		if ( !m_isLandingPrevious && m_isLanding )  // 착지한 순간
		{
			foreach ( GameObject co in m_contactingObjects )
			{
				BaseObject bo = co.GetComponent< BaseObject >();
				if ( bo == null ) continue;

				// 이벤트 매니저에게 접촉한 오브젝트의 정보를 전송
				m_eventManager.addContactingObject( bo );
			}

			// 이벤트 매니저에게 자신의 정보를 전송
			m_eventManager.addContactingObject( this );
		}

		m_contactingObjects.Clear();
		m_isLandingPrevious = m_isLanding;
	}

	/// <summary>오브젝트와의 접촉</summary>
	private void OnTriggerStay( Collider collider )
	{
		// 접촉해 온 상태를 기억해둔다.
		m_contactingObjects.Add( collider.gameObject );
	}

	/// <summary>오브젝트와의 충돌</summary>
	private void OnCollisionEnter( Collision collision )
	{
		if ( collision.gameObject.layer == m_terrainLayerIndex )
		{
			m_isLanding = true;
		}
	}


	//==============================================================================================
	// 공개 메소드

	/// <summary> 게임 내 변수를 취득한다.</summary>
	/// 지정된 이름의 게임 내 변수가 존재하지 않는 경우에는 null을 전송한다.
	public string getVariable( string name )
	{
		// 사전에서 해당되는 이름의 요소를 검색한다.
		string value;
		bool hasData = m_variables.TryGetValue( name, out value );

		return hasData ? value : null;
	}

	/// <summary>게임 내 변수를 설정한다.</summary>
	public void setVariable( string name, string value )
	{
		if ( m_variables.ContainsKey( name ) )
		{
			// 이미 같은 이름의 변수가 존재한다 →변경
			m_variables[ name ] = value;
		}
		else
		{
			// 같은 이름의 변수가 존재하지 않는다. →새롭게 추가한다.
            m_variables.Add( name, value );
		}

#if UNITY_EDITOR
		// エディタで動作させているときはインスペクタ表示を更新する
		m_debug_variables = new string[ m_variables.Count ];

		int i = 0;
		foreach ( KeyValuePair< string, string > pair in m_variables )
		{
			m_debug_variables[ i++ ] = pair.Key + " = " + pair.Value;
		}
#endif //UNITY_EDITOR
	}

	/// <summary>게임 내 변수를 클리어한다.</summary>
	public bool clearVariable( string name )
	{
		return m_variables.Remove( name );
	}

	/// <summary>모든 게임 내 변수를 클리어한다.</summary>
	public void clearAllVariables( bool alsoGlobal )
	{
		if ( alsoGlobal )
		{
			m_variables.Clear();
		}
		else
		{
            // globalScopePrefix 를 갖지 않는 키를 검출하여 삭제
			IEnumerable< string > localKeys = from key in m_variables.Keys
			                                  where key.IndexOf( m_globalScopePrefix ) != 0
			                                  select key;
			foreach ( string key in localKeys )
			{
				clearVariable( key );
			}
		}
	}

	/// <summary>회화문의 배경색을 전송한다.</summary>
	public Color getDialogBackgroundColor()
	{
		return m_dialogBackground;
	}

	/// <summary>y 좌표의 상위값을 전송한다.</summary>
	public float getYTop()
	{
		return m_yTop;
	}

	/// <summary>y 좌표의 하위값을 전송한다.</summary>
	public float getYBottom()
	{
		return m_yBottom;
	}

	/// <summary>액터에서 오는 메세지를 매 프레임에 처리한다.</summary>
	/// 파생 클래스로 메소드를 오버라이드하여 기술한다.
	/// 반환값은 현재 메세지를 다음 프레임에서도 처리할 것인가.
	public virtual bool updateMessage( string message, string[] parameters )
	{
		// 디폴트에서는 아무 처리도 하지 않고 즉시 종료
		return false;
	}


	//==============================================================================================
	// 비공개 멤버 변수

    /// <summary>Terrain 이 되는 오브젝트의 LayerIndex</summary>
	protected int m_terrainLayerIndex;

	/// <summary>착지하고 있는가</summary>
	protected bool m_isLanding = true;

	/// <summary>앞 프레임에서 착지하는가</summary>
	private bool m_isLandingPrevious = true;

	/// <summary>접촉하는 오브젝트</summary>
	private List< GameObject > m_contactingObjects = new List< GameObject >();

	/// <summary>게임 내 변수를 저장하기 위한 사전</summary>
	private Dictionary< string, string > m_variables = new Dictionary< string, string >();

    /// <summary>globalScopePrefix의 게임 내 변수를 표시하는 Prefix</summary>
	private const string m_globalScopePrefix = "_global_";
}
