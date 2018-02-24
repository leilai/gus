using UnityEngine;
using System.Collections;

/// <summary>
/// 탐지기 아래에 등장하는 메세지
/// </summary>
public class SonarMessage : MonoBehaviour {

    [SerializeField]
    private string enemyDestroyed = "The enemy is destroyed!";
    [SerializeField]
    private string itemFound = "You found the item!";
    [SerializeField]
    private string itemLost = "The item was lost...";

    void Start() 
    {
        guiText.text = "";
        guiText.enabled = false;
    }

    /// <summary>
    /// 오브젝트의 충돌
    /// /// </summary>
    /// <param name="tag"></param>
    void OnHitObject( string tag )
    {
        if (tag.Equals("Enemy")) guiText.text = enemyDestroyed;
        else if (tag.Equals("Item")) guiText.text = itemFound;
        // 점멸 시작
        SendMessage("OnStartTextBlink");
    }

    /// <summary>
    /// 오브젝트 Lost
    /// </summary>
    /// <param name="tag"></param>
    void OnLostObject( string tag )
    {
        if (tag.Equals("Item"))
        {
            guiText.text = itemLost;
        }
        // 점멸 시작
        SendMessage("OnStartTextBlink");
    }


    void OnEndTextBlink()
    {
        guiText.enabled = false;
    }
}
