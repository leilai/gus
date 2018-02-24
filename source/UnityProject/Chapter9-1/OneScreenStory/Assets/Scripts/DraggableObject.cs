
using UnityEngine;


/// <summary>マウスによるドラッグが可能なオブジェクト</summary>
class DraggableObject : BaseObject
{
	//==============================================================================================
    // MonoBehaviour 관련 멤버 변수・메소드

	/// <summary>마우스 드래그할 때에 오브젝트를 들어올리는 높이</summary>
	public float m_pickupHeight = 150.0f;

	/// <summary>오브젝트와의 충돌</summary>
	private void OnCollisionEnter( Collision collision )
	{
		if ( !m_isDragging && collision.gameObject.layer == m_terrainLayerIndex )
		{
			m_isLanding = true;
		}
	}


	//==============================================================================================
	// 공개 메소드

	/// <summary>마우스 드래그를 시작한 프레임의 갱신 메소드</summary>
	public void onDragBegin( RaycastHit hit )
	{
		// 중력은 무시한다. 
		rigidbody.useGravity = false;

		// 오브젝트를 들어올린다.(오브젝트 위치를 갱신)
		updateDragPosition();

		// 착지 플래그를 클리어
		m_isLanding = false;

		// 드래그 시작
		m_isDragging = true;
	}

	/// <summary>마우스 드래그 중인 프레임의 갱신 메소드</summary>
	public void onDragUpdate()
	{
		// 오브젝트 위치 갱신
		updateDragPosition();
	}

	/// <summary>마우스 드래그를 종료한 프레임의 갱신 메소드</summary>
	public void onDragEnd()
	{
		// 낙하속도를 초기화(착지하기 전에 드래그할 것을 반복하여 낙하속도가 축적되는 것을 막는다.)
		rigidbody.velocity = Vector3.zero;

		// 중력을 유효화한다. 
        rigidbody.useGravity = true;

		// 드래그 종료
		m_isDragging = false;
	}


	//==============================================================================================
	// 비공개 메소드

	/// <summary>마우스 드래그 중인 오브젝트의 위치를 갱신한다. </summary>
	private void updateDragPosition()
	{
		// 오브젝트의 이동지점
		Vector3 moveTo = Vector3.zero;

		// 마우스 커서 위치에 있는 Terrain의 좌표를 구한다. 
		Vector3 mousePosition = Input.mousePosition;
		Ray rayFromMouse = Camera.main.ScreenPointToRay( mousePosition );
		RaycastHit hitFromMouse;
		if ( mousePosition.x >= 0.0f && mousePosition.x <= Screen.width  &&
		     mousePosition.y >= 0.0f && mousePosition.y <= Screen.height &&
		     Physics.Raycast( rayFromMouse, out hitFromMouse, float.PositiveInfinity, 1 << m_terrainLayerIndex ) )
		{
			// Terrain의 좌표를 기준으로 이동지점을 정한다. 
			moveTo = hitFromMouse.point + m_pickupHeight * Vector3.up;
		}
		else
		{
			// 마우스커서가 화면밖에 있거나 마우스 커서 위치에 Terrain 이 없다.

			// 현재 위치를 유지
			moveTo = transform.position;
		}

		// 수정하여 원활히 들어올린다. 
		moveTo.y = Mathf.Lerp( transform.position.y, moveTo.y, 0.3f );

		transform.position = moveTo;
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>드래그 중인가</summary>
	private bool m_isDragging = false;
}
