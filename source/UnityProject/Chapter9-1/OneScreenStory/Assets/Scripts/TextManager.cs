
using UnityEngine;


/// <summary>지면의 문장・회화문 표시 클래스</summary>
class TextManager : MonoBehaviour
{
	//==============================================================================================
    // MonoBehaviour 관련 멤버 변수・메소드

	/// <summary>텍스트 오브젝트</summary>
	public GUIText m_text;
	/// <summary>폰트 사이즈</summary>
	public int m_fontSize;

	/// <summary>왼쪽 위 GUI 텍스처 오브젝트</summary>
	public GUITexture m_northWestTexture;
    /// <summary>왼쪽 GUI 텍스처 오브젝트</summary>
	public GUITexture m_westTexture;
    /// <summary>왼쪽 아래 GUI 텍스처 오브젝트</summary>
	public GUITexture m_southWestTexture;
    /// <summary>중앙 GUI 텍스처 오브젝트</summary>
	public GUITexture m_centerTexture;
    /// <summary>오른쪽 위 GUI 텍스처 오브젝트</summary>
	public GUITexture m_northEastTexture;
    /// <summary>오른쪽 GUI 텍스처 오브젝트</summary>
	public GUITexture m_eastTexture;
    /// <summary>오른쪽 아래 GUI 텍스처 오브젝트</summary>
	public GUITexture m_southEastTexture;
    /// <summary>말풍선 GUI 텍스처 오브젝트</summary>
	public GUITexture m_balloonTexture;

	/// <summary>말풍선 부분 텍스처 폭</summary>
	public float m_balloonWidth;
    /// <summary>말풍선 부분 텍스처 높이</summary>
	public float m_balloonHeight;
    /// <summary>말풍선 부분과 오브젝트의 사이</summary>
	public float m_voidBetweenDialogAndObject;
    /// <summary>말풍선을 표시할 때의 screenMargin</summary>
	public float m_screenMarginOfBalloon;

	/// <summary>지면 문장의 배경색</summary>
	public Color m_textBackground = new Color32( 0, 0, 0, 160 );

	/// <summary>회화문을 표시할 때에 재생하는 오디오소스</summary>
	public AudioSource m_dialogSoundSource;


	//==============================================================================================
	// 공개 메소드

	/// <summary>지면의 문장・회화문을 숨긴다.</summary>
	public void hide()
	{
		m_text.enabled             =
		m_northWestTexture.enabled =
		m_westTexture.enabled      =
		m_southWestTexture.enabled =
		m_centerTexture.enabled    =
		m_northEastTexture.enabled =
		m_eastTexture.enabled      =
		m_southEastTexture.enabled =
		m_balloonTexture.enabled   = false;
	}

	/// <summary>지면의 문장을 표시한다. </summary>
	public void showText( string text, Vector2 center, float marginX, float marginY, float radius )
	{
		// 일단 숨긴다. 
		hide();

		// 텍스트 위치조정・표시
		m_text.text = text;
		Rect r = m_text.GetScreenRect();
		int textLines = text.Trim().Split( '\n' ).Length;
		float textWidth  = r.width;
		float textHeight = ( m_text.lineSpacing * ( textLines - 1 ) + 1.0f ) * m_fontSize;  // 最終行は lineSpacing を無視
		m_text.pixelOffset = new Vector2( center.x, center.y - ( r.height - textHeight ) / 2.0f );
		m_text.enabled     = true;

		// GUI 텍스처를 표시
		showGUITexture( new Rect( center.x, center.y, textWidth + 2.0f * marginX, textHeight + 2.0f * marginY ), radius, m_textBackground );
	}

	/// <summary>회화문을 표시한다. </summary>
	public void showDialog( BaseObject baseObject, string text, float marginX, float marginY, float radius )
	{
        // 일단 숨긴다. 
		hide();

		// 텍스트 표시에 필요한 폭과 높이를 가져와 계산
		m_text.text = text;
		Rect r = m_text.GetScreenRect();
		int textLines = text.Trim().Split( '\n' ).Length;
		float textWidth  = r.width;
		float textHeight = ( m_text.lineSpacing * ( textLines - 1 ) + 1.0f ) * m_fontSize;

        // margin을 넣은 폭과 높이를 계산
		float wholeWidth  = textWidth  + 2.0f * marginX;
		float wholeHeight = textHeight + 2.0f * marginY;

		// GameObject 의 스크린 좌표를 가져온다. 
		Vector3 screenPointTop    = Camera.main.WorldToScreenPoint(
			baseObject.gameObject.transform.position + new Vector3( 0.0f, baseObject.getYTop(), 0.0f ) );
		Vector3 screenPointBottom = Camera.main.WorldToScreenPoint(
			baseObject.gameObject.transform.position + new Vector3( 0.0f, baseObject.getYBottom(), 0.0f ) );

		// GUI 텍스처 표시위치를 계산한다.(dialogXY 는 중심좌표,  balloonXY 는 상부표시 장소는 왼쪽아래/하부표시 장소는 왼쪽위  좌표)
		float dialogX  = screenPointTop.x + 0.2f * wholeWidth;  // ちょっとだけ右にずらすと吹き出しっぽくなる
		float balloonX = screenPointTop.x - m_balloonWidth / 2.0f;
		if ( dialogX - wholeWidth / 2.0f < m_screenMarginOfBalloon )
		{
			// 말풍선이 왼쪽으로 삐져나오게 된다. 
			dialogX = m_screenMarginOfBalloon + wholeWidth / 2.0f;
			{
				// 배경의 원형부분의 최대반경을 계산
				float radiusMaxX = wholeWidth  / 2.0f;
				float radiusMaxY = wholeHeight / 2.0f;
				float radiusMax  = radiusMaxX < radiusMaxY ? radiusMaxX : radiusMaxY;
				// 최대반경보다 큰 지정은 무효
				if ( radius > radiusMax ) { radius = radiusMax; }
				// balloonX 위치조정
				if ( balloonX < m_screenMarginOfBalloon + radius ) { balloonX = m_screenMarginOfBalloon + radius; }
			}
		}
		else if ( dialogX + wholeWidth / 2.0f + m_screenMarginOfBalloon > Camera.main.pixelWidth )
		{
            //  말풍선이 오른쪽으로 삐져나오게 된다. 
			dialogX = Camera.main.pixelWidth - m_screenMarginOfBalloon - wholeWidth / 2.0f;
			{
                // 배경의 원형부분의 최대반경을 계산
				float radiusMaxX = wholeWidth  / 2.0f;
				float radiusMaxY = wholeHeight / 2.0f;
				float radiusMax  = radiusMaxX < radiusMaxY ? radiusMaxX : radiusMaxY;
                // 최대반경보다 큰 지정은 무효
				if ( radius > radiusMax ) { radius = radiusMax; }
                // balloonX 위치조정
				if ( balloonX > Camera.main.pixelWidth - m_screenMarginOfBalloon - radius - m_balloonWidth )
				{ balloonX = Camera.main.pixelWidth - m_screenMarginOfBalloon - radius - m_balloonWidth; }
			}
		}

		// 말풍선의 위치를 결정
		bool isUpper = true;	// 原則頭上
		if ( screenPointTop.y + m_voidBetweenDialogAndObject + m_balloonHeight + wholeHeight + m_screenMarginOfBalloon > Camera.main.pixelHeight )
		{
			// 말풍선이 위로 돌출되는 경우에는 아래로
            isUpper = false;
		}
		float dialogY, balloonY;
		if ( isUpper )
		{
			// 말풍선은 상부에 표시한다. 
			balloonY = screenPointTop.y + m_voidBetweenDialogAndObject;
			dialogY  = balloonY + m_balloonHeight + wholeHeight / 2.0f;
		}
		else
		{
			// 말풍선은 하부에 표시한다. 
			balloonY = screenPointBottom.y - m_voidBetweenDialogAndObject;
			dialogY  = balloonY - m_balloonHeight - wholeHeight / 2.0f;
		}

		// GUI 텍스처를 표시
		showGUITexture( new Rect( dialogX, dialogY, wholeWidth, wholeHeight ), radius, baseObject.getDialogBackgroundColor() );

		// 말풍선 부분의 GUI텍스처를 표시
		m_balloonTexture.color      = baseObject.getDialogBackgroundColor();
		m_balloonTexture.pixelInset = new Rect( balloonX, balloonY, m_balloonWidth, isUpper ? m_balloonHeight : -m_balloonHeight );
		m_balloonTexture.enabled    = true;

		// 텍스트를 표시
		m_text.pixelOffset = new Vector2( dialogX, dialogY - ( r.height - textHeight ) / 2.0f );
		m_text.enabled = true;

		// 회화문 표시의 경우에는 사운드를 재생한다. 
		m_dialogSoundSource.Play();
	}


	//==============================================================================================
	// 비공개 메소드

	/// <summary>말풍선 부분이외의 GUITexture 배경 오브젝트를 표시한다. </summary>
	private void showGUITexture( Rect rect, float radius, Color color )
	{
		// 최대 허용 반지름(폭 또는 높이의 반정도)를 넘지 않도록 한다. 
		radius = Mathf.Min( radius, rect.width  / 2.0f, rect.height / 2.0f );

		// 왼쪽 위
		float x      = rect.x - rect.width  / 2.0f;
		float y      = rect.y + rect.height / 2.0f - radius;
		float width  = radius;
		float height = radius;
		m_northWestTexture.color      = color;
		m_northWestTexture.pixelInset = new Rect( x, y, width, height );
		m_northWestTexture.enabled    = true;

		// 오른쪽
		height = rect.height - 2.0f * radius;
		y     -= height;
		m_westTexture.color      = color;
		m_westTexture.pixelInset = new Rect( x, y, width, height );
		m_westTexture.enabled    = true;

		// 왼쪽 아래
		height = -radius;
		m_southWestTexture.color      = color;
		m_southWestTexture.pixelInset = new Rect( x, y, width, height );
		m_southWestTexture.enabled    = true;

		// 중앙
		x     += radius;
		y     -= radius;
		width  = rect.width - 2.0f * radius;
		height = rect.height;
		m_centerTexture.color      = color;
		m_centerTexture.pixelInset = new Rect( x, y, width, height );
		m_centerTexture.enabled    = true;

		// 오른쪽 위
		x     += width  + radius;
		y     += height - radius;
		width  = -radius;
		height =  radius;
		m_northEastTexture.color      = color;
		m_northEastTexture.pixelInset = new Rect( x, y, width, height );
		m_northEastTexture.enabled    = true;

		// 오른쪽
		height = rect.height - 2.0f * radius;
		y     -= height;
		m_eastTexture.color      = color;
		m_eastTexture.pixelInset = new Rect( x, y, width, height );
		m_eastTexture.enabled    = true;

		// 오른쪽 아래
		height = -radius;
		m_southEastTexture.color      = color;
		m_southEastTexture.pixelInset = new Rect( x, y, width, height );
		m_southEastTexture.enabled    = true;
	}
}
