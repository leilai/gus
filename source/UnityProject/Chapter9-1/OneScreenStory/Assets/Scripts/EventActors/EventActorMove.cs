
using UnityEngine;


/// <summary>move 커맨드 이벤트 액터</summary>
class EventActorMove : EventActor
{
	//==============================================================================================
    //공개 메소드

    /// <summary>생성자</summary>
	public EventActorMove( BaseObject target, BaseObject to, float duration )
	{
		m_target         = target;
		m_targetPosition = m_target.transform.position;  // 현재의 좌표를 기억한다.
		m_to             = to;
		m_beginTime      = Time.time;
		m_endTime        = m_beginTime + duration;
	}

    /// <summary>액터가 파괴될 때까지 매 프래임 실행되는 메소드</summary>
	public override void execute( EventManager evman )
	{
		Vector3 presentPosition;

		if ( Time.time >= m_endTime )
		{
			presentPosition = m_to.transform.position;
			m_isDone = true;
		}
		else
		{
			// 진행도 (0.0～1.0)
			float ratio = Mathf.InverseLerp( m_beginTime, m_endTime, Time.time );

			presentPosition = Vector3.Lerp( m_targetPosition, m_to.transform.position, ratio );
		}

		// y 좌표 조정 (Terrain 과의 충돌점에 배치한다.)
		RaycastHit hit;
		if ( Physics.Raycast( presentPosition + 10000.0f * Vector3.up,
		                      -Vector3.up,
		                      out hit,
		                      float.PositiveInfinity,
		                      1 << LayerMask.NameToLayer( "Terrain" ) ) )
		{
			m_target.transform.position = hit.point;
		}
		else
		{
			m_target.transform.position = new Vector3( presentPosition.x, 0.0f, presentPosition.z );
		}
	}

    /// <summary>액터로 실행해야하는 처리를 끝낼지를 전송</summary>
	public override bool isDone()
	{
		return m_isDone;
	}

    /// <summary>실행 종료후에 클릭을 기다릴지 전송</summary>
	public override bool isWaitClick( EventManager evman )
	{
		return false;
	}


	//==============================================================================================
    // 비공개 멤버 변수

	/// <summary>이동 대상 오브젝트</summary>
	private BaseObject m_target;

	/// <summary>이동전 좌표</summary>
	private Vector3 m_targetPosition;

	/// <summary>이동지점이 되는 오브젝트</summary>
	private BaseObject m_to;

	/// <summary>이동 시작시간</summary>
	private float m_beginTime;

	/// <summary>이동 종료시간</summary>
	private float m_endTime;

	/// <summary>액터의 처리를 종료할 것인가</summary>
	private bool m_isDone = false;


	//==============================================================================================
    // 정적 메소드

    /// <summary>이벤트 액터의 인스턴스를 생성한다.</summary>
	public static EventActorMove CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 3 )
		{
			ObjectManager om = manager.GetComponent< ObjectManager >();
			BaseObject target = om.find( parameters[ 0 ] );
			BaseObject to     = om.find( parameters[ 1 ] );
			float duration;

			if ( target != null && to != null && float.TryParse( parameters[ 2 ], out duration ) )
			{
                // 액터를 생성
				EventActorMove actor = new EventActorMove( target, to, duration );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
