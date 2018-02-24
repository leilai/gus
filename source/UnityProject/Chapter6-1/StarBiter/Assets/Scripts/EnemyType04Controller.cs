using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType04Controller
//  - 「적기 모델 타입04」의 동작를 제어한다.
//  - 사용 방법.
//    - 컴트롤러가 첨부된 오브젝트는 EnemyMaker에 의해 일정간격으로 작성된다.
//  - 작동 방법(임시).
//    - 플레이어의 진행방향의 왼쪽 위 또는 오른쪽 위에서 출현한다.
//    - 플레이어 주변을 돈다.
//    - 플레이어와 같은 스피드로 일정시간 붙어 있는다.
//    - 화면 밖으로
// ----------------------------------------------------------------------------
public class EnemyType04Controller : MonoBehaviour {

    public float speed = 10f;							// 적기의 스피드.
    public float turnSpeed = 1.6f;						// 적기의 방향 전환 스피드.
	public float followingSpeed = 10f;					// 감시 중인 스피드.
    public float uTurnSpeed = 20f;						// U턴 중의 스피드.
	public float distanceFromPlayer = 10f;				// 플레이어와 일정거리를 유지한다.
	public float followingTime = 5f;					// 감시 시간.
	
	private GameObject player;							// 플레이어.
	private float distanceToPlayer = 10.0f;				// 출현 위치에서 플레이어까지의 거리.
	private bool isUTurn = false;
	
	private GameObject subScreenMessage;				// SubScreen의 메세지 영역
	
	void Start () {
	
		// 플레이어를 취득
		player = GameObject.FindGameObjectWithTag("Player");
		
		// SubScreenMessage의 인스턴스를 취득.
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
		// --------------------------------------------------------------------
        // 출현 위치 지정
		// --------------------------------------------------------------------
		
		// 먼저 플레이어의 위치에
		transform.position = player.transform.position;
		float tmpAngle = Random.Range( 0f, 360f );
		transform.rotation = Quaternion.Euler( 0, tmpAngle, 0 );
		
		// 위치를 조정
		transform.Translate ( new Vector3( 0f, 0f, distanceToPlayer ) );
		
		// ----------------------------------------------------------------
        // 적기의 진행 방향을 정한다.
		// ----------------------------------------------------------------
		
		// 플레이어가 있을 때에만 처리
		if ( player )
		{
			// 플레이어의 방향을 취득.
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			Quaternion tiltedRotation = Quaternion.Euler( 0, targetRotation.eulerAngles.y + 30f, 0 );

            // 적기의 각도를 변경    
			transform.rotation = tiltedRotation;
		}

        // 적기를 움직인다.
		this.GetComponent<EnemyStatus>().SetIsAttack( true );
		
	}
	
	void Update () {
	
		bool isAttack = this.GetComponent<EnemyStatus>().GetIsAttack();
		if ( isAttack )
		{
			if ( !isUTurn )
			{
                // 적기를 이동한다.
				Forward();
			
				// 플레이어와 근접해 있나?
				IsNear();
			}
			else
			{
				// 도망친다.
				Backward();
			}	
		}
	}
	
	// ------------------------------------------------------------------------
	// 적기를 전진시킨다.
	// ------------------------------------------------------------------------
	private void Forward()
	{
        // 플레이어가 있을 때에만 처리
		if ( player )
		{
            // 플레이어의 방향을 취득.
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

            // 적기의 현재 방향에서 플레이어와 반대 방향으로 지정한 스피드로 향한 후의 각도를 취득.
			float targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnSpeed * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );

            // 적기의 각도를 변경        
			transform.rotation = tiltedRotation;

            // 적기를 이동한다.
			transform.Translate ( new Vector3( 0f, 0f, speed * Time.deltaTime ) );
		}
	}
	
	private void IsNear()
	{
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		
		if ( distance < distanceFromPlayer )
		{
			// 스피드를 낮춘다.(플레이어와의 거리를 일정하게 유지한다.).
			speed = followingSpeed;
			
			// 일정 시간후에 회피 행동을 하기 위한 타이머 설정
			StartCoroutine("SetTimer");
		}
	}
	
	IEnumerator SetTimer()
	{
		// 일정 시간 대기
		yield return new WaitForSeconds( followingTime );
		
		// 회피 행동 개시
		isUTurn = true;
	}
	
	private void Backward()	
	{
        // 플레이어가 있을 때에만 처리
		if ( player )
		{
            // 플레이어의 방향을 취득.
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

            // 적기의 현재 방향에서 플레이어와 반대 방향으로 지정한 스피드로 향한 후의 각도를 취득.
			float targetRotationAngle = targetRotation.eulerAngles.y * - 1;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnSpeed * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );

            // 적기의 각도를 변경        
			transform.rotation = tiltedRotation;

            // 적기를 이동한다.
			transform.Translate ( new Vector3( 0f, 0f, uTurnSpeed * Time.deltaTime ) );
		}
	}
	
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if ( subScreenMessage != null )
			{
				if (
					this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
					this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
				{
					subScreenMessage.SendMessage("SetMessage", " ");
					subScreenMessage.SendMessage("SetMessage", "DESTROYED PATROL SHIP." );
					subScreenMessage.SendMessage("SetMessage", " ");
				}
				else
				{
					subScreenMessage.SendMessage("SetMessage", " ");
					subScreenMessage.SendMessage("SetMessage", "LOST PATROL SHIP..." );
					subScreenMessage.SendMessage("SetMessage", " ");
				}
			}
		}
	}

}
