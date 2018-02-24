using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 충돌 시에 모델을 삭제한다.
/// </summary>
public class TorpedoModel : MonoBehaviour {


    void OnHit()
    {
        renderer.enabled = false;
    }
}
