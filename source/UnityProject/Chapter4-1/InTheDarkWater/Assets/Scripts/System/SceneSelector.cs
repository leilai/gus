using UnityEngine;
using System.Collections;

/// <summary>
/// 씬을 변경한다.
/// </summary>
public class SceneSelector : MonoBehaviour {

    public enum Type {
        None = -1,
        Title = 0,
        Stage
    };
    [SerializeField]
    private string titleSceneName = "Title";
    [SerializeField]
    private string stageSceneName = "Stage";
    [SerializeField]
    private Type type = Type.None;   // 현재 스테이지

    [SerializeField]
    private bool playOnAwake = true;    // 바로 시작할지（Release일 때에는 On으로 할 것）

    [SerializeField]    // debug
    private int highScore = 0;        // 하이 스코어 기록

	void Awake()
	{
		// LoadLevel에서 Destory되는 대상에서 제외
        DontDestroyOnLoad(gameObject);
	}
	
    void Start() 
    {
        // 즉시 Title 로드(로드 관계에서 만일에 대비해 1.0f 간격을 둔다)
        if (playOnAwake) StartCoroutine("Wait", 1.0f);
    }
    private IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        LoadScene();
    }

    private bool LoadScene()
    {
        switch (type)
        { 
            default:    return false;
            case Type.Title:
                GameObject ui = GameObject.Find("/UI");
                if(ui) {
                    // 하이 스코어 갱신
                    UpdateHiScore( ui );
                    // UI강제 삭제
                    Destroy(ui);
                    ui = null;
                }
                // 참조를 저장
                GameObject adapter = GameObject.Find("/Adapter");
                if (adapter)
                {
                    // Adapter도 강제 삭제
                    Destroy(adapter);
                    adapter = null;
                }
                Application.LoadLevel(titleSceneName);
                break;
            case Type.Stage:
                // Title는 DontDestory지정하지 않기 때문에 LoadLevel으로 OK
                Application.LoadLevel(stageSceneName);
                break;
        }
        
        return true;
    }

    private void UpdateHiScore( GameObject ui )
    {
        StageUI stageUI = ui.GetComponent<StageUI>();
        int newScore = 0;
        if (stageUI) newScore = stageUI.Score();
        if (highScore < newScore) highScore = newScore;
    }

    // 타이틀 로드
    void OnStartTitle()
    {
        // 타이틀 설정
        type = Type.Title;
        // 인터미션 시작
        BroadcastMessage("OnFadeIn", gameObject);
    }

    // 다음 스테이지를 로드
    void OnStartStage( )
    {
        // 스테이지를 설정
        type = Type.Stage;
        // 인터미션 시작.
        BroadcastMessage("OnFadeIn", gameObject);
    }

    // 인터미션 종료 수락.
    void OnIntermissionEnd()
    {
        // 로드 되지 않았다면 로드 시작.
        LoadScene(); 
    }

    public int HighScore()
    {
        return highScore;
    }
}
