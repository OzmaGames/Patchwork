using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Background : MonoBehaviour {

	Mesh GeneratedMesh;
	Texture2D BGTexture;

	// Use this for initialization
	void Start () {
	
	}

	public void Generate(float width, float height, float uvScale, Texture2D bgTexture)
	{
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
		renderer.material.shader = Shader.Find("Unlit/Texture");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
