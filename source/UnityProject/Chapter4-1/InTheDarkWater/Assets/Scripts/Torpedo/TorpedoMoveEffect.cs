using UnityEngine;
using System.Collections;

/// <summary>
/// 충돌 시에 파티클을 삭제한다.
/// </summary>
public class TorpedoMoveEffect : MonoBehaviour {

    void OnHit()
    {
        particleSystem.Stop();
    }
}
