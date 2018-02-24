using UnityEngine;
using System.Collections;

public class AttackColliderControl : MonoBehaviour {

	public PlayerControl	player = null;

	// 공격 판정 발생중?.
	private bool		is_powered = false;

	// -------------------------------------------------------------------------------- //

	void Start ()
	{
		this.SetPowered(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
	}

	// OnTriggerEnter는 콜리전끼리 연결되는 순간에만 불러올 수 있기 때문에 
	// 공격판정의 원형이 발생되었을 때에 도깨비가 원형 내에 들어오게 되면
	// 제대로 처리할 수 없다.
	//void OnTriggerEnter(Collider other)
	void OnTriggerStay(Collider other)
	{
		do {

			if(!this.is_powered) {

				break;
			}

			if(other.tag != "OniGroup") {
	
				break;
			}

			OniGroupControl	oni = other.GetComponent<OniGroupControl>();

			if(oni == null) {

				break;
			}

			//

			oni.OnAttackedFromPlayer();

			// 『공격할 수 없는 사이』타이머를 리세팅한다.（바로 공격 가능 상태로 설정한다）.
			this.player.resetAttackDisableTimer();

            // 공격 HitEffect를 재생한다.
			this.player.playHitEffect(oni.transform.position);

            // 공격 HitSound를 재생한다.
			this.player.playHitSound();

		} while(false);
	}

	public void	SetPowered(bool sw)
	{
		this.is_powered = sw;

		if(SceneControl.IS_DRAW_PLAYER_ATTACK_COLLISION) {

			this.GetComponent<Renderer>().enabled = sw;
		}
	}
}
