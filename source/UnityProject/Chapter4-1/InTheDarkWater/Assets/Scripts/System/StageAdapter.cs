using UnityEngine;
using System.Collections;

/// <summary>
/// Stage씬용 Adapte. 각 Stage마다의 Field 로더 역할
/// 게임의 시작과 종료는 반드시 여기를 거쳐, 씬 변환시에 Root에 종료를 전달한다.
/// </summary>
public class StageAdapter : MonoBehaviour {

    public enum Type
    {
        None = -1,
        Stage1,
        Stage2,
        Stage3
    };
    [SerializeField]
    private Type currentType = Type.None;   // 현재 스테이지
    [SerializeField]
    private string[] fieldSceneName = new string[] { 
        "Field1", "Field2", "Field3" 
    };  // 각종 메인 씬 제목
   
    private GameObject root = null;
    private GameObject ui = null;
    private GameObject field = null;

    [SerializeField]
    private bool playOnAwake = true;
    private bool nextStage = false;
    private bool loaded = false;

    void Awake()
    {
        // LoadLevel에서 Destory대상에서 제외한다.
        // Destory의 판단은 SceneSelector가 실행한다.
        DontDestroyOnLoad(gameObject);
    }

    // 로드 종료시
//    void OnLevelWasLoaded(int level)
//    {
//        Debug.Log("OnLevelWasLoaded : level=" + level + " - " + Application.loadedLevelName);
    // 로드 종료시 실행하면, 단체 디버깅을 할 수 없으므로 Start로 설정해 둔다.
    void Start()
    {
        root = GameObject.Find("/Root");
        ui = GameObject.Find("/UI");
        // Field가 아직 로딩되어 있지 않으므로 Intermission을 건너뛰어 처음 스테이지를 로드
        if (playOnAwake)
        {
            SetNextStage(Type.Stage1);
            OnIntermissionEnd();
        }
    }

    // 각 Field에서 불러온다.
    void OnLoadedField()
    {
        loaded = true;
        // Field까지 Load되면 인터미션 시작
        Debug.Log("Field Loaded");
        field = GameObject.Find("/Field");

        // 탐지기 위치를 조정하는 등. SlideOut하기 전에 설정해 두어야 할 것
        if (ui) ui.BroadcastMessage("OnAwakeStage", (int)currentType);

        if (root) root.BroadcastMessage("OnSlideOut", gameObject);
        else OnIntermissionEnd();
    }

    // 인터미션 종료 수락
    void OnIntermissionEnd()
    {
        if (loaded) GameStart();
        else Load();
    }

    // 씬 종료 시에 불러온다.
    void OnGameEnd(bool nextStage_)
    {
        nextStage = nextStage_;
        if (nextStage) {
            // 스테이지 클리어 동작 표시
            Debug.Log("!!! GameClear !!!");
            if (field) field.BroadcastMessage("OnGameClear", SendMessageOptions.DontRequireReceiver);
            if (ui) ui.BroadcastMessage("OnGameClear", SendMessageOptions.DontRequireReceiver);
        }
		else {
	        // 게임 오버 동작 표시
            Debug.Log("!!! GameOver !!!");
            if (field) field.BroadcastMessage("OnGameOver", SendMessageOptions.DontRequireReceiver);
            if (ui) ui.BroadcastMessage("OnGameOver", SendMessageOptions.DontRequireReceiver);
		}
    }

    // TitleSwitcher에서 반드시 불러온다.
    void OnSceneEnd()
    {
        if (nextStage)
        {
            // 다음 Stage로 변화
            SetNextStage();
            // 인터미션 시작
            if (root) root.BroadcastMessage("OnSlideIn", gameObject);
            else OnIntermissionEnd();
        }
        else
        {
            // Title로 되돌아온다.
            if (root) root.SendMessage("OnStartTitle");
        }
    }
    
    // 주로 디버그용
    void OnFieldLoad( Type type )
    {
        SetNextStage(type);
        if (root) root.BroadcastMessage("OnSlideIn", gameObject);
        else OnIntermissionEnd();
    }


    // 이제부터 게임 시작
    private void GameStart()
    {
        Debug.Log("!!! GameStart !!!");
        // 게임 시작
        if (field) field.BroadcastMessage("OnGameStart", SendMessageOptions.DontRequireReceiver);
        if (ui) ui.BroadcastMessage("OnGameStart", SendMessageOptions.DontRequireReceiver);
    }
    // 로드
    private bool Load()
    {
        if (currentType == Type.None) return false;

        // field는 삭제한다.
        if (field) GameObject.Destroy(field);
        // UIをリセット
        if (ui) ui.BroadcastMessage("OnStageReset", SendMessageOptions.DontRequireReceiver);

        string name = fieldSceneName[(int)currentType];
        Debug.Log("Load : " + name);
        Application.LoadLevelAdditive(name);
        return true;
    }

    private void SetNextStage(Type setType = Type.None)
    {
        loaded = false;

        // currentType을 설정한다.
        if (setType == Type.None)
        {
            int current = (int)currentType;
            current++;
            // 스테이지 수를 초과한 경우 Title로 되돌아온다.
            if (current >= fieldSceneName.Length)
            {
                // Title로 되돌아온다.
                if (root) root.SendMessage("OnStartTitle");
                return;
            }
            else currentType = (Type)(current);
        }
        else currentType = setType;
    }

}
