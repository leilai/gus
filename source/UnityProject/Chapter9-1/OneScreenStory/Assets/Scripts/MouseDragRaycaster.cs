
using System.Collections;
using UnityEngine;


/// <summary> MouseDragRaycaster를 실행하는 클래스</summary>
class MouseDragRaycaster : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 관련 멤버 변수・메소드

    /// <summary>Lay의 충돌 대상이 되는 오브젝트의 Layer</summary>
	public LayerMask m_detectionLayer;

	/// <summary>이벤트매니저 오브젝트</summary>
	public EventManager m_eventManager;

    /// <summary>매직핸드 오브젝트</summary>
	public GameObject m_magicHand;
	private Vector3 m_magicHandOffset;
	private bool m_isReleasing = false;

	/// <summary>매직핸드 표시/ 비표시 교체시에 표시하는 효과</summary>
	public ParticleSystem m_magicHandEffect;

	/// <summary>스타트업 메소드</summary>
	private void Start()
	{
		m_magicHandOffset = m_magicHand.transform.position;

        // 프레임마다의 처리는 Coroutine 내에서 실행한다. 
		StartCoroutine( "updateByCoroutine" );
	}


	//==============================================================================================
	// 비공개 메소드

    /// <summary>Coroutine에 따른 매 프레임 갱신 메소드</summary>
	private IEnumerator updateByCoroutine()
	{
		while ( true )
		{
			// 왼쪽 버튼 클릭 대기 
            while ( !Input.GetMouseButtonDown( 0 ) || m_eventManager.isExecutingEvents() )
			{
				if ( m_isReleasing && !m_magicHand.animation.isPlaying )
				{
					m_magicHand.SetActiveRecursively( false );
					m_isReleasing = false;
				}

				yield return 0;
			}

            // 마우스 위치에 ray를  설정하여 오브젝트를 검색한다. 
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
			RaycastHit hit;
			if ( Physics.Raycast( ray, out hit, float.PositiveInfinity, m_detectionLayer.value ) )
			{
				// DraggableObject 컴포넌트를 가져온다.
				DraggableObject draggable = hit.rigidbody.gameObject.GetComponent< DraggableObject >();

				if ( draggable != null )
				{
					// 매직핸드를 표시・해당 오브젝트로 이동・들어올리기 애니메이션 재성
					Vector3 position = m_magicHandOffset + draggable.transform.position + draggable.getYTop() * Vector3.up;
					m_magicHand.SetActiveRecursively( true );
					m_magicHand.transform.position = position;
					m_magicHand.animation.Play( "M20_magichand_pick" );

					// 드래그 시작음을 재생한다. 
					audio.Play();

					// 드래그 시작을 알림
					draggable.onDragBegin( hit );

					// onDragBegin 과 onDragUpdate 가 같은 프레임에서 일어나지 않도록 여기에서  yield 을 추가
					yield return 0;

					// 왼쪽 버튼을 누르고 잇는 동안은 드래그 중인것으로 알림
					while ( Input.GetMouseButton( 0 ) )
					{
						// 매직 핸드 이동
						position = m_magicHandOffset + draggable.transform.position + draggable.getYTop() * Vector3.up;
						m_magicHand.transform.position = position;

						draggable.onDragUpdate();
						yield return 0;
					}

                    // magicHand Releasing 애니메이션 재생
					m_magicHand.animation.Play( "M20_magichand_releasing" );
					m_isReleasing = true;

					// ドラッグ終了を通知
					draggable.onDragEnd();
				}
			}

			// 프레임 진행
			yield return 0;
		}
	}
}
