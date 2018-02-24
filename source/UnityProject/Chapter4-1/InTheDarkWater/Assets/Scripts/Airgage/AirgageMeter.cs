using UnityEngine;
using System.Collections;

/// <summary>
/// 공기 잔량 표시 미터. Cutout의 셰이더 이용.
/// </summary>
public class AirgageMeter : MonoBehaviour {

    /// <summary>
    /// [SendMessage]값 갱신.
    /// </summary>
    /// <param name="value">갱신 값. [0,1]</param>
    void OnDisplayAirgage(float value)
    {
        //Debug.Log("OnDeflate: Air=" + value);
        //셰이더의 알파 cutoff의 값을 갱신하여  표시를 갱신한다.
        renderer.material.SetFloat("_Cutoff", value);
    }
}
