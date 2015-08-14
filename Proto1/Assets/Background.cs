using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Background : MonoBehaviour {

	Mesh GeneratedMesh;
	Texture2D BGTexture;

	public void Generate(float halfWidth, float halfHeight, float uvScale, Texture2D bgTexture)
	{
		BGTexture = bgTexture;

		GeneratedMesh = Helpers.GenerateQuad(halfWidth * 2.0f, halfHeight * 2.0f, uvScale);

		// Setup mesh.
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = GeneratedMesh;
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material.shader = Shader.Find("Unlit/Texture");
		meshRenderer.material.mainTexture = BGTexture;
		meshRenderer.material.color = Color.cyan;
	}
}
