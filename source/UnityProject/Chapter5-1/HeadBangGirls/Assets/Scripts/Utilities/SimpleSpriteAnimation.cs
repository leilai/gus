using UnityEngine;
using System.Collections;
// SimpleSpriteAnimation을 실행하는 컴포넌트
public class SimpleSpriteAnimation: MonoBehaviour {
//Public variables
	public float animationFrameRateSecond=0.2f;
	public int divisionCountX=1;
	public int divisionCountY=1;
//Public methods
	public void BeginAnimation( int fromIndex, int toIndex, bool loop=false ){
		m_currentIndex = m_fromIndex = fromIndex;
		m_toIndex = toIndex;
		m_loop = loop;
		m_frameElapsedTime = 0;
		SetMaterilTextureUV();
	}
	//현재의 메인 텍스처를 취득
	public Texture GetTexture(){
		return renderer.material.GetTexture("_MainTex");
	}
	//텍스처 표시 부분의 Rect를 취득
	public Rect GetUVRect(int frameIndex){
		int frameIndexNormalized=frameIndex;
		if(frameIndex>=divisionCountX*divisionCountY) 
			frameIndexNormalized=frameIndex%(divisionCountX*divisionCountY);
		float posX=((frameIndexNormalized%divisionCountX)/(float)divisionCountX);
		float posY=(1- ((1+(frameIndexNormalized/divisionCountX))/(float)divisionCountY));
		return new Rect( 
			posX, 
			posY, 
			renderer.material.mainTextureScale.x, 
			renderer.material.mainTextureScale.y
		);
	}
	public Rect GetCurrentFrameUVRect(){
		return GetUVRect(m_currentIndex);
	}
	//명확한 지정이 없는 경우의 애니메이션을 설정
	public void SetDefaultAnimation( int defaultFromIndex, int defaultToIndex ){
		m_currentIndex = m_fromIndex = m_defaultFromIndex = defaultFromIndex;
		m_toIndex = m_defaultToIndex = defaultToIndex;
	}
	//픽셀 폭 취득
	public float GetWidth(){
		return renderer.material.mainTextureScale.x * renderer.material.GetTexture("_MainTex").width;
	}
	//픽셀 높이를 취득
	public float GetHeight(){
		return renderer.material.mainTextureScale.y * renderer.material.GetTexture("_MainTex").height;
	}
	//애니메이션의 코마를 진행한다.
	public void AdvanceFrame(){
		if(m_fromIndex<m_toIndex){
			if( m_currentIndex <= m_toIndex ){
				m_currentIndex++;
				if( m_toIndex < m_currentIndex ){
					if( m_loop ){
						m_currentIndex=m_fromIndex;
					}
					else{
						m_currentIndex = m_fromIndex = m_defaultFromIndex;
						m_toIndex = m_defaultToIndex;
					}
				}
				SetMaterilTextureUV();
			}
		}
		else{
			if( m_currentIndex >= m_toIndex ){
				m_currentIndex--;
				if( m_toIndex > m_currentIndex ){
					if( m_loop ){
						m_currentIndex=m_fromIndex;
					}
					else{
						m_currentIndex = m_fromIndex = m_defaultFromIndex;
						m_toIndex = m_defaultToIndex;
					}
				}
				SetMaterilTextureUV();
			}
		}
	}
	void Start () {
		renderer.material.mainTextureScale = new Vector2(1.0f/divisionCountX,1.0f/divisionCountY);
	}
	// Update is called once per frame
	void Update () {
		m_frameElapsedTime+=Time.deltaTime;
		if( animationFrameRateSecond < m_frameElapsedTime ){
			m_frameElapsedTime=0;
			AdvanceFrame();
		}
	}
	//코마 번호에서 적절한 텍스처 좌표UV를 설정
	void SetMaterilTextureUV(){
		Rect uvRect = GetCurrentFrameUVRect();
		renderer.material.mainTextureOffset=new Vector2(uvRect.x,uvRect.y);
	}
//Private variables
	float m_frameElapsedTime=0;
	int m_fromIndex = 0, m_toIndex = 0;
	int m_defaultFromIndex = 0, m_defaultToIndex = 0;
	bool m_loop = false;
	int m_currentIndex=0;
	
}
