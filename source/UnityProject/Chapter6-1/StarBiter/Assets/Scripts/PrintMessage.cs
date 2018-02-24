using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// PrintMessage
//  - sub screen에 표시할 메세지를 제어한다.
//  - 사용방법
//    - 표시할 메세지를 인수로 SetMessage를 불러온다.
//  - 작동 방법
//    - SetMessage에 전달된 메세지에 대해 최초 메세지 부터 순서대로 처리한다.
//    - 메세지를 조금씩sub screen에 표시
//    - 지정한 행수를 초과한 경우에는 첫 행부터 지워 간다.
// ----------------------------------------------------------------------------
public class PrintMessage : MonoBehaviour {
	
	private ArrayList messages = new ArrayList();
	private bool isPrinting = false;
	private GUIText subScreenGUIText;
	const int MAX_ROW_COUNT = 6;
	
	private static int		ADDITION_NUM = 1;		// 한 번에 표시할 문자수
	private static string 	CURSOR_STR = "_";		// 커서의 문자
	
	void Start () {
		
		// sub screen의 인스턴스를 취득
		subScreenGUIText = GameObject.FindGameObjectWithTag("SubScreenMessage").GetComponent<GUIText>();
		
		// sub screen를 개행으로 채운다(첫 메세지는 가장 아래행부터 표시하도록 하기 위해)
		subScreenGUIText.text = "\n\n\n\n\n\n";
		
		// 게임 시작 메세지를 표시      
		SetMessage("STAND BY ALERT.");
		SetMessage("ENEMY FLEETS ARE APPROACHING.");
		SetMessage(" ");
		
	}
	
	void Update () {
	
		// 표시해야 할 메세지의 유무를 확인.
		if ( messages.Count > 0 )
		{
			// sub screen에 표시하고 있는 도중에 새로운 메세지를 처리할 수 없다.
			if ( !isPrinting )
			{
				// 메세지 표시처리를 불러온다.
				isPrinting = true;
				string tmp = messages[0] as string;
				messages.RemoveAt(0); 
				StartCoroutine( "PlayMessage", tmp );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 메세지를 메모리에 설정(선입력 선출력).
	// ------------------------------------------------------------------------
	public void SetMessage( string message )
	{
		messages.Add( message );
	}
	
	// ------------------------------------------------------------------------
	// 메세지를 sub screen에 조금씩 표시
	// ------------------------------------------------------------------------
	IEnumerator PlayMessage( string message )
	{
		char[] charactors = new char[256];
			
		// 영역 이상의 문자는 폐기한다.
		if ( message.Length > 255 )
		{
			message = message.Substring(0, 254);
		}
		
		// 한 문자씩 분할.
		charactors = message.ToCharArray();
		
		// 표시 문자를 취득
		string subScreenText = subScreenGUIText.text;

		subScreenText += "\n";

		// 한 번에 표시할 문자.
		// 고정값＋큐에 담겨있는 행수
		// 표시 대기 메세지가 큐에 많이 담겨 있는 경우에
		// 표시 스피드가 빨라지도록 	
		int	additionNum = ADDITION_NUM + messages.Count;

		for(int i = 0;i < charactors.Length;i += additionNum)
		{
			// 말미의 커서를 한 번 삭제
			if(subScreenText.EndsWith(CURSOR_STR)) {

				subScreenText = subScreenText.Remove(subScreenText.Length - 1);
			}


			//  버퍼 중인 문자를  additionNum 문자씩 추가
			for(int j = 0;j < additionNum;j++) {

				if(i + j >= charactors.Length) {

					break;
				}

				subScreenText += charactors[i + j];
			}


			// 커서를 추가
			subScreenText += CURSOR_STR;

			// 화면에서 밀리는 행은 삭제

			string[] lines = subScreenText.Split("\n"[0]);

			if(lines.Length > MAX_ROW_COUNT) {

				subScreenText = "";

				// 뒤부터 MAX_ROW_COUNT 행을 추가한다.
				for(int j = lines.Length - MAX_ROW_COUNT;j < lines.Length;j++) {

					subScreenText += lines[j];

					if(j < lines.Length - 1) {

						subScreenText += "\n";
					}
				}
			}

			subScreenGUIText.text = subScreenText;

            //  Wait
			yield return new WaitForSeconds( 0.001f );
		}
		
		// 문자 표시가 전부 종료되면 커서는 표시하지 않는다. 
		if(subScreenText.EndsWith(CURSOR_STR)) {

			subScreenText = subScreenText.Remove(subScreenText.Length - 1);

			subScreenGUIText.text = subScreenText;
		}
			
		// 표시 처리 종료
        isPrinting = false;
	}

}
