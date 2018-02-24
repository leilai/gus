using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// PlayerShotMaker 작성
// ----------------------------------------------------------------------------
public class PlayerShotMaker : MonoBehaviour {
	
	public float fireInterval = 0.1f;		// 총알과 총알의 간격
	public float fireSetInterval = 0.3f;	// 다음의 3탄을 발포하기 까지의 대기 시간.
	public int fireOrderMax = 1;			// 발사 지시의 최대수
	
	public GameObject shot;					// 총알
	
	public GameObject effectShot;
	
	private GameObject effectShot1;
	private GameObject effectShot2;
	private GameObject effectShot3;
	private ParticleSystem particleEffectShot1;		// 발사 효과
    private ParticleSystem particleEffectShot2;		// 발사 효과
    private ParticleSystem particleEffectShot3;		// 발사 효과
	
	private ArrayList fireOrders = new ArrayList();	// 발사 지시
	
	private bool isFiring = false;			// 발포중
	
	void Start () 
	{
        // 발사 효과 인스턴스를 취득           
		effectShot1 = Instantiate( effectShot, Vector3.zero, new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
		particleEffectShot1 = effectShot1.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
		effectShot2 = Instantiate( effectShot, Vector3.zero, new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
		particleEffectShot2 = effectShot2.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
		effectShot3 = Instantiate( effectShot, Vector3.zero, new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
		particleEffectShot3 = effectShot3.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
	}
	
	void Update ()
	{
		
		ReadFireOrder();
	}
	
	// Fire -> Memory
	public void SetFireOrder()
	{
		if ( fireOrders.Count < fireOrderMax )
		{
			fireOrders.Add( true );
		}
	}
	
	// Memory -> Fire
	private void ReadFireOrder()
	{
		if ( fireOrders.Count > 0 )
		{
			if ( !isFiring )
			{
				isFiring = true;
				fireOrders.RemoveAt(0);
				StartCoroutine( "Firing" );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 일정 간격으로 탄을 3개씩 발포한다.
	// ------------------------------------------------------------------------
	IEnumerator Firing()
	{
		// 총알 오브젝트가 존재하는가.
		if ( shot )
		{
			// 총알 1 작성
			Instantiate( shot, transform.position, transform.rotation );
			effectShot1.transform.rotation = transform.rotation;
			effectShot1.transform.position = effectShot1.transform.forward * 1.5f;
			// 효과의 각도를 수정
			//  - 소재의 방향 수정. 90f를 더하는 것은 단순히 소재의 방향을 수정하기 위한 것.
			//  - 57.29578f로 나누면 정확히 플레이어의 전방 위치가 된다.(ParticleSystem의 startRotation은 이러한 처리를 하지 않으면 벗어난다.).
			particleEffectShot1.startRotation = ( transform.rotation.eulerAngles.y + 90f ) / 57.29578f;
			// 효과 재생
			particleEffectShot1.Play();
			
			// 일정 시간 대기
			yield return new WaitForSeconds( fireInterval );

            // 총알 2 작성
			Instantiate( shot, transform.position, transform.rotation );
			effectShot2.transform.rotation = transform.rotation;
			effectShot2.transform.position = effectShot2.transform.forward * 1.5f;
            // 효과의 각도를 수정
            //  - 소재의 방향 수정. 90f를 더하는 것은 단순히 소재의 방향을 수정하기 위한 것.
            //  - 57.29578f로 나누면 정확히 플레이어의 전방 위치가 된다.(ParticleSystem의 startRotation은 이러한 처리를 하지 않으면 벗어난다.).
			particleEffectShot2.startRotation = ( transform.rotation.eulerAngles.y + 90f ) / 57.29578f;
            // 효과 재생
			particleEffectShot2.Play();

            // 일정 시간 대기
			yield return new WaitForSeconds( fireInterval );

            // 총알 3 작성
			Instantiate( shot, transform.position, transform.rotation );
			effectShot3.transform.rotation = transform.rotation;
			effectShot3.transform.position = effectShot3.transform.forward * 1.5f;
            // 효과의 각도를 수정
            //  - 소재의 방향 수정. 90f를 더하는 것은 단순히 소재의 방향을 수정하기 위한 것.
            //  - 57.29578f로 나누면 정확히 플레이어의 전방 위치가 된다.(ParticleSystem의 startRotation은 이러한 처리를 하지 않으면 벗어난다.).
			particleEffectShot3.startRotation = ( transform.rotation.eulerAngles.y + 90f ) / 57.29578f;
            // 효과 재생
			particleEffectShot3.Play();

            // 다음 발사까지 일정 시간 대기
			yield return new WaitForSeconds( fireSetInterval );
			
			// 발사종료
			isFiring = false;
		}
	}

}
