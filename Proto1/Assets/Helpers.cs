using UnityEngine;
using System.Collections;

public static class Helpers
{
	public static Mesh GenerateQuad(float width, float height, float uvScale)
	{
		return GenerateQuad(width, height, uvScale, uvScale);
	}

	public static Mesh GenerateQuad(float width, float height, float uScale, float vScale)
	{
		Mesh outMesh = new Mesh();
		
		Vector3[] vertices = new Vector3[4];
		Vector2[] uvs = new Vector2[4];
		int[] indices = new int[6];
		
		float halfWidth = width * 0.5f;
		float halfHeight = height * 0.5f;
		
		vertices[0] = new Vector3(-halfWidth, halfHeight);
		vertices[1] = new Vector3(-halfWidth, -halfHeight);
		vertices[2] = new Vector3(halfWidth, -halfHeight);
		vertices[3] = new Vector3(halfWidth, halfHeight);
		uvs[0] = new Vector3(0.0f, vScale);
		uvs[1] = new Vector3(0.0f, 0.0f);
		uvs[2] = new Vector3(uScale, 0.0f);
		uvs[3] = new Vector3(uScale, vScale);
		indices[0] = 2;
		indices[1] = 1;
		indices[2] = 0;
		indices[3] = 0;
		indices[4] = 3;
		indices[5] = 2;
		outMesh.vertices = vertices;
		outMesh.uv = uvs;
		outMesh.triangles = indices;
		outMesh.RecalculateNormals();
		outMesh.RecalculateBounds();

		return outMesh;
	}
}
