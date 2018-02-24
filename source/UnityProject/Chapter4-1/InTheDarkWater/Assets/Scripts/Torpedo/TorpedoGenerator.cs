using UnityEngine;
using System.Collections;

/// <summary>
/// 어뢰 생성
/// </summary>
public class TorpedoGenerator : MonoBehaviour {

    [SerializeField]
    private GameObject target = null;
    [SerializeField]
    private Vector3 pos = new Vector3();            // 최초 생성위치
    [SerializeField]
    private float coolTime = 3.0f;  // 쿨타임
    [SerializeField]
    private bool sound = false;    // 소리를 재생하는가
    [SerializeField]
    private bool sonar = false;  // 탐지기 표시할 것인가
    [SerializeField]
    private float speed = 15.0f;
    [SerializeField]
    private TorpedoCollider.OwnerType type = TorpedoCollider.OwnerType.Enemy;
                                        // 소유자

    private float current;

    private bool valid = true;
    private GameObject parentObj = null;

    void Start()
    {
        // 어뢰 배치 장소
        parentObj = GameObject.Find("/Field/Torpedoes");
    }

    void Update()
    {
        if (!valid)
        {
            // 쿨 타임 계산
            current += Time.deltaTime;
            if (current >= coolTime)
            {
                valid = true;
            }
        }
    }

    public void Generate()
    {
        // 쿨타임 중에는 생성하지 않는다.
        if (valid == false)
        {
            //Debug.Log("Cool time:" + Time.time);
            return;
        }

        // 위치・각도를 구한다.
        Vector3 vec = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        vec += pos.x * transform.right;
        vec += pos.y * transform.up;
        vec += pos.z * transform.forward;
        Quaternion rot = Quaternion.Euler(transform.eulerAngles);
        // 생성
        GameObject newObj = Object.Instantiate(target, vec, rot) as GameObject;
        // 부모를 설정
        newObj.transform.parent = parentObj.transform;

        // 오너 설정
        TorpedoCollider torpedoCollider = newObj.GetComponent<TorpedoCollider>();
        if (torpedoCollider) torpedoCollider.SetOwner(type);
        else Debug.LogError("Not exists TorpedoCollider");
        // 스피드 설정
        TorpedoBehavior torpedoBehavior = newObj.GetComponent<TorpedoBehavior>();
        if (torpedoBehavior) torpedoBehavior.SetSpeed(speed);
        else Debug.LogError("Not exists TorpedoBehavior");
        // 사운드 설정
        Note note = newObj.GetComponentInChildren<Note>();
        if (note) note.SetEnable(sound);
        else Debug.LogError("Not exists Note");
        // 탐지기 설정
        parentObj.SendMessage("OnInstantiatedChild", newObj); 
        if (sonar)
        {
            newObj.BroadcastMessage("OnActiveSonar");
        }


        // 쿨타임 시작
        valid = false;
        current = 0.0f;
    }

}
