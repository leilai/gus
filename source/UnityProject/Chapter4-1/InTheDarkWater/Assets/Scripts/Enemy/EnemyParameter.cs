using UnityEngine;

/// <summary>
/// 적의 파라미터
/// </summary>
[System.Serializable]
public class EnemyParameter 
{
    public int scoreMax = 1000; // 적을 쓰러트려 받을 수 있는 스코어 최대값
    public int scoreMin = 100;   // 적을 쓰러트려 받을 수 있는 스코어 최소값

    public float cautionWaitMax = 1.0f;    // Caution 갱신 간격 최대값
    public float cautionWaitMin = 0.01f;   // Caution 갱신 간격 최소값

    public float cautionWaitLimit = 10.0f;  // sneaking시의 Cautionの갱신 간격 한계값

    public float sneaking = 0.5f;  // 플레이어가 속도를 낮출 때, Caution 갱신 간격의 허용 정도.


    public int sonarHitAddCaution = 10;  // 출현시간(이 시간을 초과하면 자동소멸)
    public EnemyParameter(){ }
    public EnemyParameter(EnemyParameter param_)
    {
        scoreMax = param_.scoreMax;
        scoreMin = param_.scoreMin;
        cautionWaitMax = param_.cautionWaitMax;
        cautionWaitMin = param_.cautionWaitMin;
        cautionWaitLimit = param_.cautionWaitLimit;
        sonarHitAddCaution = param_.sonarHitAddCaution;
        sneaking = param_.sneaking;
    }
}
