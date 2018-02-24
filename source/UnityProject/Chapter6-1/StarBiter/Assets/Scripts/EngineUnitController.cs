using UnityEngine;
using System.Collections;

public class EngineUnitController : MonoBehaviour {
	
	private GameObject subScreenMessage;				// SubScreen의 메세지 영역
	
	void Start () 
	{
		// SubScreenMessage 인스턴스를 취득
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
	}
	
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if (
				this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
				this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
			{
				subScreenMessage.SendMessage("SetMessage", " ");
				subScreenMessage.SendMessage("SetMessage", "DESTROYED DEFENSIVE BULKHEAD." );
				subScreenMessage.SendMessage("SetMessage", " ");
			}
		}
	}
}
