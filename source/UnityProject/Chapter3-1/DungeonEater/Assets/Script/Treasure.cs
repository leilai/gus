using UnityEngine;
using System.Collections;

public class Treasure : MonoBehaviour {
	public GameObject m_pickupEffect;	// 주웠을 때의 효과.
	public AudioClip m_SEPickuped; 		// 주웠을 때의 SE.
	public AudioClip m_SEAppear; 		// 주웠을 때의 SE.
	public BillBoradText m_scoreBorad;  // 주웠을 때의 득점표시.
	public int m_point = 1000;			// 주웠을 때의 득점.
	
	// 소멸시간.
	public float m_lifeTime = 10.0f;
	
	// 초기화.
	void Start () {
		AudioChannels audio = FindObjectOfType(typeof(AudioChannels)) as AudioChannels;
		if (audio != null)
			audio.PlayOneShot(m_SEAppear,1.0f,0.0f);
		Destroy(gameObject,m_lifeTime);
	}
	
	// Update
	void Update () {
	}
	
	public void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerController>() != null) {  // todo: 플레이어가 어떤지에 대해서는 컴포넌트로 판단하면 되는지?
			Score.AddScore(m_point);
			GameObject borad = (GameObject)Instantiate(m_scoreBorad.gameObject,transform.position + new Vector3(0,2.0f,0),Quaternion.identity);
			borad.GetComponent<BillBoradText>().SetText(m_point.ToString());

			// 효과 발생.
			GameObject o = (GameObject)Instantiate(m_pickupEffect.gameObject,transform.position  + new Vector3(0,1.0f,0),Quaternion.identity);
			
			// 효과음.
			(FindObjectOfType(typeof(AudioChannels)) as AudioChannels).PlayOneShot(m_SEPickuped,1.0f,0.0f);
			Destroy(o,3.0f);
			Destroy(gameObject);
		}
	}
}
