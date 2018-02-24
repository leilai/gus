
using UnityEngine;


/// <summary>집 오브젝트 전용 클래스</summary>
class HouseObject : BaseObject
{
	//==============================================================================================
	// 공개 메소드

	/// <summary>애니메이션 재생대상 오브젝트</summary>
	public GameObject m_animatingObject;

	/// <summary>이벤트 액터에서 오는 메세지를 처리한다. </summary>
	public override bool updateMessage( string message, string[] parameters )
	{
		if ( !m_isAnimated )
		{
			switch ( message )
			{
			// 오픈
			case "open":
				m_animatingObject.animation.Play( "M11_house01_door_Open" );
				m_isAnimated = true;
				return true;

			// 기타
			default:
				return false;	// 바로 종료
			}
		}
		else
		{
			if ( m_animatingObject.animation.isPlaying )
			{
				return true;
			}
			else
			{
				// 애니메이션 종료
				m_isAnimated = false;
				return false;
			}
		}
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>애니메이션 중인가</summary>
	private bool m_isAnimated = false;
}
