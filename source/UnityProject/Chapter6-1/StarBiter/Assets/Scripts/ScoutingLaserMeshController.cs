using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 탐색 레이저의 충돌 판정에 사용하는 MeshCollider를 작성.
// ----------------------------------------------------------------------------
public class ScoutingLaserMeshController : MonoBehaviour {
	
	private Mesh mesh;
	private MeshFilter meshFilter;
	private MeshCollider meshCollider;
	private ScoutingLaser scoutingLaser;
	
	private static float PIECE_ANGLE = 5.0f;		// 1폴리곤의 각도(원만한 원).　
	private static float FAN_RADIUS = 10.0f;		// 원의 반지름
	
	void Start () {

		mesh = new Mesh();
		meshFilter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
		meshCollider.mesh = mesh;

		// .
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser").GetComponent<ScoutingLaser>();
		
	}
	
	void Update () {
	}
	
	public void clearShape()
	{
		mesh.Clear();
		meshFilter.mesh = mesh;
		// mesh 를 변경한 후에는 false -> true 로 설정하지 않으면 반영되지 않는다.
		meshCollider.enabled = false;
		meshCollider.enabled = true;
	}

	public void makeFanShape( float[] angle )
	{
		float startAngle;					// 원의 시작 각도
		float endAngle;						// 원의 종료 각도
        float pieceAngle = PIECE_ANGLE;		// 1폴리곤의 각도(원만한 원).　
		float radius = FAN_RADIUS;			// 원의 반지름
		
		startAngle = angle[0];
		endAngle = angle[1];

		// --------------------------------------------------------------------
		// 준비
		// --------------------------------------------------------------------

		if ( Mathf.Abs ( startAngle - endAngle ) > 180f )
		{
			// 0도<-> 359도를 넘는다고 간주하여 +360도 한다.
            if ( startAngle < 180f )
			{
				startAngle += 360f;
			}
			if ( endAngle < 180f )
			{
				endAngle += 360f;
			}
		}
		
		Vector3[]	circleVertices;			// 원을 구성하는 각 폴리곤의 항목 좌표.                 
		int[]		circleTriangles;		// 폴리곤의 면 정보(정점 접속 정보)        

		// 각도는 시작 < 종료가 되도록 한다. 
		if ( startAngle > endAngle )
		{
			float tmp = startAngle;
			startAngle = endAngle;
			endAngle = tmp;
		}

		// 삼각형의 수.
		int	triangleNum = (int)Mathf.Ceil(( endAngle - startAngle ) / pieceAngle );

		// 배열을 확보
		circleVertices = new Vector3[triangleNum + 1 + 1];
		circleTriangles = new int[triangleNum*3];

		// --------------------------------------------------------------------
		// 폴리곤을 작성 
		// --------------------------------------------------------------------

		//정점

		circleVertices[0] = Vector3.zero;


		for( int i = 0; i < triangleNum + 1; i++ )
		{

			float currentAngle = startAngle + (float)i*pieceAngle;

			// 마미막 값을 초과하지 않도록
			currentAngle = Mathf.Min( currentAngle, endAngle );

			circleVertices[1 + i] = Quaternion.AngleAxis( currentAngle, Vector3.up ) * Vector3.forward * radius;
		}

		// 인덱스   

		for( int i = 0; i < triangleNum; i++ )
		{
			circleTriangles[i*3 + 0] = 0;
			circleTriangles[i*3 + 1] = i + 1;
			circleTriangles[i*3 + 2] = i + 2;
		}

		// --------------------------------------------------------------------
		// 메시 작성 
		// --------------------------------------------------------------------

		mesh.Clear();
		
		mesh.vertices = circleVertices;
		mesh.triangles = circleTriangles;

		mesh.Optimize();
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		
		meshFilter.mesh = mesh;

		// mesh 를 변경한 후에는 false -> true 로 설정하지 않으면 반영되지 않는다.
		meshCollider.enabled = false;
		meshCollider.enabled = true;
	}

	void OnTriggerEnter( Collider collider )
	{
		scoutingLaser.Lockon( collider );
	}

	void OnTriggerExit()
	{
	}
	
	void OnTriggerStay( Collider collider )
	{
	}

}
