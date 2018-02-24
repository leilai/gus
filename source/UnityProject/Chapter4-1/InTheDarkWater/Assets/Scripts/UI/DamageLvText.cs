using UnityEngine;
using System.Collections;

/// <summary>
/// Airgage 아래에 데미지 레벨 표시
/// </summary>
public class DamageLvText : MonoBehaviour {

    [SerializeField]
    private int disitSize = 1;

    /// <summary>
    /// [SendMessage]표시 갱신
    /// </summary>
    /// <param name="value"></param>
    void OnDisplayDamageLv(int value)
    {
        guiText.text = value.ToString("D" + disitSize);
    }
	
}
