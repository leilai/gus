using UnityEngine;
using System.Collections;

/// <summary>
/// 최신 최대값 Caution을 표시하기 위한 스크립트
/// </summary>
public class CautionUpdater : MonoBehaviour
{
    [SerializeField] // debug
    private int instantiatedCount = 0;

    private GameObject ui = null;
    private GameObject maxCautionEnemy = null;

    void Start()
    {
    }

    void OnGameStart()
    {
        // 씬을 나누었기 때문에 만일에 대비하여 OnGameStart으로 연결한다.
        ui = GameObject.Find("/UI");
    }

    void OnInstantiatedChild(GameObject target)
    {
        instantiatedCount++;
        //EnemyCaution enemyCaution = target.GetComponent<EnemyCaution>();
        //float waitTime = 0.0f;
        //if (instantiatedCount>0) waitTime = maxWaitTime / (float)instantiatedCount;
        //enemyCaution.SetCountUp(waitTime);

        // 보통 0으로 되어 있지만 만일에 대비하여 Update
        DisplayValue(target, GetCautionValue(target));
    }

    void OnDestroyChild(GameObject target)
    {
        if (target.Equals(maxCautionEnemy))
        {
            maxCautionEnemy = null;
            if(ui)ui.BroadcastMessage("OnUpdateCaution", 0, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void DisplayValue(GameObject updateEnemy, int newValue)
    {
        //Debug.Log(updateEnemy.name + ".cation=" + newValue);
        int maxValue = 0;
        if (!updateEnemy.Equals(maxCautionEnemy))
        {
            //동일하지 않다면 현재 상태의 Max값을 가지는 적의 현재 값과 비교.
            maxValue = GetCautionValue(maxCautionEnemy);
            if (newValue > maxValue)
            {
                maxValue = newValue;
                maxCautionEnemy = updateEnemy;
            }
        }
        else
        {
            // 동일하다면 그대로 갱신.
            maxValue = newValue;
        }
        // 최대값을 표시용에 알림.
        if(ui)ui.BroadcastMessage("OnUpdateCaution", maxValue, SendMessageOptions.DontRequireReceiver);
    }

    private int GetCautionValue(GameObject enemyObj)
    {
        if(enemyObj == null ) return 0;
        EnemyCaution enemyCauiton = enemyObj.GetComponent<EnemyCaution>();
        if (enemyCauiton) return enemyCauiton.Value();
        return 0;
    }
}
