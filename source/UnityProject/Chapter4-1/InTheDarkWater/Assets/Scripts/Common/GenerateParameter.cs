using UnityEngine;

/// <summary>
/// 생성시의 파라미터.
/// </summary>
[System.Serializable]
public class GenerateParameter 
{
    public Rect posXZ = new Rect(-900.0f, -900.0f, 1800.0f, 1800.0f);
    public bool fill = false;   // true: posXZ내를 전부 대상으로 한다.
                                // flase: posXZ의 외주를 대상으로 한다.
    public int limitNum = 1;    // 필드에 존재가능한 수.
    public float delayTime = 1.0f;  // 생성 전의 delay.
    public bool endless = true;   // 한도수에서 감소하는 경우 자동 추가할지.

}
