using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DecorationCircleStopper : GamePieceBase {

	const float ZPosDecorationLayer = -5.0f;

	Texture2D BGTexture;
	Mesh GeneratedMesh;

	CirclePatch owner;

	// Use this for initialization
	void Start () {
	}

	public void Generate(float width, float height, float uvScale, Texture2D bgTexture)
	{
		transform.position = new Vector3(0.0f, 0.0f, ZPos);
//		ZPos += ZPosAdd;

		BGTexture = bgTexture;
		
		GeneratedMesh = new Mesh();
		
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> indices = new List<int>();
		
		float halfWidth = width * 0.5f;
		float halfHeight = height * 0.5f;
		
		vertices.Add(new Vector3(-halfWidth, halfHeight));
		vertices.Add(new Vector3(-halfWidth, -halfHeight));
		vertices.Add(new Vector3(halfWidth, -halfHeight));
		vertices.Add(new Vector3(halfWidth, halfHeight));
		uvs.Add(new Vector3(0.0f, uvScale));
		uvs.Add(new Vector3(0.0f, 0.0f));
		uvs.Add(new Vector3(uvScale, 0.0f));
		uvs.Add(new Vector3(uvScale, uvScale));
		indices.Add(2);
		indices.Add(1);
		indices.Add(0);
		indices.Add(0);
		indices.Add(3);
		indices.Add(2);
		GeneratedMesh.vertices = vertices.ToArray();
		GeneratedMesh.uv = uvs.ToArray();
		GeneratedMesh.triangles = indices.ToArray();
		GeneratedMesh.RecalculateNormals();
		GeneratedMesh.RecalculateBounds();
		
		// Setup mesh.
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = GeneratedMesh;
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
		renderer.material.mainTexture = BGTexture;
		renderer.material.shader = Shader.Find("Unlit/Transparent");
	}

	public bool CollidesAgainst(CirclePatch patch)
	{
		float x1 = transform.position.x;
		float y1 = transform.position.y;
		float x2 = patch.transform.position.x;
		float y2 = patch.transform.position.y;
		float r1 = 0.5f;
		float r2 = patch.GetSize() * 1.0f;//SegmentScale;
		
		float distance = ((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1));
		float sumRadius = ((r1 + r2) * (r1 + r2));
		if(distance <= sumRadius)
		{
			owner = patch;
			return true;
		}
		
		return false;
	}

	public void SetCollider(CirclePatch collider)
	{
		owner = collider;
	}

	public override void Place()
	{
		owner.PlaceDecoration(this);
		transform.parent = owner.transform;
		transform.localPosition = new Vector3(0.0f, 0.0f, ZPosDecorationLayer);
	}

	public override void SetPosition(float x, float y)
	{
		transform.position = new Vector3(x, y, transform.position.z);
	}

	// Update is called once per frame
	void Update () {	
	}

}
