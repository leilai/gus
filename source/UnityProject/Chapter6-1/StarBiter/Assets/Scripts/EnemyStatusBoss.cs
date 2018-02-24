using UnityEngine;
using System.Collections;

public class EnemyStatusBoss : EnemyStatus {
	
	public bool isBoss = false;							// 자신은 BOSS(=true).
	public int bossGuardLimit = 0;						// 이 수치 이상의 Life가 있는 경우에는 공격을 방어한다.
	private bool canGuard = false;						// 공격을 방어할 수 있다.
	
	private GameObject boss;							// Boss의 인스턴스
	
	public override void StartSub()
	{
		// BOSS용
		if ( isBoss )
		{
			// BOSS를 추종한다.
			SetIsFollowingLeader( true );
			
			// BOSS의 인스턴스를 취득
			boss = GameObject.FindGameObjectWithTag("Boss");
			
			// 방어 가능한지 확인
			if ( CanGuard() )
			{
				canGuard = true;
			}
		}
	}
	
	public override void UpdateSub()
	{
		// BOSS용
		if ( isBoss && canGuard )
		{
			if ( !CanGuard() )
			{
				canGuard = false;
				
				// 콜리전을 유효화한다.
				EnableCollider();
			}
		}
	}
	
	public override void DestroyEnemySub()
	{
		// 보스의 경우에는 
		if ( isBoss )
		{
			if ( transform.parent )
			{
				transform.parent.SendMessage( "SetLockonBonus", lockonBonus );
				transform.parent.SendMessage( "SetIsBreak", true );
			}
		}
	}
	
	private bool CanGuard()
	{
		if ( !isBoss )
		{
			return false;
		}
		
		int bossLife = boss.GetComponent<EnemyStatus>().GetLife();

		if ( bossLife >= bossGuardLimit )
		{
			return true;
		}
		return false;
	}
	
	protected override void DestroyEnemyEx()
	{
		// 아무것도 처리하지 않는다.
	}
	
	private void EnableCollider()
	{
		// 콜리전을 유효화 한다.(자식 오브젝트의 Collider를 모두 유효화한다.).
		Transform[] children = this.GetComponentsInChildren<Transform>();
  		foreach ( Transform child in children )
		{
			if ( child.tag == "Enemy" )
			{
				if ( child.GetComponent<SphereCollider>() )
				{
					// collider를 유효화한다. 
                    child.GetComponent<SphereCollider>().enabled = true;
				}
			}
		}
	}
}
