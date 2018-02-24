using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 소리를 재상할 오브젝트를 폐기한다.
//  - (노이즈가 들어갈 수 있으므로)재생 종료후 폐기한다.
// ----------------------------------------------------------------------------
public class AudioBreaker : MonoBehaviour {
	
	private bool isDestroy = false;
	
	void Update ()
	{
		// 폐기할 때 한정.
		if ( isDestroy )
		{
			// 소리의 재생이 종료되었는지 확인.
			if ( !audio.isPlaying )
			{
				//폐기한다.
				Destroy( this.gameObject );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 폐기시작
	// ------------------------------------------------------------------------
	public void SetDestroy()
	{
		isDestroy = true;
	}
}
