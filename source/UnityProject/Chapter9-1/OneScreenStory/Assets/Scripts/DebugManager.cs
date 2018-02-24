
using System.Collections.Generic;
using UnityEngine;


/// <summary>조건 변수의 Watch와 변수 기능을 제공하는 클래스</summary>
class DebugManager : MonoBehaviour
{
	//==============================================================================================
	// 내부 데이터 형태

    /// <summary>Watch 하는 조건변수</summary>
	private struct WatchCV
	{
		public string target;     // 캐릭터
		public string condition;  // 조건변수
	};


	//==============================================================================================
	// MonoBehaviour 관련 멤버 변수・메소드

#if UNITY_EDITOR
	/// <summary>フレーム毎更新メソッド</summary>
	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.W ) )
		{
			m_isActive = !m_isActive;
		}
	}

	/// <summary>GUI ハンドリングメソッド</summary>
	private void OnGUI()
	{
		if ( m_isActive )
		{
			displayWatchCVS();
		}
	}
#endif //UNITY_EDITOR


	//==============================================================================================
	// 공개 메소드

    /// <summary>Watch 하는 조건변수를 추가한다.</summary>
	public void addWatchConditionVariable( string target, string condition )
	{
		WatchCV cv;
		cv.target    = target;
		cv.condition = condition;

		// 리스트에 없다면 추가한다.
		if ( m_watchCVS.FindIndex( x => ( x.target == target && x.condition == condition ) ) < 0 )
		{
			m_watchCVS.Add( cv );
		}
	}


	//==============================================================================================
	// 비공개 메소드

    /// <summary>Watch 하는 조건변수를 표시한다.</summary>
	private void displayWatchCVS()
	{
		// 초기위치
		int x = 100;
		int y =  50;
		int w = 150;
		int h =  20;

		foreach ( WatchCV cv in m_watchCVS )
		{
			GameObject go = GameObject.Find( cv.target );
			BaseObject bo = go != null ? go.GetComponent< BaseObject >() : null;
			if ( bo != null )
			{
				string status = " ";
				string value = bo.getVariable( cv.condition );

				if(value == null) {
					// 조건변수가 발견되지 않은(미정의) 경우 
					status = "?";
					value  = "0";
				}

				// 캐릭터명과 조건변수명을 표시
				GUI.Label( new Rect( x, y, w, h ), status + cv.target + " " + cv.condition );
				// 값을 변경하기 위한 텍스트필드
				string newValue = GUI.TextField( new Rect( x + w, y, 50, h ), value );

				// 값이 변경되면 게임 내 변수에 반영
				if ( newValue != value )
				{
					bo.setVariable( cv.condition, newValue );
				}

				y += h;
			}
		}
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>조건 변수 리스트</summary>
	private List< WatchCV > m_watchCVS = new List< WatchCV >();

	/// <summary>이 기능이 액티브로 되어 있는가</summary>
	private bool m_isActive = false;
}
