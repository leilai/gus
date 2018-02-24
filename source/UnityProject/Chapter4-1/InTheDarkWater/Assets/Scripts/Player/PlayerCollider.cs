using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이의 충돌
/// </summary>
public class PlayerCollider : MonoBehaviour {

    [SerializeField]
    private float speedDown = 2.0f;

    private PlayerController controller;
    private bool damage = true;

	void Start () 
    {
        // 컨트롤러
        controller = GetComponent<PlayerController>();
	}

    void OnGameOver()
    {
        damage = false;
    }
    void OnGameClear()
    {
        damage = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (damage) CheckDamageCollision(collision.gameObject);
        else CheckTerrainCollision(collision.gameObject);
    }
    void OnCollisionStay(Collision collision)
    {
        if (damage) CheckDamageCollision(collision.gameObject);
    }

    private void CheckDamageCollision(GameObject target)
    {
        // 약간 스피드를 낮추고 미세조정(너무 스피드가 있으면 explosion 효과가 없다.)
        if (target.CompareTag("Torpedo"))
        {
            controller.AddSpeed( -speedDown );
        }
    }

    private void CheckTerrainCollision(GameObject target)
    {
        // Terrain에 접촉하면 가라앉는 사운드를 재생.
        if (target.CompareTag("Terrain"))
        {
            PlayAudioAtOnce();
        }
    }

    private void PlayAudioAtOnce()
    {
        if (!audio) return;
        if (audio.isPlaying) return;
        audio.Play();
    }
}
