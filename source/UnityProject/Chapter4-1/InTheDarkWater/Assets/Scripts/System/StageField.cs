using UnityEngine;
using System.Collections;

/// <summary>
///스테이지의 각 Field가 공통으로 갖는 스크립트
/// </summary>
public class StageField : MonoBehaviour {


    void Start()
    { 
    }

    // 로드 종료시  LoadLevelAdditiveはOnLevelWasLoaded을 불러오지 않기 때문에 Awake로 처리
    //    void OnLevelWasLoaded(int level)
    void Awake()
    {
        Debug.Log("Stage Loaded");
        // 로드 완료 알림
        GameObject adapter = GameObject.Find("/Adapter");
        if (adapter) adapter.SendMessage("OnLoadedField");
        else Debug.Log("Adapter is not exist!!!");
    }
}
