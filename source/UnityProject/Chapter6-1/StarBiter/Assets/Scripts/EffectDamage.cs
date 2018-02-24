using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 데미지 효과와 소리 재생
// ----------------------------------------------------------------------------
public class EffectDamage : MonoBehaviour {

	private ParticleSystem[] effects = new ParticleSystem[2];	// 재생할 효과   
	private int endOfPlayingCount = 0;					// 재생 종료한 수
	private int playingCount = 0;						// 재생 새작한 수
	
	private AudioMaker damageAudioMaker;				// 데미지 소리 메이커
	
	void Start ()
	{	
		// 데미지 소리 인스턴스를 취득.
		damageAudioMaker =
			GameObject.FindGameObjectWithTag("EffectDamageAudioMaker")
				.GetComponent<AudioMaker>();
		
		// 音の再生.
		if ( damageAudioMaker )
		{
			damageAudioMaker.Play( this.gameObject );
		}
		
		// 자식 오브젝트에 존재하는 모든 효과 오브젝트를 취득.
		effects = GetComponentsInChildren<ParticleSystem>();
		
		// 모든 효과를 재생.
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
		// 재생 종료한 효과를 카운트         
		for( int i = 0; i < effects.Length; i++ )
		{
			if ( !effects[i].isPlaying )
			{
				endOfPlayingCount++;
			}
		}
				
		// 모두 재생이 종료된 경우 오브젝트를 폐기한다.
		if ( endOfPlayingCount >= playingCount )
		{
			Destroy( this.gameObject );
		}
		
	}
}
