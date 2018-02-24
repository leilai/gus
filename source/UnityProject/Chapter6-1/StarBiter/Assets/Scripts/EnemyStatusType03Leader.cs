using UnityEngine;
using System.Collections;

public class EnemyStatusType03Leader : EnemyStatus {
	
	public bool isType03Leader = false;			// 자신은 TYPE 03 Leader(=true).
	
	public override void StartSub()
	{
		
		// Type03Leader용.
		if ( isType03Leader )
		{
			// 리더를 추종한다.
			SetIsFollowingLeader( true );
		}
	}
	
	public override void DestroyEnemySub()
	{
		// Type03Leader용.
		if ( isType03Leader )
		{
			if ( transform.parent )
			{
				transform.parent.SendMessage( "SetLockonBonus", lockonBonus );
				transform.parent.SendMessage( "SetIsBreak", true );
			}
		}
	}
	
}
