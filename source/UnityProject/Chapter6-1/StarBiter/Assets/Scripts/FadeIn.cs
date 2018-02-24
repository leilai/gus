using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 화면을 페이드인.
//  - alpha(투과)を 1(투과율0%) -> 0(투과율100%)로 변화시켜 페이드인 효과를 재현한다.
// ----------------------------------------------------------------------------
public class FadeIn : MonoBehaviour {
	
	private float alphaRate = 1f;			// 투과율
	private Color textureColor;				// 텍스처 색 정보.

	void Start () {
	
		// 텍스처를 화면에 넓게 펼친다.
        guiTexture.pixelInset = new Rect(0, 0, 480, 640);
		
		// FadeOut된 상태
		textureColor = guiTexture.color;
		textureColor.a = alphaRate;
		guiTexture.color = textureColor;
	}

	void Update () {
	
		// 투과가 100%에 도달되어 있나?.
		if ( alphaRate > 0 )
		{
			// FadeIn.
			alphaRate -= 0.007f;
			textureColor.a = alphaRate;
			guiTexture.color = textureColor;
		}
		
	}
}
