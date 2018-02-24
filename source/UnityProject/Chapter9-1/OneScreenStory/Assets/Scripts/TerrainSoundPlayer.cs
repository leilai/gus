
using UnityEngine;


/// <summary>Terrain 과 DraggableObject가 충돌했을 때의 사운드를 재생하는 클래스</summary>
class TerrainSoundPlayer : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 관련 멤버 변수・메소드

	/// <summary>이벤트 매니저 오브젝트</summary>
	public EventManager m_manager;

	/// <summary>수면 효과</summary>
	public ParticleSystem m_waterEffect = null;

	/// <summary>오브젝트와의 충돌</summary>
	private void OnCollisionEnter( Collision collision )
	{
		if ( m_manager.isExecutingEvents() ) return;  // 이벤트 실행중에는 재생하지 않는다. 

		DraggableObject draggable = collision.gameObject.GetComponent< DraggableObject >();
		if ( draggable != null && audio != null )
		{
			audio.Play();
		}

		// 수면효과는 여기에서 재생
		if( m_waterEffect != null )
		{
			Vector3 position = draggable.transform.position;

			// 수면과 같은 높이가 되도록
			position.y = 70.0f;
			// 물기둥이 캐릭터에 가려지지 않도록 조금 앞으로 나오게 한다. 
			position.z -= 70.0f;

			// 재생
			m_waterEffect.transform.position = position;
			m_waterEffect.Play();
		}
	}
}
