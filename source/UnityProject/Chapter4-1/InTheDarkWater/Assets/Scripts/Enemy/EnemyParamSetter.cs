using UnityEngine;
using System.Collections;

/// <summary>
/// 적의 파라미터 변동
/// </summary>
public class EnemyParamSetter : MonoBehaviour
{
    [SerializeField]
    private EnemyParameter fromParam = new EnemyParameter();
    [SerializeField]
    private EnemyParameter toParam = new EnemyParameter();
    [SerializeField]
    private float duration = 240.0f;

    private float timeStamp = 0.0f;

    void Start()
    {
        timeStamp = Time.timeSinceLevelLoad;
    }

    void OnInstantiatedChild(GameObject target)
    {
        // 생성된 오브젝트에 대한 설정
        float t = 0;
        if(duration > 0) t = (Time.timeSinceLevelLoad - timeStamp) / duration;
//        Debug.Log("EnemyParamSetter: t=" + t);

        EnemyParameter param = new EnemyParameter();
        param.scoreMax = (int)Mathf.Lerp(fromParam.scoreMax, toParam.scoreMax, t);
        param.scoreMin = (int)Mathf.Lerp(fromParam.scoreMin, toParam.scoreMin, t);
        param.cautionWaitMax = Mathf.Lerp(fromParam.cautionWaitMax, toParam.cautionWaitMax, t);
        param.cautionWaitMin = Mathf.Lerp(fromParam.cautionWaitMin, toParam.cautionWaitMin, t);
        param.sonarHitAddCaution = (int)Mathf.Lerp(fromParam.sonarHitAddCaution, toParam.sonarHitAddCaution, t);
        param.sneaking = Mathf.Lerp(fromParam.sneaking, toParam.sneaking, t);
        // 생성시부터 카운트를 시작한다.
        target.SendMessage("OnStartCautionCount", param);
    }
}
