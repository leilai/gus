using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// SubScreen오른쪽 위의 LockSlot 표시 제어
// ----------------------------------------------------------------------------
public class LockSlot : MonoBehaviour {

    public GameObject lockSlot1;	// LockSlot1 사용중 표시를 위한 오브젝트.
    public GameObject lockSlot2;	// LockSlot2 사용중 표시를 위한 오브젝트.
    public GameObject lockSlot3;	// LockSlot3 사용중 표시를 위한 오브젝트.
    public GameObject lockSlot4;	// LockSlot4 사용중 표시를 위한 오브젝트.
    public GameObject lockSlot5;	// LockSlot5 사용중 표시를 위한 오브젝트.
    public GameObject lockSlot6;	// LockSlot6 사용중 표시를 위한 오브젝트.

    private GUITexture[] lockSlotGUITexture;	// LockSlot( LockSlot 표시를 위한 오브젝트).
	
	private bool isEnable = false;	// 표시 유효
	
	void Start ()
	{
        //  LockSlot의 메모리 영역 확보 
		lockSlotGUITexture = new GUITexture[6];

        //  LockSlot의 표시를 위한 오브젝트 모두가 준비되어 있는가?
		if (
			lockSlot1 && lockSlot2 && lockSlot3 &&
			lockSlot4 && lockSlot5 && lockSlot6 )
		{
			isEnable = true;
		
			// LockSlot의 인스턴스 취득.
			lockSlotGUITexture[0] = lockSlot1.GetComponent<GUITexture>();
			lockSlotGUITexture[1] = lockSlot2.GetComponent<GUITexture>();
			lockSlotGUITexture[2] = lockSlot3.GetComponent<GUITexture>();
			lockSlotGUITexture[3] = lockSlot4.GetComponent<GUITexture>();
			lockSlotGUITexture[4] = lockSlot5.GetComponent<GUITexture>();
			lockSlotGUITexture[5] = lockSlot6.GetComponent<GUITexture>();
			
			// 초기값 표시
			for( int i = 0; i < lockSlotGUITexture.Length; i++ )
			{
				lockSlotGUITexture[i].enabled = false;
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // 록온 수만큼  LockSlot을 사용중으로 표시.   
	// ------------------------------------------------------------------------
	public void SetLockCount( int lockCount )
	{
		if ( isEnable )
		{
			for( int i = 0; i < lockSlotGUITexture.Length; i++ )
			{
				if ( i < lockCount )
				{
					lockSlotGUITexture[i].enabled = true;
				}
				else
				{
					lockSlotGUITexture[i].enabled = false;
				}
			}			
		}
	}
}
