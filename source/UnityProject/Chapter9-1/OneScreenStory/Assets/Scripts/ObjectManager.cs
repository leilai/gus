
using System.Collections.Generic;
using UnityEngine;


/// <summary>오브젝트 관리 클래스</summary>
class ObjectManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 관련 멤버 변수・메소드

	/// <summary> 게임 시작시에 비액티브 상태인 오브젝트</summary>
	/// 여기에 등장한 오브젝트를 미리 비액티브로 설정할 필요는 없다.(자동적으로 비액티브화된다.)
	/// 액티비/비액티브의 교체를 하지 않는 오브젝트를 등장시킬 필요는 없다.
	public BaseObject[] m_initialDeactiveObjects;

    /// <summary>액티비/비액티브의 교체시에 표시할 효과</summary>
	public ParticleSystem m_switchingEffect;

	/// <summary>스타트업 메소드</summary>
	private void Start()
	{
		foreach ( BaseObject bo in m_initialDeactiveObjects )
		{
			deactivate( bo );
		}
	}


	//==============================================================================================
	// 공개 메소드

	/// <summary>오브젝트를 검색한다. </summary>
	/// 비액티비인 오브젝트도 검색할 수 있도록 하는 메소드이다. 
    /// public BaseObject find( string name )
	{
		// 먼저 액티브 상태의 오브젝트를 찾아본다. 
		GameObject go = GameObject.Find( name );
		BaseObject bo = ( go != null ) ? go.GetComponent< BaseObject >() : null;

		if ( bo == null )
		{
			// 발견되지 않는 경우에는 비액티브 오브젝트를 찾아본다. 
			if ( !m_deactiveObjects.TryGetValue( name, out bo ) )
			{
				// 발견되지 않았다.
				return null;
			}
		}

		// 발견되었다. 
		return bo;
	}

	/// <summary>오브젝트를 비액티비 상태로 설정한다. </summary>
	/// 반환값은 비액티브 상태로 되었는가
	public bool deactivate( BaseObject baseObject )
	{
		BaseObject boInDictionary;
		if ( m_deactiveObjects.TryGetValue( baseObject.name, out boInDictionary ) )
		{
			// 이미 같은 이름의 오브젝트가 존재한다. 

			if ( baseObject == boInDictionary )
			{
				// 같은 오브젝트
				Debug.LogWarning( "\"" + baseObject.name + "\" has already deactivated." );

				baseObject.gameObject.SetActiveRecursively( false );	// 만일에 대비해 다시 비액티브로 
				return true;
			}
			else
			{
				// 다른 오브젝트   
				Debug.LogError( "There is already a same name object in the dictionary." );
				return false;
			}
		}

		// 비액티브로 설정한다.
		baseObject.gameObject.SetActiveRecursively( false );
		m_deactiveObjects.Add( baseObject.name, baseObject );
		return true;
	}

	/// <summary>오브젝트를 액티비상태로 설정한다. </summary>
	/// 반환값은 액티브 상태로 되었는가
	public bool activate( BaseObject baseObject )
	{
		if ( m_deactiveObjects.ContainsKey( baseObject.name ) )
		{
			// 액티브로 설정한다.
			baseObject.gameObject.SetActiveRecursively( true );
			m_deactiveObjects.Remove( baseObject.name );

			return true;
		}

		Debug.LogWarning( "\"" + baseObject.name + "\" is NOT deactivated." );
		return false;
	}

	/// <summary>액티비/비액티브의 교체시에 표시할 효과를 재생</summary>
	public void playSwitchingEffect( BaseObject baseObject )
	{
		// 캐릭터의 앞에 표시하기 위한 계산
		Vector3 baseObjectCenter = baseObject.transform.position
		                         + 0.5f * ( baseObject.getYTop() + baseObject.getYBottom() ) * Vector3.up;
		Quaternion qt = Quaternion.AngleAxis( Camera.main.transform.eulerAngles.x, Vector3.right );
		Ray ray = new Ray( baseObjectCenter, qt * -Vector3.forward );

		// 재생
		m_switchingEffect.transform.position = ray.GetPoint( 100.0f );
		m_switchingEffect.Play();
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>비액티브 상태인 오브젝트의 사전</summary>
	private Dictionary< string, BaseObject > m_deactiveObjects = new Dictionary< string, BaseObject >();
}
