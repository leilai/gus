using UnityEngine;
using System.Collections;

/// <summary>
/// 랜덤한 위치에 인스턴스를 생성.
/// </summary>
public class RandomGenerator : MonoBehaviour {

    [SerializeField]
    private GameObject target = null;  // 생성대상.
    [SerializeField]
    private GenerateParameter param = new GenerateParameter();
    [SerializeField]
    private bool relative = false;
    [SerializeField]
    private Rect runningArea = new Rect(-700.0f, -700.0f, 1400.0f, 1400.0f);

    private int counter = 0;

    private bool clear = false;
    private bool limitCheck = false;
    private bool ready = false;

    private ArrayList childrenArray = new ArrayList();
    private ArrayList sonarArray = new ArrayList();

    private GameObject field = null;
    private GameObject player = null;
   
    void Start()
    {
        // 초기 배치 분량이 존재하는 경우에는 여기에서 등록해 둔다.(주로 디버그용)
        GameObject[] children = GameObject.FindGameObjectsWithTag(target.tag);
        for (int i = 0; i < children.Length; i++ )
        {
            childrenArray.Add(children[i]);
            sonarArray.Add(children[i]);
        }

        field = GameObject.Find("/Field");
        player = GameObject.Find("/Field/Player");
    }

    void Update()
    {
        if (TimingCheck())
        {
            ready = false;
            StartCoroutine("Delay");
//            Generate();
        }
    }

    private bool TimingCheck()
    {
        // 준비되어 있지 않다.
        if (!ready) return false;
        // 1번의 한도점에 도달하고,  endless 플래그가 설정되어 있지 않은 경우에는 추가하지 않는다.
        if (!param.endless && limitCheck) return false;
        // 개수 체크.
        return (ChildrenNum() < param.limitNum) ? true : false;
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(param.delayTime);

        Generate();
        ready = true;
    }

    void OnGeneratorStart()
    {
        counter = 0;
        ready = true;
        limitCheck = false;
    }
    void OnGeneratorSuspend()
    {
        ready = false;
    }
    void OnGeneratorResume()
    {
        ready = true;
    }

    /// <summary>
    /// [ Message ] 오브젝트 폐기.
    /// </summary>
    void OnDestroyObject( GameObject target )
    {
        // 배열에 남아있으면 삭제.
        childrenArray.Remove(target);
        sonarArray.Remove(target);
        // Child가 감소한 것을 알림.
        SendMessage("OnDestroyChild", target, SendMessageOptions.DontRequireReceiver);
        if (field) field.SendMessage("OnSwitchCheck", target.tag);
        Destroy(target);
    }

    // 오브젝트 생성
    public void Generate()
    {
        Rect rect = param.posXZ;
        float offsetX = 0.0f;
        float offsetZ = 0.0f;
        if (relative)
        {
            offsetX = player.transform.position.x;
            offsetZ = player.transform.position.z;
        }

        Vector3 pos = new Vector3(rect.xMin + offsetX, 0, rect.yMin + offsetZ);
        if (param.fill)
        {
            // posRange내에 랜덤하게 위치를 정한다.
            pos.x += rect.width * Random.value;
            pos.z += rect.height * Random.value;
        }
        else {
            // posRange외주에 랜덤하게 위치를 정한다.
            if (Random.Range(0, 2) == 1)
            {
                pos.x += rect.width * Random.value;
                if (Random.Range(0, 2) == 1) pos.z = rect.yMax;
            }
            else
            {
                if (Random.Range(0, 2) == 1) pos.x = rect.xMax;
                pos.z += rect.height * Random.value;
            }
        }

        // 범위 밖이라면 Clamp하여 수정
        pos.x = Mathf.Clamp(pos.x, runningArea.xMin, runningArea.xMax);
        pos.z = Mathf.Clamp(pos.z, runningArea.yMin, runningArea.yMax);

        // 인스턴스 생성.
        GameObject newChild = Object.Instantiate(target, pos, Quaternion.identity) as GameObject;
        // 自分を親にする
        newChild.transform.parent = transform;
        Debug.Log("generated[" + ChildrenNum() + "]=" + newChild.name);

        // 배열 갱신
        childrenArray.Add(newChild);
        sonarArray.Add(newChild);

        // Child가 증가한 것을 알림.
        SendMessage("OnInstantiatedChild", newChild, SendMessageOptions.DontRequireReceiver);

        counter++;
        if (counter >= param.limitNum)
        {
            limitCheck = true;  // 한 번의 한도에 도달하면 체크한다.
        }
    }

    public int ChildrenNum()
    {
        if (childrenArray != null) return childrenArray.Count;
        return 0;
    }

    public GameObject Target() { return target; }
    public bool Clear() { return clear; }

    // 관리하고 있는 Child 참조.
    public ArrayList Children() { return childrenArray; }
    // 탐지기에 해당되는 부분을 설정한다.
    public ArrayList SonarChildren() { return sonarArray; }
    // 생성 파라미터 설정
    public void SetParam(GenerateParameter param_) {  param = param_; }

}
