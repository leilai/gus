using UnityEngine;
using System.Collections;

public class RandomGenerator : MonoBehaviour {

    [SerializeField]
    private GameObject target;  // 생성대상.
    [SerializeField]
    private GenerateParameter param = new GenerateParameter();
    [SerializeField]
    private float posY = 1.0f;

    private int counter = 0;
    private bool limitCheck = false;
    private bool ready = false;

    private ArrayList childrenArray = new ArrayList();
//    private ArrayList sonarArray = new ArrayList();

    private GameObject field = null;
   
    void Start()
    {
        // 초기 배치 분량이 존재하는 경우에는 여기에 등장시켜 둔다. (주로 디버그용)
        GameObject[] children = GameObject.FindGameObjectsWithTag(target.tag);
        for (int i = 0; i < children.Length; i++ )
        {
            childrenArray.Add(children[i]);
//            sonarArray.Add(children[i]);
        }

        //field = GameObject.Find("/Field");
        OnGeneratorStart();
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
        // 1회 한정으로 되어 있고,endless 플래그 설정되어 있지 않은 경우에는 추가하지 않는다.
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
        // 배열에 남아 있으면 삭제.
        childrenArray.Remove(target);
//        sonarArray.Remove(target);
        // Child가 감소했음을 알림.
        SendMessage("OnDestroyChild", target, SendMessageOptions.DontRequireReceiver);
//        if (field) field.SendMessage("OnSwitchCheck", target.tag);
        Destroy(target);
    }

    // 오브젝트 생성.
    public void Generate()
    {
        Rect rect = param.posXZ;
        Vector3 pos = new Vector3(rect.xMin, posY, rect.yMin);
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

        // 인스턴스 생성.
        GameObject newChild = Object.Instantiate(target, pos, Quaternion.identity) as GameObject;
        // 부모로 설정한다.
        newChild.transform.parent = transform;
        Debug.Log("generated[" + ChildrenNum() + "]=" + newChild.name);

        // 배열갱신.
        childrenArray.Add(newChild);
//        sonarArray.Add(newChild);
        // Child가 증가했음을 알림.
        SendMessage("OnInstantiatedChild", newChild, SendMessageOptions.DontRequireReceiver);

        counter++;
        if (counter >= param.limitNum)
        {
            limitCheck = true;  // 한번의 한도에 도달하면 체크를 한다.
        }
    }

    /*
    // 오류 수집.
    private void UpdateArray()
    {
        childrenArray = GameObject.FindGameObjectsWithTag(target.tag);
        // OnUpdateArray가 있으면 알림.
        SendMessage("OnUpdateArray", childrenArray, SendMessageOptions.DontRequireReceiver);
    }
    */

    public int ChildrenNum()
    {
        if (childrenArray != null) return childrenArray.Count;
        return 0;
    }
    public GameObject Target() { return target; }

    // 관리하고 있는 Child 참조.
    public ArrayList Children() { return childrenArray; }
    // 음향 탐지기에 해당되는 부분을 준비한다.
//    public ArrayList SonarChildren() { return sonarArray; }
    // 생성 Parameter 설정.
    public void SetParam(GenerateParameter param_) {  param = param_; }

}
