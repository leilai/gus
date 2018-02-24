using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// SubScreen 오른쪽 위의  LockBonus 표시 제어
// ----------------------------------------------------------------------------
public class LockBonus : MonoBehaviour {

    public Texture2D lockBonus0;			// lockBonus x 0  화상.
    public Texture2D lockBonus2;			// lockBonus x 2  화상.
    public Texture2D lockBonus4;			// lockBonus x 4  화상.
    public Texture2D lockBonus8;			// lockBonus x 8  화상.
    public Texture2D lockBonus16;			// lockBonus x 16 화상.
    public Texture2D lockBonus32;			// lockBonus x 32 화상.
    public Texture2D lockBonus64;			// lockBonus x 64화상.

    private GUITexture lockBonusGUITexture;	// LockBonus( LockBonus표시를 위한 오브젝트).
	
	private bool isEnable = false;			// 표시 유효
	
	void Start ()
	{
		// LockBonus의 인스턴스를 취득
		lockBonusGUITexture =
			GameObject.FindGameObjectWithTag("LockBonus")
				.GetComponent<GUITexture>();

        //  LockBonus의 화상을 모두 준비하였는가?
		if (
			lockBonus0 && lockBonus2 && lockBonus4 &&
			lockBonus8 && lockBonus16 && lockBonus32 &&
			lockBonus64 )
		{
			isEnable = true;
			
			// 초기값 표시
			lockBonusGUITexture.texture = lockBonus0;
		}
		
	}
	
	// ------------------------------------------------------------------------
    // 지정한  LockBonus의 화상 표시  
	// ------------------------------------------------------------------------
	public void SetLockCount( int lockCount )
	{
		if ( isEnable )
		{
			switch( lockCount )
			{
			case 0:
				lockBonusGUITexture.texture = lockBonus0;
				break;
			case 1:
				lockBonusGUITexture.texture = lockBonus2;
				break;
			case 2:
				lockBonusGUITexture.texture = lockBonus4;
				break;
			case 3:
				lockBonusGUITexture.texture = lockBonus8;
				break;
			case 4:
				lockBonusGUITexture.texture = lockBonus16;
				break;
			case 5:
				lockBonusGUITexture.texture = lockBonus32;
				break;
			case 6:
				lockBonusGUITexture.texture = lockBonus64;
				break;
			default:
				break;
			}
		}
	}
}
