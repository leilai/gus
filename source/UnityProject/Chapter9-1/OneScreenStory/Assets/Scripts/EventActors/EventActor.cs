
/// <summary> 이벤트 처리를 실행할 액터의 기초 클래스</summary>
abstract class EventActor
{
	//==============================================================================================
	// 공개 메소드

	/// <summary> 액터가 생성될 때에 가장 처음에 실행되는 메소드</summary>
	/// UnityEngine.MonoBehaviour.Start()  
	public virtual void start( EventManager evman )
	{
	}

	/// <summary> 액터가 파괴될 때까지 매 프래임 실행되는 메소드</summary>
	/// UnityEngine.MonoBehaviour.Update() 
	public virtual void execute( EventManager evman )
	{
	}

	/// <summary> 액터가 파괴될 때까지  GUI의 표시 타이밍에서 실행되는 메소드</summary>
	/// UnityEngine.MonoBehaviour.OnGUI() 
	public virtual void onGUI( EventManager evman )
	{
	}

	/// <summary> 액터로 실행해야하는 처리를 끝낼지를 전송</summary>
	public virtual bool isDone()
	{
		// 기본은 바로 종료
		return true;
	}

	/// <summary> 실행 종료후에 클릭을 기다릴지 전송</summary>
	public virtual bool isWaitClick( EventManager evman )
	{
		// 기본 대기
		return true;
	}
}
