using UnityEngine;
using System.Collections;

/// <summary>
/// 나머지 공기 잔량 표시.
/// </summary>
public class Airgage : MonoBehaviour {

    [SerializeField]
    private float offsetGageSize = 120.0f;  // zero로 화면
    [SerializeField]
    private Vector2 offsetPixelGage = Vector2.zero;  // zero로 화면
    [SerializeField]
    private Vector2 offsetPixelText = Vector2.zero;  // zero로 화면
    
    [SerializeField]
    private float[] airUpdateTime = new float[] {
        8.0f, 5.0f, 3.0f, 2.0f, 1.0f, 0.5f
    };  // 산소 감소 갱신 빈도.

    [SerializeField]
    private float airMax = 1000.0f;     // air의 최대값.
    [SerializeField]
    private float step = 1.0f;          // 1번에  갱신되는 감소량.

    [SerializeField]    // debug
    private float air = 0;              // 현재의 air값.

    private int damageLv = 0;           // 데미지 레벨.
    private float counter = 0;

    private bool gameover = false;

    private GameObject meterObj;
    private GameObject damageLvObj;


    void Start()
    {
        meterObj = GameObject.Find("AirgageMeter");
        damageLvObj = GameObject.Find("DamageLvText");

        // 위치조정
        float w = (float)Screen.width;
        float h = (float)Screen.height;

        float aspect = w / h;
        offsetPixelGage.x += offsetGageSize;
        offsetPixelGage.y += offsetGageSize;
        float posX = aspect * (1.0f - offsetPixelGage.x / w);
        float posY = 1.0f - offsetPixelGage.y / h;
        meterObj.transform.position = new Vector3(posX, posY, 0.0f);

        posX = 1.0f - offsetPixelText.x / w;
        posY = 1.0f - offsetPixelText.y / h;
        damageLvObj.transform.position = new Vector3(posX, posY, 0.0f);

        OnStageReset();
    }

    void Update()
    {
        // 카운터에 따른 갱신.
        if (!gameover)
        {
            counter += Time.deltaTime;
            if (counter > airUpdateTime[damageLv])
            {
                Deflate();
                counter = 0;
            }
        }
    }

    /// <summary>
    /// [BroadcastMessage]
    /// 데미지를 받았다.
    /// </summary>
    /// <param name="value">데미지양.   일반적으로1</param>
    void OnDamage(int value)
    {
        // 데미지 레벨 더하기.
        damageLv += value;
        UpdateDamageLv();
    }

    void OnAddAir(int value )
    {
        air += value;
        if (airMax < air) air = airMax;
    }

    void OnStageReset()
    {
        air = airMax;
        damageLv = 0;
        UpdateAirgage();
        UpdateDamageLv();
        gameover = false;
    }

    /// <summary>
    /// air갱신
    /// </summary>
    private void Deflate()
    {
        // 값 갱신.
        air -= step;
        if( air <= 0.0f ) {
            air = 0.0f;
            gameover = true;
        }
        // 미터에 값을 전달한다.
        UpdateAirgage();

        if (gameover)
        {
            // 산소 소멸. 게임 오버(false을 전달한다.)
            GameObject adapter = GameObject.Find("/Adapter");
            adapter.SendMessage("OnGameEnd", false);
        }
    }

    private void UpdateDamageLv()
    {
        damageLv = Mathf.Clamp(damageLv, 0, airUpdateTime.Length - 1);
        BroadcastMessage("OnDisplayDamageLv", damageLv);
    }
    private void UpdateAirgage()
    {
        float threshold = Mathf.InverseLerp(0, airMax, air);
        meterObj.SendMessage("OnDisplayAirgage", threshold);
    }

    public float Air() { return air; }
    public int DamageLevel() { return damageLv; }
}
