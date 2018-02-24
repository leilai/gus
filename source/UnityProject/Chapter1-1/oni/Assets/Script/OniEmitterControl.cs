using UnityEngine;
using System.Collections;

public class OniEmitterControl : MonoBehaviour {

	public GameObject[]	oni_prefab;

	// SE.
	public AudioClip	EmitSound = null;		// 멀리서 들려오는 음（『퓨~』）.
	public AudioClip	HitSound = null;		// 도깨비가 도깨비 산에 당도했을 때의 음악.

	// 마지막에 생성되는 도깨비.
	private GameObject	last_created_oni = null;

	private static float	collision_radius = 0.25f;

	// 생성할 도개비의 수(나머지).
	// 실제 값은 결과에 따라 바뀐다.
	public int		oni_num = 2;

	public bool		is_enable_hit_sound = true;

	// -------------------------------------------------------------------------------- //

	void Start()
	{
		this.GetComponent<AudioSource>().PlayOneShot(this.EmitSound);
	}

	void 	Update()
	{

		do {

			if(this.oni_num <= 0) {

				break;
			}

			// 마지막에 생성되는 도깨비가 충분히 멀어질 때까지 기다린다.
			// （같은 위치에 중복되도록 생성하면 콜리전에 의해 크게 보일 수 있다）.
			if(this.last_created_oni != null) {

				if(Vector3.Distance(this.transform.position, last_created_oni.transform.position) <= OniEmitterControl.collision_radius*2.0f) {

					break;
				}
			}

			Vector3	initial_position = this.transform.position;

			initial_position.y += Random.Range(-0.5f, 0.5f);
			initial_position.z += Random.Range(-0.5f, 0.5f);

			// 회전（랜덤하게 보이면 충분）.
			Quaternion	initial_rotation;

			initial_rotation = Quaternion.identity;
			initial_rotation *= Quaternion.AngleAxis(this.oni_num*50.0f, Vector3.forward);
			initial_rotation *= Quaternion.AngleAxis(this.oni_num*30.0f, Vector3.right);

			GameObject oni = Instantiate(this.oni_prefab[this.oni_num%2], initial_position, initial_rotation) as GameObject;	

			//

			oni.GetComponent<Rigidbody>().velocity        = Vector3.down*1.0f;
			oni.GetComponent<Rigidbody>().angularVelocity = initial_rotation*Vector3.forward*5.0f*(this.oni_num%3);

			oni.GetComponent<OniStillBodyControl>().emitter_control = this;

			//

			this.last_created_oni = oni;

			this.oni_num--;

		} while(false);

	}

	// 도깨비가 도깨비 산에 당도했을 때에 울리는 음악.
	//
	// 짧은 간격으로 음악이 울리게 되면 혼란스워질 수 있으므로 일정간격으로
	// 울리도록 조정한다.
	//
	public void	PlayHitSound()
	{
		if(this.is_enable_hit_sound) {

			bool	to_play = true;
	
			if(this.GetComponent<AudioSource>().isPlaying) {
	
				if(this.GetComponent<AudioSource>().time < this.GetComponent<AudioSource>().clip.length*0.75f) {
	
					to_play = false;
				}
			}
	
			if(to_play) {
	
				this.GetComponent<AudioSource>().clip = this.HitSound;
				this.GetComponent<AudioSource>().Play();
			}
		}
	}

}
