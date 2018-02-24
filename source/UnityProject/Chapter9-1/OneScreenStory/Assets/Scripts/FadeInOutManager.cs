
using UnityEngine;


/// <summary>화면의 암전을 관리하는 클래스</summary>
class FadeInOutManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 관련 멤버 변수・메소드

	/// <summary>암전 텍스처</summary>
	public GUITexture m_fadeInOutObject;

	/// <summary>스타트업 메소드</summary>
	private void Start()
	{
		m_fadeInOutObject.color = new Color( 0.0f, 0.0f, 0.0f, m_currentAlpha );
		m_fadeInOutObject.enabled = true;
	}

	/// <summary>매 프레임 갱신 메소드</summary>
	private void Update()
	{
		if ( m_isFading )
		{
			if ( Time.time >= m_endTime )
			{
				// 페이드 시간 경과후 첫 Update
				// 알파값을 목적값으로 변경
				m_fadeInOutObject.color = new Color( 0.0f, 0.0f, 0.0f, m_currentAlpha = m_destinationAlpha );

				// 페이드인이 완료한 경우에는 GUITexture 을 무효로 한다. 
				if ( m_destinationAlpha < 0.25f )	// float 이므로  == 0.0f 는 하지 않는다. 
				{
					m_fadeInOutObject.enabled = false;
				}

				// 페이드 종료
				m_isFading = false;
			}
			else
			{
				// 진행도 (0.0～1.0)
				float ratio = Mathf.InverseLerp( m_beginTime, m_endTime, Time.time );

				// 알파값 변경
				m_fadeInOutObject.color = new Color( 0.0f, 0.0f, 0.0f,
				                                     Mathf.Lerp( m_currentAlpha, m_destinationAlpha, ratio ) );
			}
		}
	}


	//==============================================================================================
	// 공개 메소드

	/// <summary>페이드를 실행한다. </summary>
	public void fadeTo( float destinationAlpha, float duration )
	{
		m_destinationAlpha = destinationAlpha;
		m_beginTime        = Time.time;
		m_endTime          = m_beginTime + duration;
		m_isFading         = true;

		// GUITexture 을 유효화한다.
		m_fadeInOutObject.enabled = true;
	}

	/// <summary>페이드아웃을 실행한다. </summary>
	public void fadeOut( float duration )
	{
		fadeTo( 1.0f, duration );
	}

	/// <summary>페이드인을 실행한다. </summary>
	public void fadeIn( float duration )
	{
		fadeTo( 0.0f, duration );
	}

	/// <summary>페이드 중인지를 전송한다. </summary>
	public bool isFading()
	{
		return m_isFading;
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>현재의 알파값</summary>
	private float m_currentAlpha = 1.0f;

	/// <summary>목표 알파값</summary>
	private float m_destinationAlpha = 0.0f;

	/// <summary>페이드 시작시간</summary>
	private float m_beginTime = 0.0f;

	/// <summary>페이드 종료시간</summary>
	private float m_endTime = 0.0f;

	/// <summary>페이드 중인가</summary>
	private bool m_isFading = false;
}
