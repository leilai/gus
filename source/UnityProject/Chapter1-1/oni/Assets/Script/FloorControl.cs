using UnityEngine;
using System.Collections;
using Script;

public class FloorControl : MonoBehaviour
{
    // 카메라
    private GameObject main_camera = null;

    // 초기위치
    private Vector3 initial_position;

    // 바닥 폭（X방향）.
    public static float WIDTH = 10.0f * 4.0f;

    // 바닥 모델의 수.
    public static int MODEL_NUM = 3;

    void Start()
    {
        // 카메라의 인스턴스를 준비한다.
        main_camera = GameObject.FindGameObjectWithTag(Tags.MAIN_CAMERA);

        initial_position = transform.position;

        GetComponent<Renderer>().enabled = SceneControl.IS_DRAW_DEBUG_FLOOR_MODEL;
    }

    void Update()
    {
        // 무한히 바닥이 반복되도록 설정한다.

#if true
        // 간단한 방법.
        // 화면 밖으로 나오게 되면 플레이어의 전방(후방)으로 워프한다.
        // 플레이어가 워프하는 경우 문제 발생.


        // 배경 전체（모든 모델을 배열）의 폭.
        //

        float total_width = WIDTH * MODEL_NUM;

        // 배경의 위치.
        Vector3 floor_position = transform.position;

        // 카메라의 위치.
        Vector3 camera_position = main_camera.transform.position;

        if (floor_position.x + total_width / 2.0f < camera_position.x)
        {
            //앞으로 워프.
            floor_position.x += total_width;

            transform.position = floor_position;
        }

        if (camera_position.x < floor_position.x - total_width / 2.0f)
        {
            // 뒤로 워프.
            floor_position.x -= total_width;

            transform.position = floor_position;
        }
#else
// プレイヤーがワープしても対応できる方法.
//플레이어가 워프에도 대응할 수있는 방법

// 背景全体（すべてのモデルを並べた）の幅.
// 배경 전체 (모든 모델을 늘어 놓은)의 폭.
		float		total_width = FloorControl.WIDTH*FloorControl.MODEL_NUM;

		Vector3		camera_position = this.main_camera.transform.position;

		float		dist = camera_position.x - this.initial_position.x;

    /**
     * 初期位置の距離を背景全体の幅で割って、四捨五入する.
	 * モデルは total_width の整数倍の位置に現れる.
     * 모델은 total_width의 정수배의 위치에 나타난다.
     * 초기 위치의 거리를 배경 전체 너비로 나누어 반올림한다.
     */

		int			n = Mathf.RoundToInt(dist/total_width);

		Vector3		position = this.initial_position;

		position.x += n*total_width;

		this.transform.position = position;
#endif
    }
}