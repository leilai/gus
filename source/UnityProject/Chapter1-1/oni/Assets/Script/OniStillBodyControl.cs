using UnityEngine;
using System.Collections;

public class OniStillBodyControl : MonoBehaviour {


	public OniEmitterControl	emitter_control = null;

	void 	Start()
	{
	}
	
	void 	Update()
	{
	}

	void	OnCollisionEnter(Collision other)
	{
		if(other.gameObject.tag == "OniYama") {

			// 도깨비 산에 충돌하였을 때의 음악을 재생한다.
			this.emitter_control.PlayHitSound();

			// 여기에서 직접 SE를 재생하게 되면 짧은 간격의 음악이 되고, 소리가 겹쳐지게 되어
			// 잘 들리지 않게 되기 때문에 OniEmitterControl 으로 적절한 간격으로 음악이 재생되도록 한다.
			// .
		}

		if(other.gameObject.tag == "Floor") {

			// 물리계산을 멈추기 위해 rigidbody 의 컴포넌트를 삭제한다.
			// 무리일 수도 있지만 Sleep() 가 느려지게 된다.
			Destroy(this.GetComponent<Rigidbody>());
		}
	}
}
