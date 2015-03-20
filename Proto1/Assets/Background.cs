using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Background : MonoBehaviour {

	Mesh GeneratedMesh;
	Texture2D BGTexture;

	public void Generate(float halfWidth, float halfHeight, float uvScale, Texture2D bgTexture)
	{
		BGTexture = bgTexture;

		GeneratedMesh = new Mesh();

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> indices = new List<int>();

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
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material.shader = Shader.Find("Unlit/Texture");
		meshRenderer.material.mainTexture = BGTexture;
		meshRenderer.material.color = Color.cyan;
	}
}
