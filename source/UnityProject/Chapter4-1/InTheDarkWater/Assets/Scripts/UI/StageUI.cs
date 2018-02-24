using UnityEngine;
using System.Collections;

/// <summary>
/// Stage씬 용
/// </summary>
public class StageUI : MonoBehaviour {


	void Awake()
	{
		// LoadLevel에서 Destory 대상에서 제외한다.
        // Destory의 판단은 SceneSelector가 실행한다.
        DontDestroyOnLoad(gameObject);
	}

    // 스코어를 보낸다.
    public int Score()
    {
        GameObject scoreDisp = GameObject.Find("ScoreDisplay");
        if (scoreDisp)
        {
            ScoreDisplay disp = scoreDisp.GetComponent<ScoreDisplay>();
            if (disp) return disp.Score();
        }
        return 0;
    }
}
