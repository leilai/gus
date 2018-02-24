using UnityEngine;
using System.Collections;

public class EffectControl : MonoBehaviour {

	public GameObject	eff_break = null;	// 종이를 찢을 때의 효과
	public GameObject	eff_miss  = null;	// 철판에 충돌하였을 때의 효과

	public GameObject	game_camera = null;

	// ---------------------------------------------------------------- //

	void 	Start()
	{
		this.game_camera = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	void	Update()
	{
	
	}

    // 종이를 찢을 때의 효과
	public void	createBreakEffect(SyoujiPaperControl paper, NekoControl neko)
	{
		GameObject 	go = Instantiate(this.eff_break) as GameObject;

		go.AddComponent<Effect>();

		Vector3	position = paper.transform.position;

		position.x = neko.transform.position.x;
		position.y = neko.transform.position.y;

		position.y += 0.3f;

		go.transform.position = position;

		// 화면에 오래 표시하기 위해 카메라의 자식구조로 설정한다.(카메라와 함께 이동하도록)      
		go.transform.parent = this.game_camera.transform;
	}

	public void	createMissEffect(NekoControl neko)
	{
		GameObject 	go = Instantiate(this.eff_miss) as GameObject;

		go.AddComponent<Effect>();

		Vector3	position = neko.transform.position;

		position.y += 0.3f;

		// 철판에 가려지지 않도록
		position.z -= 0.2f;

		go.transform.position = position;
	}
}
