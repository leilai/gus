using UnityEngine;
using System.Collections;

/// <summary>
/// 잔량의 공기 거품이 나오는 효과.
/// </summary>
public class AirgageBubble : MonoBehaviour {

    void OnDisplayDamageLv(int value)
    {
        particleSystem.emissionRate = 5 + 10 * (float)(value);
    }

    void OnGameOver()
    {
        particleSystem.Stop();
    }
}
