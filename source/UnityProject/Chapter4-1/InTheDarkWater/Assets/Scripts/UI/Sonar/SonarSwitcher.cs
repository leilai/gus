using UnityEngine;
using System.Collections;

/// <summary>
///  PassiveSonar와 ActiveSonar의 변경
/// </summary>
public class SonarSwitcher : MonoBehaviour
{

    [SerializeField]
    private GameObject activeObj = null;
    [SerializeField]
    private GameObject passiveObj = null;
    [SerializeField]
    private int offsetPixel = 10;   // 왼쪽부터 위치 오프셋
    [SerializeField]
    private float aspect = 0.4f;    // 화면에 대한 사이즈비율
    [SerializeField]
    private int cameraRayoutPixel = 8;  // 외관상, 텍스처 크기보다 조금 작은 크기로 카메라 위치를 정한다.

    private GameObject currentObj = null;

    private float radius = 0;

    public enum SonarMode {
        None,
        PassiveSonar,
        ActiveSonar
    }
    private SonarMode mode = SonarMode.None;

	void Start () 
    {
        InitTexturePos();
    }

    void Update()
    {
        // 누르고 있는 동안만 ActiveSonar
        if (Input.GetKeyDown(KeyCode.Space)) SetMode(SonarMode.ActiveSonar);
        if (Input.GetKeyUp(KeyCode.Space)) SetMode(SonarMode.PassiveSonar);
    }

    void SetMode( SonarMode mode_ )
    {
        if (mode == mode_) return;

        // 스크린 크기에 맟춘 크기, 위치 조정
        if (currentObj != null)
        {
            Destroy(currentObj);
        }

        switch (mode_)
        {
            case SonarMode.ActiveSonar:
                CreateSonar(activeObj);
                break;

            case SonarMode.PassiveSonar:
                CreateSonar(passiveObj);
                break;

            default:
                guiTexture.enabled = false;
                break;
        }

        mode = mode_;
    }

    void OnGameStart()
    {
    }

    void OnAwakeStage(int index)
    { 
        InitCameraPos();
    }

    void OnStageReset()
    {
//        InitCameraPos();
        Debug.Log("OnStageReset");
        // PassiveSonar가 기본값
        SetMode(SonarMode.PassiveSonar);
    }

    private void InitTexturePos()
    {
        float size = Screen.height * aspect;

        guiTexture.enabled = true;
        guiTexture.pixelInset = new Rect(offsetPixel, Screen.height - offsetPixel - size, size, size);
    }
    private void InitCameraPos()
    {
        // 카메라에 텍스처 크기를 전한다.
        Rect cameraRect = new Rect(guiTexture.pixelInset);
        cameraRect.x += cameraRayoutPixel;
        cameraRect.y += cameraRayoutPixel;
        cameraRect.width -= cameraRayoutPixel * 2;
        cameraRect.height -= cameraRayoutPixel * 2;

        GameObject cameraObj = GameObject.Find("/Field/Player/SonarCamera");
        if (cameraObj)
        {
            SonarCamera sonarCamera = cameraObj.GetComponent<SonarCamera>();
            sonarCamera.SetRect(cameraRect);
            radius = sonarCamera.Radius();
        }
//        SetSize(activeObj);
//        SetSize(passiveObj);
    }

    private void SetSize(GameObject obj)
    {
        ActiveSonar activeSonar = currentObj.GetComponent<ActiveSonar>();
        if (activeSonar) activeSonar.SetMaxRadius(radius);
        SonarEffect effecter = currentObj.GetComponent<SonarEffect>();
        if (effecter) effecter.Init(guiTexture.pixelInset);
    }

    private void CreateSonar( GameObject obj ) 
    {
        currentObj = Object.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
        currentObj.transform.parent = transform;

        SetSize(currentObj);
   }
}
