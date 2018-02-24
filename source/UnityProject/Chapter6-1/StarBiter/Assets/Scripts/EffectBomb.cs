using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 폭발 효과와 소리 재생
// ----------------------------------------------------------------------------
public class EffectBomb : MonoBehaviour {
	
	private ParticleSystem[] effects = new ParticleSystem[2];	// 재생할 효과. 
	private int endOfPlayingCount = 0;					// 재생 종료한 수
	private int playingCount = 0;						// 재생 시작한 수
	
	private AudioMaker bombAudioMaker;					// 폭발음 메이커
	private BattleSpaceController battleSpaceContoller;	// 전투 공간
	
	private float speed = 0f;
	private bool isMoving = false;
	
	void Start () {
		
		// 전투 공간의 인스턴스를 취득
		battleSpaceContoller =
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
		// 폭발음 메이커의 인스턴스를 취득.
		bombAudioMaker = 
			GameObject.FindGameObjectWithTag("EffectBombAudioMaker")
				.GetComponent<AudioMaker>();
		
		// 소리 재생
		if ( bombAudioMaker )
		{
			bombAudioMaker.Play( this.gameObject );
		}
		
		// 자식 오브젝트에 존재하는 모든 효과 오브젝트를 취득.
		effects = GetComponentsInChildren<ParticleSystem>();
		
		// 모든 효과를 재생
		for( int i = 0; i < effects.Length; i++ )
		{
			if ( effects[i] )
			{
				effects[i].Play();
				playingCount++;
			}
		}
	}
	
	void Update () 
	{	
		// 폐기 전의 동작을 더함
		if ( isMoving )
		{
			transform.Translate( transform.forward * speed * Time.deltaTime );
			// 서서히 스피드가 감소.
			if ( speed > 0 )
			{
				speed -= 0.1f;
			}
		}
		
		// 전투 공간의 스크롤 방향을 추가한다.
		transform.position -= battleSpaceContoller.GetAdditionPos();
		
		// 재생 종료한 효과를 카운트.
		for( int i = 0; i < effects.Length; i++ )
		{
			if ( !effects[i].isPlaying )
			{
				endOfPlayingCount++;
			}
		}
				
		// 전부 재생이 종료된 경우, 오브젝트를 폐기한다.
		if ( endOfPlayingCount >= playingCount )
		{
			Destroy( this.gameObject );
		}
		
	}
	
	// ------------------------------------------------------------------------
	// 효과가 작동하는 스피드를 설정한다.
	//  - 폐기 전 적기의 스피드를 이용한다.
	// ------------------------------------------------------------------------
	public void SetIsMoving( float speed )
	{
		this.speed = speed * 40f;
		if ( this.speed > 5f )
		{
			this.speed = 5f;
		}
		this.isMoving = true;
	}

}
