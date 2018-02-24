using UnityEngine;
using System.Collections;

// 『다시하기』버튼.
public class RetryButtonControl : MonoBehaviour {

	// 표시용 간이 스프라이트.
	public SimpleSprite	sprite = null;

	public Texture		texture;

	private GameControl	game_control;

	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {
	
		this.sprite = this.gameObject.AddComponent<SimpleSprite>();

		this.sprite.setTexture(this.texture);
		this.sprite.setSize(this.texture.width*0.02f/3.0f, this.texture.height*0.02f/3.0f);
		this.sprite.create();

		this.GetComponent<MeshCollider>().sharedMesh = this.GetComponent<MeshFilter>().mesh;

		this.game_control = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown()
	{
		this.game_control.OnRetryButtonPush();
	}
}
