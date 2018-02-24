using UnityEngine;
using System.Collections;

public class FruitEffect : MonoBehaviour {

	void Start ()
	{

	}
	
	void Update ()
	{

		// 재생이 종료되면 삭제
		if(!this.GetComponentInChildren<ParticleSystem>().isPlaying) {

			Destroy(this.gameObject);
		}
	}
}
