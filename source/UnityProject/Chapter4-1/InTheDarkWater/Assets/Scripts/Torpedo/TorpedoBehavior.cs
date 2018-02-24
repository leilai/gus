using UnityEngine;
using System.Collections;

/// <summary>
/// 어뢰 이동
/// </summary>
public class TorpedoBehavior : MonoBehaviour {

    [SerializeField]
    private float speed = 1.0f;

    private bool stop = false;

	void Start () 
    {
	
	}
	
	void Update () 
    {
        // 보통은 앞으로 이동한다.
        MoveForward();
	}

    /// <summary>
    /// Note에서 삭제허락을 받는다.
    /// </summary>
    void OnDestroyLicense()
    {
        // 부모에게 전달한다. 부모가 삭제하도록 한다.
        transform.parent.SendMessage("OnDestroyChild", gameObject);
    }
    /// <summary>
    /// 충돌 알림
    /// </summary>
    void OnHit()
    {
        speed = 0.0f;
        stop = true;
    }

    private void MoveForward()
    {
        if (stop) return;
        Vector3 vec = speed * transform.forward.normalized;
        rigidbody.MovePosition(rigidbody.position + vec * Time.deltaTime);
    }


    public void SetSpeed( float speed_ ) { speed = speed_; }

}
