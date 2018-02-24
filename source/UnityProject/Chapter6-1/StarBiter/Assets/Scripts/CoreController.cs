using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// BOSS Core 파괴되는 경우의 메세지 표시.
// ----------------------------------------------------------------------------
public class CoreController : MonoBehaviour {
	
	private GameObject subScreenMessage;	// SubScreen의 메세지 영역
	
	void Start () {
		
		// SubScreenMessage의 인스턴스를 취득.
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
	}
	
	// ------------------------------------------------------------------------
	// BOSS Core 가 파괴되는 경우의 처리         
	// ------------------------------------------------------------------------
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if (
				this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
				this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
			{
				subScreenMessage.SendMessage("SetMessage", " ");
				subScreenMessage.SendMessage("SetMessage", "DEFEATED SPIDER-TYPE." );
				subScreenMessage.SendMessage("SetMessage", "MISSION ACCOMPLISHED." );
				subScreenMessage.SendMessage("SetMessage", " ");
			}
		}
	}
	
}
