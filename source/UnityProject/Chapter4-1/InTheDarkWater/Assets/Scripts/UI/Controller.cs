using UnityEngine;
using System.Collections;

/// <summary>
/// 컨트롤러
/// </summary>
public class Controller : MonoBehaviour {

    [SerializeField]
    private Texture guiCompass = null;
    [SerializeField]
    private float aspect = 0.5f;

    private bool enable = false;

    private Matrix4x4 tmpMat;
    private float angleY;

    private Rect textureRect;
    private Vector3 pivotPoint;

    void Start () {
        angleY = 0.0f;
        float w = Screen.width * aspect;
        float h = w;
        pivotPoint  = new Vector2(Screen.width * 0.5f, (float)Screen.height);
        textureRect = new Rect(pivotPoint.x - w * 0.5f, pivotPoint.y - h * 0.5f, w, h);
	}

    void OnGUI()
    {
        if (!enable) return;
        // 텍스처 회전은 GUIUtility.RotateAroundPivot가 아니면 회전할 수 없다.
        tmpMat = GUI.matrix;    // 일시대피
            GUIUtility.RotateAroundPivot(angleY, pivotPoint);
            GUI.DrawTexture(textureRect, guiCompass);
        }
        GUI.matrix = tmpMat;    // 되돌아온다
    }

    void OnStageReset()
    {
        angleY = 0.0f;
    }

    public void Enable( bool flag )   {  enable = flag;   }
    public void SetAngle(float angle) {  angleY = angle;  }
}
