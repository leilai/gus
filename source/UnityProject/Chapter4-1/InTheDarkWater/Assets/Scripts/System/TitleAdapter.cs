using UnityEngine;
using System.Collections;

/// <summary>
/// Title씬용 Adapter。
/// Root에 씬 종료를 전달한다.
/// </summary>
public class TitleAdapter : MonoBehaviour {

    private GameObject root = null;
    private GameObject ui = null;

    // 로드 종료시
//    void OnLevelWasLoaded(int level)
//    {
//        Debug.Log("OnLevelWasLoaded : level=" + level + " - " + Application.loadedLevelName);
    // 로드 종료시 설정하면 단체 디버깅을 할 수 없기 때문에 Start에 한다.
    void Start()
    {
        root = GameObject.Find("/Root");
        ui = GameObject.Find("/UI");

        if (root)
        {
            SetHighScore();
            root.BroadcastMessage("OnFadeOut", gameObject);
        }
        else OnIntermissionEnd();
    }

    // 인터미션 종류 수락
    void OnIntermissionEnd()
    {
        // 이제부터 게임시작(플레이어 조작 가능)
        if(ui) ui.BroadcastMessage("OnStartSwitcher");
    }

    // 씬 종료시 불러온다.
    void OnSceneEnd()
    {
        // Stage를 시작한다.
        if (root) root.SendMessage("OnStartStage");
    }

    private void SetHighScore()
    {
        int highScore = 0;
        SceneSelector selecter = root.GetComponent<SceneSelector>();
        if (selecter) highScore = selecter.HighScore();
        ui.BroadcastMessage("OnAddScore", highScore);
    }
}
