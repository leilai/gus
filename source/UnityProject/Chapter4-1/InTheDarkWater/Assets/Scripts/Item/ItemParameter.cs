/// <summary>
/// 아이템 파라미터
/// </summary>
[System.Serializable]
public class ItemParameter 
{
    public int scoreMax = 1000; // 스코어 최대값
    public int scoreMin = 100;   // 스코어 최소값

    public int recoveryMax = 100;    // 회복량 최대값
    public int recoveryMin = 10;     // 회복량 최소값

    public float lifeTime = 60.0f;  // 출현시간(이 시간을 초과하면 자동소멸)


    public ItemParameter() { }
    public ItemParameter(ItemParameter param_)
    {
        scoreMax = param_.scoreMax;
        scoreMin = param_.scoreMin;  
        recoveryMax = param_.recoveryMax;   
        recoveryMin = param_.recoveryMin;
        lifeTime = param_.lifeTime;  

    }

}
