ing UnityEngine;
using System.Collections;

/// <summary>
/// 어뢰 전반 관리. 어뢰 자동삭제.
/// </summary>
public class TorpedoManager : MonoBehaviour {

    [SerializeField]
    private bool check = true;
    [SerializeField]
    private Rect runningArea = new Rect(-950, -950, 1900, 1900);   // 유효범위(월드 좌표)
    [SerializeField]
    private bool relative = false;
    [SerializeField]
    private float delayTime = 2.0f;

    private Rect rect;

    private ArrayList childrenArray = new ArrayList();
    private ArrayList sonarArray = new ArrayList();

    void Start()
    {
        // 시작한다.
        if (check) StartCoroutine("CheckDelay");
    }

    /// <summary>
    /// 인스턴스 생성 타이밍
    /// </summary>
    /// <param name="target">생성된 인스턴스</param>
    void OnInstantiatedChild(GameObject target)
    {
        childrenArray.Add(target);
        sonarArray.Add(target);
    }
    /// <summary>
    /// 인스턴스 삭제 타이밍
    /// </summary>
    /// <param name="target">삭제대상/param>
    void OnDestroyChild(GameObject target)
    {
        // 리스트에 들어있다면 삭제한다.
        Debug.Log("TorpedManager.OnDestroyChild");
        childrenArray.Remove(target);
        sonarArray.Remove(target);

        Destroy(target);
    }

    /// <summary>
    /// 게임 오버시
    /// </summary>
    void OnGameOver()
    {
        StopAllCoroutines();
    }
    /// <summary>
    /// 게임 클리어시
    /// </summary>
    void OnGameClear()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 어뢰가 유효 영역 밖으로 나오는지 정기적으로 체크한다.ㄷ
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckDelay()
    {
        yield return new WaitForSeconds(delayTime);

        if (relative)
        {
            GameObject player = GameObject.Find("/Field/Player");
            if (player) {
                Vector3 pos = player.transform.position;
                rect = new Rect(runningArea.xMin + pos.x, runningArea.yMin + pos.z, runningArea.width, runningArea.height);
            }
        }
        else rect = new Rect(runningArea);

        int i = 0;
        while (i < childrenArray.Count)
        {
            GameObject target = childrenArray[i] as GameObject;
            if (target == null)
            {
                i++;
                continue;
            }

            Vector3 pos = target.transform.position;
            if (rect.Contains(new Vector2(pos.x, pos.z)))
            {
                i++;
            }
            else
            {
                childrenArray.RemoveAt(i);  // 대상을 삭제
                sonarArray.Remove(target);  // 만일 sonar에도 남아있다면 삭제
                Destroy(target);
            }
        }

        // 다음 Delay
        StartCoroutine("CheckDelay");
    }

    public int ChildrenNum()
    {
        if (childrenArray != null) return childrenArray.Count;
        return 0;
    }

    // 관리하는 Children 참조
    public ArrayList Children() { return childrenArray; }
    // 탐지에 해당되는 부분을 설정한다.
    public ArrayList SonarChildren() { return sonarArray; }
}
