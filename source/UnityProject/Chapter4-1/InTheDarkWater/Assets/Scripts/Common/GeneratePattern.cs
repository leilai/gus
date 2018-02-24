using UnityEngine;
using System.Collections;

/// <summary>
/// 생성 패턴.
/// </summary>
public class GeneratePattern : MonoBehaviour {

    // 발생시간.
    [SerializeField]
    private float[] timing = new float[] { };
    // 생성 상황 변경.
    [SerializeField]
    private GenerateParameter[] variation = new GenerateParameter[]{ };

    private int current = 0;
    private int validSize;

    private RandomGenerator generator = null;

	void Start () 
    {
        validSize = (timing.Length > variation.Length) ? variation.Length : timing.Length;

        generator = GetComponent<RandomGenerator>();

        // 카운트 시작.
        StartCoroutine("Counter");
    }

    private IEnumerator Counter()
    {
        yield return new WaitForSeconds(timing[current]);

        // 파라미터 변경.
        Debug.Log("Change Generate Parameter:time=" + timing[current]);
        if (generator) generator.SetParam(variation[current]);

        current++;
        if (current < validSize)
        {
            // 카운트 재시작.
            StartCoroutine("Counter");
        }
    }
}
