using UnityEngine;
using System.Collections;

[System.Serializable]
public class GenerateParameter 
{
    public Rect posXZ = new Rect(-900.0f, -900.0f, 1800.0f, 1800.0f);
    public bool fill = false;   // true: posXZ내를 전부 대상으로 한다.
                                // flase: posXZ의 외주를 대상으로 한다.
    public int limitNum = 1;    // 필드에 존재가능한 수.
    public float delayTime = 1.0f;  // 생선 전의 delay
    public bool endless = true;   // 한계점의 수보다 줄어든 경우에 자동 추가할지.

}
