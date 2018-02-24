using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 화면을 페이드 아웃
//  - alpha(투과)を 0(투과율100%) -> 1(투과율0%) 로 변화시켜 페이드 아웃 효과를 재현한다.
// ----------------------------------------------------------------------------
public class FadeOut : MonoBehaviour 
{
    private float alphaRate = 0f;			// 투과율
    private Color textureColor;				// 텍스처 색 정보.
	private bool isEnabled = false;
	private string openingSceneName = "opening";	//오프닝 제목

	void Start () 
	{
        //  텍스처를 화면에 넓게 펼친다.
        guiTexture.pixelInset = new Rect(0, 0, 480, 640);

        // FadeOut된 상태
		textureColor = guiTexture.color;
		textureColor.a = alphaRate;
		guiTexture.color = textureColor;
	}

	void Update () 
	{
		if ( isEnabled )
		{
            // 투과가 100%에 도달되어 있나?.
			if ( alphaRate < 1 )
			{
				// FadeOut.
				alphaRate += 0.007f;
				textureColor.a = alphaRate;
				guiTexture.color = textureColor;
			}
			else
			{
				// 게임 씬을 불러온다.
				Application.LoadLevel( openingSceneName );
			}
		}
	}
	
	public void SetEnable()
	{
		StartCoroutine( WaitAndEnable( 8f ) );
	}
	
	IEnumerator WaitAndEnable( float waitForSeconds )
	{
		// 지정한 시간을 기다린다.
		yield return new WaitForSeconds( waitForSeconds );
		
		isEnabled = true;
	}
}
