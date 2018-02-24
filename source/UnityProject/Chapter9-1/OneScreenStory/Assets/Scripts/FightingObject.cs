
using UnityEngine;


/// <summary>전투 오브젝트 전용 클래스</summary>
class FightingObject : DraggableObject
{
	//==============================================================================================
	// 내부 데이터 형태

	/// <summary>전투 커맨드의 진행상황</summary>
	private enum BattleStatus
	{
		BeforeBattle,
		ZoomingIn,
		Battle,
		AfterBattle,
		ZoomingOut,
		EndBattle
	}


	//==============================================================================================
	// MonoBehaviour 관련 멤버 변수・메소드

	/// <summary>카메라 매니저 오브젝트</summary>
	public CameraManager m_cameraManager;

	/// <summary>전투중에 표시하는 효과</summary>
	public ParticleSystem[] m_fightingEffects;

	/// <summary>스타트업 메소드</summary>
	private void Start()
	{
		m_objectManager = m_eventManager.GetComponent< ObjectManager >();
		m_soundManager  = m_eventManager.getSoundManager();
	}


	//==============================================================================================
	// 공개 메소드

	/// <summary>액터에서 오는 메세지를 프레임마다 처리한다. </summary>
	public override bool updateMessage( string message, string[] parameters )
	{
		switch ( message )
		{
		//격투 커맨드
		case "battle":
			{
				switch ( m_battleStatus )
				{
				case BattleStatus.BeforeBattle:
					// 텍스트를 지운다.
					TextManager textman = m_objectManager.GetComponent< TextManager >();
					if ( textman != null ) textman.hide();

					// 전투발생장소에서 카메라를 향해 뻗어있는 레이를 구한다. 
                    BaseObject enemy = m_objectManager.find( parameters[ 0 ] );
					Vector3 myPosition    = transform.position + 0.5f * ( getYTop() + getYBottom() ) * Vector3.up;
					Vector3 enemyPosition = enemy.transform.position + 0.5f * ( enemy.getYTop() + enemy.getYBottom() ) * Vector3.up;
					Vector3 battlePosition = Vector3.Lerp( myPosition, enemyPosition, 0.5f );
					Quaternion qt = Quaternion.AngleAxis( 20.0f, Vector3.right );

					m_rayFromBattlePositionToCamera = new Ray( battlePosition, qt * -Vector3.forward );

					// 격투장소로 줌인
					m_cameraManager.moveTo(
						m_rayFromBattlePositionToCamera.GetPoint( 5100.0f / Mathf.Sin( 32.92f * Mathf.Deg2Rad ) ),
						22.0f,
						m_cameraManager.getOriginalSize() * 0.5f,
						0.5f );

					m_battleStatus = BattleStatus.ZoomingIn;
					return true;  // 실행을 계속

				case BattleStatus.ZoomingIn:
					// 카메라 이동중
					if ( !m_cameraManager.isMoving() )
					{
						m_battleStatus = BattleStatus.Battle;
						m_currentFrame = 0;
					}
                    return true;  // 실행을 계속

				case BattleStatus.Battle:
					// 애니메이션/사운드 재생
					if ( m_currentFrame == 0 )
					{
						foreach ( ParticleSystem particle in m_fightingEffects )
						{
							// 격투 효과는 캐릭터에 겹쳐지도록 조금 앞쪽에 표시
							particle.transform.position = m_rayFromBattlePositionToCamera.GetPoint( 100.0f );
							particle.Play();
						}

						// 격투음재생
						m_soundManager.playSE( "rpg_system07", true );
					}

					if ( ++m_currentFrame >= m_animationFrames )
					{
						m_battleStatus = BattleStatus.AfterBattle;
					}
					return true;  // 싫행을 계속

				case BattleStatus.AfterBattle:
                    // 애니메이션/사운드 정지
					foreach ( ParticleSystem particle in m_fightingEffects )
					{
						particle.Stop();
					}
					m_soundManager.stopSE();

					// 카메라를 원래의 장소로 보낸다.
					m_cameraManager.moveTo(
						m_cameraManager.getOriginalPosition(),
						m_cameraManager.getOriginalRotationX(),
						m_cameraManager.getOriginalSize(),
						0.5f );

					m_battleStatus = BattleStatus.ZoomingOut;
					return true;  // 실행을 계속

				case BattleStatus.ZoomingOut:
					// 카메라 이동중
					if ( !m_cameraManager.isMoving() )
					{
						m_battleStatus = BattleStatus.EndBattle;
					}
					return true;  // 실행을 계속

				case BattleStatus.EndBattle:
					// 종료
					m_battleStatus = BattleStatus.BeforeBattle;
					return false;

				default:
					return false;
				}
			}
		}

		return false;
	}


	//==============================================================================================
	// 비공개 멤버 변수

	/// <summary>오브젝트매니저 오브젝트</summary>
	private ObjectManager m_objectManager;

	/// <summary>사운드매니저 오브젝트</summary>
	private SoundManager m_soundManager;

	/// <summary>격투 커맨드의 진행상태</summary>
	private BattleStatus m_battleStatus = BattleStatus.BeforeBattle;

	/// <summary>전투발생 장소에서 카메라를 향해 뻗어있는 레이</summary>
	private Ray m_rayFromBattlePositionToCamera;

	/// <summary>전투 애니메이션의 프레임 번호</summary>
	private int m_currentFrame = 0;

	/// <summary>전투 애니메이션 프리임수</summary>
	private const int m_animationFrames = 300;
}
