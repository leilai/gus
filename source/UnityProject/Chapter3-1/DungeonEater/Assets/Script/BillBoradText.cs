using UnityEngine;
using System.Collections;

public class BillBoradText : MonoBehaviour {
	public float m_lifeTime = 3.0f;	// 출현시간.
	public float SPEED = 0.1f; // 상승 스피드.
	
	private Vector3 m_position;
	private float m_offsetY;

	// Use this for initialization
	void Start () {
		m_position = transform.position;
		m_offsetY = 0;
	}
	
	public void SetText(string text)
	{
		GetComponent<GUIText>().text = text;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Camera.main.WorldToViewportPoint(m_position) + new Vector3(0,m_offsetY,0);
		
		m_offsetY += SPEED * Time.deltaTime;
	
		m_lifeTime -= Time.deltaTime;
		if (m_lifeTime < 0.0f)
			Destroy(gameObject);
	}
}
