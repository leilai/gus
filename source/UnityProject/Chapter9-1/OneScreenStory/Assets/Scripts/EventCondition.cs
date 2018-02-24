
/// <summary>이벤트 발생조건</summary>
class EventCondition
{
	//==============================================================================================
	// 공개 메소드

	/// <summary>생성자</summary>
	public EventCondition( BaseObject baseObject, string name, string compareValue )
	{
		m_object       = baseObject;
		m_name         = name;
		m_compareValue = compareValue;
	}

	/// <summary>발생조건을 평가한다</summary>
	public bool evaluate()
	{
		string value = m_object.getVariable( m_name );
		if ( value == null ) {
			value = "0";
		}

		return m_compareValue == value;
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>게임 내 변수를 참조할 오브젝트</summary>
	private BaseObject m_object;

	/// <summary>게임 내 변수</summary>
	private string m_name;

	/// <summary>비교값</summary>
	private string m_compareValue;
}
