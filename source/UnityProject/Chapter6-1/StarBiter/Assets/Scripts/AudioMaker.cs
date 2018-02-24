using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 소리의 동시발생을 컨트롤한다.
//  - 동시발생 수의 상한선을 설정한다.
//  - 음의 정지시에 발생하는 노이즈를 제어한다.
// ----------------------------------------------------------------------------
public class AudioMaker : MonoBehaviour {

	public int maxPlayingCount = 0;						// 최대 동시 재생수
	public GameObject audioObject;						// 재생할 소리
	
	private bool initialized = false;					// 초기화 완료
	private GameObject[] AudioInstances;				// 재생할 소리의 인스턴스.
	
	void Start() 
	{
		// 소리가 없는 경우 종료
		if ( !audioObject )
		{
			return;
		}
			
		// 재생할 소리 준비(최대 동시 발생수의 수만큼 준비).
		AudioInstances = new GameObject[maxPlayingCount];
		for( int i = 0; i < maxPlayingCount; i++ )
		{
			GameObject audioInstance = Instantiate(
				audioObject,
				Vector3.zero,
				new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
			AudioInstances[i] = audioInstance;
		}
		
		// 초기화 완료 상태를 저장한다.
		if ( AudioInstances.Length > 0 )
		{
			initialized = true;
		}
	}
	
	// 음의 재생.
	public void Play( GameObject target )
	{
		if ( initialized && target )
		{
			bool canPlay = false;
			for( int i = 0; i < maxPlayingCount; i++ )
			{
				AudioSource audioSource = AudioInstances[i].GetComponent<AudioSource>();
				
				// 빈 공간(재생하지 않은 소리)를 찾는다.
				if ( !audioSource.isPlaying )
				{
					// 소리 재생.
					canPlay = true;
					audioSource.Play();
					break;
				}
			}
			// 동시발생 수에 도달하였으므로 재생할 수 없다.
			if ( !canPlay )
			{
				// 재생 중인 소리 한 개를 정지하고 새롭게 재생한다.
				
				// ------------------------------------------------------------
				// 노이즈 대책.
				//  - Audio를 Stop(또는 재생중에 다시 Play)하면 노이즈가 들어가므로 아래의 대책을 따른다.
				//    1. 하나의 소리를 Mute한다.
				//    2. 새롭게 오브젝트를 작성한다.
				//    3. 재생한다.
				// ------------------------------------------------------------
				
				// Mute、삭제
				AudioInstances[0].GetComponent<AudioSource>().mute = true;
				AudioInstances[0].GetComponent<AudioBreaker>().SetDestroy();
				
				// 선두 요소를 삭제한 후에 배열 앞에 설정한다.
				for( int i = 0; i < maxPlayingCount - 1; i++ )
				{
					AudioInstances[i] = AudioInstances[i + 1];
				}
				
				// 새롭게 오브젝트를 작성하여 요소의 마지막에 추가후 재생한다.
				GameObject audioInstance = Instantiate(
					audioObject,
					Vector3.zero,
					new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
				AudioInstances[maxPlayingCount - 1] = audioInstance;
				AudioInstances[maxPlayingCount - 1].GetComponent<AudioSource>().Play();
			}
		}
	}
}
