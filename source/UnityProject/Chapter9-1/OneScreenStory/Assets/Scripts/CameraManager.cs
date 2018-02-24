
using UnityEngine;


/// <summary>카메라의 위치・회전 각도・평행투영 사이즈를 관리하는 클래스</summary>
class CameraManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 관련 멤버 변수・메소드 

	/// <summary>Terrain 의 좌측</summary>
	public float m_terrainEndLeft;
	/// <summary>Terrain 의 우측</summary>
	public float m_terrainEndRight;
	/// <summary>Terrain 의 앞쪽</summary>
	public float m_terrainEndFront;
	/// <summary>Terrain 의 안쪽</summary>
	public float m_terrainEndBack;
	/// <summary>Terrain 안의 배경 위쪽</summary>
	public float m_backgroundTop;

	/// <summary> 스타트업 메소드</summary>
	private void Start()
	{
		// 초기위치(＝시작 지점에서의 현재위치)를 기억한다.
		m_originalPosition  = m_currentPosition  = transform.position;
		m_originalRotationX = m_currentRotationX = transform.eulerAngles.x;
		m_originalSize      = m_currentSize      = camera.orthographicSize;
	}

	/// <summary> 매 프레임 갱신 메소드</summary>
	private void Update()
	{
		if ( m_isMoving )
		{
			if ( Time.time >= m_endTime )
			{
				// 카메라 이동 시간 경과 후 첫 Update

				// 현재위치＝목표위치로 이동
				transform.position      = m_currentPosition = m_destinationPosition;
				camera.orthographicSize = m_currentSize     = m_destinationSize;
				m_currentRotationX = m_destinationRotationX;
				transform.eulerAngles = new Vector3( m_currentRotationX, transform.eulerAngles.y, transform.eulerAngles.z );

				// 이동 종료
				m_isMoving = false;
			}
			else
			{
				// 진행도(0.0～1.0)
				float ratio = Mathf.InverseLerp( m_beginTime, m_endTime, Time.time );

				// 각도와 투영사이즈는 수정
				transform.eulerAngles = new Vector3( Mathf.Lerp( m_currentRotationX, m_destinationRotationX, ratio ),
				                                     transform.eulerAngles.y, transform.eulerAngles.z );
				camera.orthographicSize = Mathf.Lerp( m_currentSize, m_destinationSize, ratio );

				// 위치는 수정중에 Terrain 쪽이 보이지 않도록 조정
				transform.position = fixPosition( Vector3.Lerp( m_currentPosition, m_destinationPosition, ratio ), transform.eulerAngles.x, camera.orthographicSize );
			}
		}
	}


	//==============================================================================================
    // 공개 메소드

	/// <summary>지정 위치로 이동한다.</summary>
	public void moveTo( Vector3 destinationPosition, float destinationRotationX, float destinationSize, float duration )
	{
		// 좌표조정
		destinationPosition = fixPosition( destinationPosition, destinationRotationX, destinationSize );

		m_destinationPosition  = destinationPosition;
		m_destinationRotationX = destinationRotationX;
		m_destinationSize      = destinationSize;

		m_beginTime = Time.time;
		m_endTime   = m_beginTime + duration;
		m_isMoving  = true;
	}

	/// <summary>초기 위치를 취득한다.</summary>
	public Vector3 getOriginalPosition()
	{
		return m_originalPosition;
	}

	/// <summary>초기 x 축 회전각도를 취득한다.</summary>
	public float getOriginalRotationX()
	{
		return m_originalRotationX;
	}

	/// <summary>초기 평행투영사이즈를 취득한다.</summary>
	public float getOriginalSize()
	{
		return m_originalSize;
	}

	/// <summary>현재 위치를 취득한다.</summary>
	/// 카메라가 이동중인 경우에는 이동을 시작하기 전의 위치
    /// public Vector3 getCurrentPosition()
	{
		return m_currentPosition;
	}

	/// <summary> 현재의 x 축 회전각도를 취득한다.</summary>
	/// 카메라가 이동중인 경우에는 이동을 시작하기 전의 회전각도
	public float getCurrentRotationX()
	{
		return m_currentRotationX;
	}

	/// <summary> 현재의 평행투영 사이즈를 취득한다. </summary>
	/// 카메라가 이동중인 경우에는 이동을 시작하기 전의 사이즈
	public float getCurrentSize()
	{
		return m_currentSize;
	}

	/// <summary>카메라가 이동중인지를 전송한다.</summary>
	public bool isMoving()
	{
		return m_isMoving;
	}


	//==============================================================================================
	// 비공개 메소드

	/// <summary>Terrain  쪽이 보이지 않도록 좌표를 조정한다.</summary>
	private Vector3 fixPosition( Vector3 position, float rotationX, float size )
	{
		Vector3 newPosition = new Vector3( position.x, position.y, position.z );
		float horizontalSize = size * Screen.width / Screen.height;

		// 좌측
		if ( position.x - horizontalSize < m_terrainEndLeft )
		{
			newPosition.x = m_terrainEndLeft + horizontalSize;
		}

		// 우측
		if ( position.x + horizontalSize > m_terrainEndRight )
		{
			newPosition.x = m_terrainEndRight - horizontalSize;
		}

		// 앞쪽
		float terrainZOfBottom = position.z
		                       + position.y / Mathf.Tan( rotationX * Mathf.Deg2Rad )
		                       - size / Mathf.Sin( rotationX * Mathf.Deg2Rad );
		if ( terrainZOfBottom < m_terrainEndFront )
		{
			newPosition.z = position.z + m_terrainEndFront - terrainZOfBottom;
		}

		// 안쪽
		float terrainYOfTop = position.y
		                    - ( m_terrainEndBack - position.z ) * Mathf.Tan( rotationX * Mathf.Deg2Rad )
		                    + size / Mathf.Cos( rotationX * Mathf.Deg2Rad );
		if ( terrainYOfTop > m_backgroundTop )
		{
			newPosition.z = position.z - ( terrainYOfTop - m_backgroundTop ) / Mathf.Tan( rotationX * Mathf.Deg2Rad );
		}

		return newPosition;
	}


	//==============================================================================================
	// 비공개 멥버 변수

	/// <summary>초기위치/summary>
	private Vector3 m_originalPosition;

	/// <summary>초기x 축회전각도/summary>
	private float m_originalRotationX;

	/// <summary>초기평행투영사이즈/summary>
	private float m_originalSize;

	/// <summary>현재위치/summary>
	private Vector3 m_currentPosition;

	/// <summary>현재의x 축회전각도/summary>
	private float m_currentRotationX;

	/// <summary>현재의 평행투영사이즈</summary>
	private float m_currentSize;

	/// <summary>목표위치/summary>
	private Vector3 m_destinationPosition;

	/// <summary>목표 x 축회전각도</summary>
	private float m_destinationRotationX;

	/// <summary>목표 평행투영사이즈</summary>
	private float m_destinationSize;

	/// <summary>카메라 이동의 시작시간</summary>
	private float m_beginTime = 0.0f;

	/// <summary>카메라 이동의 종료시간</summary>
	private float m_endTime = 0.0f;

	/// <summary>카메라가 이동중인가</summary>
	private bool m_isMoving = false;
}
