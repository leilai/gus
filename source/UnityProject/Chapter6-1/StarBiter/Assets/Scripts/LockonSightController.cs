using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
//  LockonSightController
// ----------------------------------------------------------------------------
public class LockonSightController : MonoBehaviour {

    public GameObject lockonEnemy;				//  Lockon한 적기
    public bool isEnabled = false;				// LockonSight 유효
	
	void Update ()
	{
        // Lockon한 적기가 존재하는 경우에는  LockonSight 적기를 추종한다. 
		if ( lockonEnemy )
		{
			// 적기를 추종
			transform.position = new Vector3(
				lockonEnemy.transform.position.x,
				lockonEnemy.transform.position.y + 1f,
				lockonEnemy.transform.position.z );
		}

        // Lockon한 적기가 존재하지 않는 경우에는 파괴했다고 간주하는 LockonSight를 제거한다. 
		if ( !lockonEnemy )
		{
			if ( isEnabled )
			{
				Destroy( this.gameObject );
			}
		}
	}
	
	// ------------------------------------------------------------------------
    // Lockon 대상의 적기를 저장한다.
	// ------------------------------------------------------------------------
	private void SetLockonEnemy( GameObject lockonEnemy )
	{
		this.lockonEnemy = lockonEnemy;
		isEnabled = true;
	}
	
	// ------------------------------------------------------------------------
    // LockonSight를 제거 
	// ------------------------------------------------------------------------
	public void Destroy()
	{
		Destroy( this.gameObject );
	}
}
