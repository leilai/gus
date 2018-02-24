using UnityEngine;
using System.Collections;

/// <summary>
/// 액티브 탐지기
/// </summary>
public class ActiveSonar : MonoBehaviour {

    [SerializeField]
    private float delayTime = 0.2f;

    private float maxRadius = 300.0f;

    private GameObject player = null;
    private RandomGenerator enemy = null;
    private RandomGenerator item  = null;
    private TorpedoManager torpedo = null;
    private SonarEffect effect = null;

	void Start () 
    {
        effect = GetComponent<SonarEffect>();

        player = GameObject.Find("/Field/Player");
        GameObject enemyObj = GameObject.Find("/Field/Enemies");
        if (enemyObj) enemy = enemyObj.GetComponent<RandomGenerator>();
        GameObject itemObj = GameObject.Find("/Field/Items");
        if (itemObj) item = itemObj.GetComponent<RandomGenerator>();
        GameObject torpedoObj = GameObject.Find("/Field/Torpedoes");
        if (torpedoObj) torpedo = torpedoObj.GetComponent<TorpedoManager>();

        GameObject sonarCameraObj = GameObject.Find("/Field/Player/SonarCamera");
        if (sonarCameraObj) maxRadius = sonarCameraObj.GetComponent<SphereCollider>().radius;

        StartCoroutine("Delay");
	}

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(delayTime);

        // 오류 검색
        float effectDist = Mathf.Lerp(0.0f, maxRadius, effect.Value());
        //Debug.Log("ActiveSonar="+effectDist + ":" + Time.time);
        if (enemy)
        {
            //Debug.Log(effectDist + "- Enemy Search :" + enemy.SonarChildren().Count);
            Search(enemy.SonarChildren(), effectDist);
        }
        if (item)
        {
            //Debug.Log(effectDist + "- Item Search :" + item.SonarChildren().Count);
            Search(item.SonarChildren(), effectDist);
        }
        if (torpedo)
        {
            //Debug.Log(effectDist + "- Torpedo Search :" + torpedo.SonarChildren().Count);
            Search(torpedo.SonarChildren(), effectDist);
        }

        StartCoroutine("Delay");
    }
	
    void Search(ArrayList array, float effectDist) 
    {
        if (array == null) return;

        int i = 0;
        while (i < array.Count)
        {
            GameObject target = array[i] as GameObject;
            if (target == null)
            {
                i++;
                continue;
            }

            float dist = Vector3.Distance(target.transform.position, player.transform.position);
            if (dist < effectDist)
            {
                // 지정거리 이내라면 액티브 탐지기
                target.BroadcastMessage("OnActiveSonar");
                // 탐지기 대상 리스트에서 제외
                array.RemoveAt(i);
            }
            else i++;
        }
    }

    public void SetMaxRadius( float radius )
    {
        maxRadius = radius;
    }
}
