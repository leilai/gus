using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
	public GameCtrl m_gameCtrl;		// 게임.
	public GameObject m_sword;		// 플레이어의 검.
	public GameObject m_scoreBorad;	// 스코어 표시 오브젝트.
	private AudioChannels m_audio;	// 오디오.
	public AudioClip m_swordAttackSE;	// 공격SE.
	public GameObject SWORD_ATTACK_OBJ;  // 공격 범위 오브젝트.
	
	private bool m_equiped = false;  // 검 장비중.
	private Transform m_target;  // 공격대상.
	
	// 득점.
	private const int POINT = 500;
	private const int COMBO_BONUS = 200;
	private int m_combo = 0;
	
	// 초기화.
	void Start () {
		m_equiped = false;
		m_sword.renderer.enabled = false;

		// 애니메이션 합성을 시작하는 노드.
		Transform mixTransform = transform.Find("root/hip/mune");

		// 검을 휘두른다.
		animation["up_sword_action"].layer = 1;
		animation["up_sword_action"].AddMixingTransform(mixTransform);		

		// 검을 가슴 앞으로 든다.
		animation["up_sword"].layer = 1;
		animation["up_sword"].AddMixingTransform(mixTransform);			

		m_audio = FindObjectOfType(typeof(AudioChannels)) as AudioChannels;
		m_combo = 0;
	}
	
	// 스테이지 시작시.
	void OnStageStart()
	{
		m_equiped = false;
		m_sword.renderer.enabled = false;
	}
	
	// 검을 줍는다.
	void OnGetSword()
	{
		if (!m_equiped) {
			m_sword.renderer.enabled = true;
			m_equiped = true;
			animation.CrossFade("up_sword",0.25f);
		} else {
			BillBoradText borad = ((GameObject)Instantiate(m_scoreBorad,transform.position + new Vector3(0,2.0f,0),Quaternion.identity)).GetComponent<BillBoradText>();
			int point = POINT + COMBO_BONUS * m_combo;
			borad.SetText(point.ToString());
			Score.AddScore(point);
			m_combo++;
		}
	}
	
	void Remove()  
	{
		m_sword.renderer.enabled = false;
		m_equiped = false;
//		animation.CrossFade("up_idle",0.25f);
		animation.Stop("up_sword_action");
		animation.Stop("up_sword");
		m_combo = 0;
	}

	
	// 자동공격 한다.
	public void AutoAttack(Transform other)
	{
		if (m_equiped) {
			m_target = other;
			StartCoroutine("SwordAutoAttack");
		}
	}
	
	// 공격 가능한가?.
	public bool CanAutoAttack()
	{
		if (m_equiped)
			return true;
		else
			return false;
	}
		
	
	IEnumerator SwordAutoAttack()
	{
		m_gameCtrl.OnAttack();
		// 휘두른다.
		transform.LookAt(m_target.transform);
		yield return null;
		// 공격.
		animation.CrossFade("up_sword_action",0.2f);
		yield return new WaitForSeconds(0.3f);
		m_audio.PlayOneShot(m_swordAttackSE,1.0f,0.0f);		
		yield return new WaitForSeconds(0.2f);
		Vector3 projectilePos;
		projectilePos = transform.position + transform.forward * 0.5f;
		Instantiate(SWORD_ATTACK_OBJ,projectilePos,Quaternion.identity);
		yield return null;
		// 방향을 원래대로 되돌린다.
		Remove();
		m_gameCtrl.OnEndAttack();
	}
}
