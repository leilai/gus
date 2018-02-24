
using UnityEngine;


/// <summary>타이틀 화면에서의 게임시작용 클래스</summary>
class TitleControl : MonoBehaviour
{
	//==============================================================================================
	// 내부 데이터 형태

	/// <summary>전환상태</summary>
	private enum STEP
	{
		NONE = -1,
		SELECT = 0,   // 선택중
        PLAY_JINGLE,  // JINGLE 재생중     
		START_GAME,   // 게임 시작
		NUM
	}

	/// <summary>게임 챕터</summary>
	private enum CHAPTER
	{
		NONE = -1,
		PROLOGUE = 0,
		C1,
		C2,
		C3_0,
		C3_1,
		C4,
		C5,
		EPILOGUE,
		NUM
	}


	//==============================================================================================
    // MonoBehaviour  관련 멤버 변수・메소드

	/// <summary>타이틀 화면 텍스처</summary>
	public Texture2D m_titleTexture;

    /// <summary>JINGLE음 오디오클립</summary>
	public AudioClip m_startSound;

	/// <summary>스타트업 메소드</summary>
	private void Start()
	{
		m_chapterNames = new string[ ( int ) CHAPTER.NUM ];

		m_chapterNames[ ( int ) CHAPTER.PROLOGUE ] = "프롤로그";
		m_chapterNames[ ( int ) CHAPTER.C1 ]       = "제1장";
		m_chapterNames[ ( int ) CHAPTER.C2 ]       = "제2장";
        m_chapterNames[(int)CHAPTER.C3_0]          = "제3장　전반";
        m_chapterNames[(int)CHAPTER.C3_1]          = "제3장　후반";
        m_chapterNames[(int)CHAPTER.C4]            = "제4장";
        m_chapterNames[(int)CHAPTER.C5]            = "제5장";
		m_chapterNames[ ( int ) CHAPTER.EPILOGUE ] = "에필로그";
	}

	/// <summary>매 프레임 갱신 메소드</summary>
	private void Update()
	{
		// 스탭 내의 전환 체크
		if ( m_nextStep == STEP.NONE )
		{
			switch ( m_step )
			{
			case STEP.NONE:
				m_nextStep = STEP.SELECT;
				break;

#if !UNITY_EDITOR
			case STEP.SELECT:
				if ( Input.GetMouseButtonDown( 0 ) )
				{
					m_nextStep = STEP.PLAY_JINGLE;
				}
				break;
#endif //!UNITY_EDITOR

			case STEP.PLAY_JINGLE:
				if ( !audio.isPlaying )
				{
					m_nextStep = STEP.START_GAME;
				}
				break;
			}
		}

		// 상태가 전환될 때의 초기화
		while ( m_nextStep != STEP.NONE )
		{
			m_step = m_nextStep;
			m_nextStep = STEP.NONE;

			switch ( m_step )
			{
			case STEP.PLAY_JINGLE:
                    // JINGLE 음재생
				audio.clip = m_startSound;
				audio.Play();
				break;

			case STEP.START_GAME:
#if !UNITY_EDITOR
				// 프롤로그 시작 
				GlobalParam.getInstance().setStartScriptFiles( "Events/c00_main.txt", "Events/c00_sub.txt" );
#else
				// 選択によって読み込むファイルを変える
				switch ( m_selectedChapter )
				{
				case ( int ) CHAPTER.PROLOGUE:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c00_main.txt", "Events/c00_sub.txt" );
					break;

				case ( int ) CHAPTER.C1:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c01_main.txt", "Events/c01_sub.txt" );
					break;

				case ( int ) CHAPTER.C2:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c02_main.txt", "Events/c02_sub.txt" );
					break;

				case ( int ) CHAPTER.C3_0:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c03_0_main.txt", "Events/c03_0_sub.txt" );
					break;

				case ( int ) CHAPTER.C3_1:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c03_1_main.txt", "Events/c03_1_sub.txt" );
					break;

				case ( int ) CHAPTER.C4:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c04_main.txt", "Events/c04_sub.txt" );
					break;

				case ( int ) CHAPTER.C5:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c05_main.txt", "Events/c05_sub.txt" );
					break;

				case ( int ) CHAPTER.EPILOGUE:
					GlobalParam.getInstance().setStartScriptFiles( "Events/c90_main.txt", "Events/c90_sub.txt" );
					break;
				}
#endif //!UNITY_EDITOR

				// 게임씬을 로딩
				Application.LoadLevel( "GameScene" );

				break;
			}
		}
	}

	/// <summary>GUI 핸드링 메소드</summary>
	private void OnGUI()
	{
		// 타이틀 화면
		GUI.DrawTexture( new Rect( 0, 0, m_titleTexture.width, m_titleTexture.height ), m_titleTexture );

#if UNITY_EDITOR
		// デバッグ用の章セレクトボタン
		int x      =  10;
		int y      =  10;
		int width  = 100;
		int height =  20;

		for ( int i = 0; i < ( int ) CHAPTER.NUM; ++i, y += height + 10 )
		{
			if ( GUI.Button( new Rect( x, y, width, height ), m_chapterNames[ i ] ) )
			{
				if ( m_step == STEP.SELECT )
				{
					m_selectedChapter = i;
					m_nextStep        = STEP.PLAY_JINGLE;
				}
			}
		}
#endif //UNITY_EDITOR
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>현재의 상태</summary>
	private STEP m_step = STEP.NONE;

	/// <summary>다음으로 전환하는 상태</summary>
	private STEP m_nextStep = STEP.NONE;

	/// <summary>각 장의 이름</summary>
	private string[] m_chapterNames;

#if UNITY_EDITOR
	/// <summary>デバッグモードで選択した章
	private int m_selectedChapter = 0;
#endif //UNITY_EDITOR
}
