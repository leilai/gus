using UnityEngine;
using System.Collections;

/// <summary>
/// 어뢰 충돌
/// </summary>
public class TorpedoCollider : MonoBehaviour {

    public enum OwnerType {
        Player,
        Enemy
    };
    private OwnerType owner;

    [SerializeField]
    private float delayTime = 2.0f;
    [SerializeField]
    private int damageValue = 1;

    [System.Serializable]
    public class Explosion
    {
        [SerializeField]
        private float force = 100.0f;
        [SerializeField]
        private float upwardsModifier = 0.0f;
        [SerializeField]
        private ForceMode mode = ForceMode.Impulse;

        private float radius = 3.0f;
            
        public void Add(Rigidbody target, Vector3 pos) 
        {
            target.AddExplosionForce(force, pos, radius, upwardsModifier, mode);
        }
        public void SetRadius(float value) { radius = value; }
    };
    [SerializeField]
    Explosion explosion = new Explosion(); 

    private GameObject ui = null;

	void Start () 
    {
        ui = GameObject.Find("/UI");

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider) explosion.SetRadius( sphereCollider.radius );

        // 발사한 자신도 충돌이 존재하기 때문에, 발사 후 몇 초정도 충돌판정을 하지 않는다.
        collider.enabled = false;
        StartCoroutine("Delay");
	}

    /// <summary>
    /// 게임 오버시
    /// /// </summary>
    void OnGameOver()
    {
        collider.enabled = false;
    }
    /// <summary>
    /// 게임 클리어시
    /// </summary>
    void OnGameClear()
    {
        collider.enabled = false;
    }

    //  Enter만으로 놓칠 가능성이 있기 때문에, Stay에서도 Collision판정을 한다.
    void OnCollisionEnter(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }
    void OnCollisionStay(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(delayTime);

        collider.enabled = true;
        Debug.Log("Wait EndCoroutine");
    }

    private void CheckCollision(GameObject target)
    {
        bool hit = false;
        hit |= CheckPlayer(target);
        hit |= CheckEnemy(target);
        hit |= CheckTorpedo(target);
        if( hit ) {
            // 모두 충돌했을 경우 충돌 후 스스로 처리
            BroadcastMessage("OnHit");
            // Collider무효화.
            collider.enabled = false;
        }
    }
    /// <summary>
    /// 어뢰간 충돌
    /// </summary>
    /// <param name="target"> 점검 대상</param>
    /// <returns></returns>
    private bool CheckTorpedo(GameObject target)
    {
        if (target.CompareTag("Torpedo"))
        {
            // 자신과 같다면 어뢰간의 충돌
            Debug.Log("CheckTorpedo");
            return true;
        }
        return false;
    }
    /// <summary>
    /// 플레이어와의 충돌
    /// </summary>
    /// <param name="target">점검 대상</param>
    /// <returns></returns>
    private bool CheckPlayer(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            Debug.Log("CheckPlayer");
            // 충돌을 준다.
            explosion.Add( target.rigidbody, transform.position );
            // 충돌 알림만 전송한다.
            target.BroadcastMessage("OnHit");
            // 데미지 알림
            if (ui) ui.BroadcastMessage("OnDamage", damageValue, SendMessageOptions.DontRequireReceiver);
            return true;
        }
        return false;
    }
    /// <summary>
    /// 적과 충돌
    /// /// </summary>
    /// <param name="target">점검 대상</param>
    /// <returns></returns>
    private bool CheckEnemy(GameObject target)
    {
        if (target.CompareTag("Enemy"))
        {
            Debug.Log("CheckEnemy");
            if (owner == OwnerType.Player)
            {
                // 자신의 어뢰가 적에게 충돌한 경우에만, 적이 가지고 있는 스코어를 더하여 알림
                target.SendMessage("OnAddScore", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                // 적에게 충돌 알림만을 전송한다.
                target.BroadcastMessage("OnHit");
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 어뢰를 발사한 오너 설정
    /// </summary>
    /// <param name="type"></param>
    public void SetOwner(OwnerType type) { owner = type; }
    /// <summary>
    /// 데미지양 설정. 보통은 필요 없음
    /// </summary>
    /// <param name="value"></param>
    public void SetDamageValue(int value) { damageValue = value; }

}
