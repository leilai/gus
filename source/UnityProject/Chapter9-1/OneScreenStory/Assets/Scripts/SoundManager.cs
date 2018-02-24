
using UnityEngine;


/// <summary>사운드 재생관리 클래스ス</summary>
class SoundManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 관련 멤버 변수・메소드

	/// <summary>재생대상 오디오클립</summary>
	public AudioClip[] m_audioClips;

	/// <summary>스타트업 메소드</summary>
	private void Start()
	{
		// SE 와 BGM 용 음원 작성
		audioSE  = gameObject.AddComponent< AudioSource >();
		audioBGM = gameObject.AddComponent< AudioSource >();

		// BGM 을 재생
		audioBGM.clip = getAudioClip( "rpg_ambience01" );
		audioBGM.loop = true;
		audioBGM.Play();
	}


	//==============================================================================================
	// 공개 메소드

	/// <summary>SE를 재생한다</summary>
	public void playSE( AudioClip clip, bool isLoop = false )
	{
		if ( clip != null )
		{
			audioSE.clip = clip;
			audioSE.loop = isLoop;
			audioSE.Play();
		}
	}

	/// <summary>이름을 지정하여 SE를 재생한다.</summary>
	public void playSE( string name, bool isLoop = false )
	{
		AudioClip clip = getAudioClip( name );
		if ( clip != null )
		{
			playSE( clip, isLoop );
		}
	}

	/// <summary>SE 를 정지한다. </summary>
	public void stopSE()
	{
		audioSE.Stop();
	}

	/// <summary>이름에서 오디오클립을 가져온다. </summary>
	public AudioClip getAudioClip( string name )
	{
		AudioClip audioClip = null;
		foreach ( AudioClip clip in m_audioClips )
		{
			if ( name == clip.name )
			{
				audioClip = clip;
				break;
			}
		}

		return audioClip;
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>SE용 음원 오브젝트</summary>
	private AudioSource audioSE;

	/// <summary>BGM용 음원 오브젝트</summary>
	private AudioSource audioBGM;
}
