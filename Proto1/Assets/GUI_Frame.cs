using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GUI_Frame : GUI_Base
{

	public float Width = 0.0f;
	public float Height = 0.0f;

	Mesh GUIMesh;
	Material GUIMaterial;
	
	void Start()
	{
	}

	void ReDraw()
	{
		if((Width > 0.0f) && (Height > 0.0f))
		{
			// Re-generate mesh.
			GUIMesh = new Mesh();

			List<Vector3> vertices = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();
			List<int> indices = new List<int>();
			
			float halfWidth = Width * 0.5f;
			float halfHeight = Height * 0.5f;
			
			vertices.Add(new Vector3(-halfWidth, halfHeight));
			vertices.Add(new Vector3(-halfWidth, -halfHeight));
			vertices.Add(new Vector3(halfWidth, -halfHeight));
			vertices.Add(new Vector3(halfWidth, halfHeight));
			uvs.Add(new Vector3(0.0f, 1.0f));
			uvs.Add(new Vector3(0.0f, 0.0f));
			uvs.Add(new Vector3(1.0f, 0.0f));
			uvs.Add(new Vector3(1.0f, 1.0f));
			indices.Add(2);
			indices.Add(1);
			indices.Add(0);
			indices.Add(0);
			indices.Add(3);
			indices.Add(2);
			GUIMesh.vertices = vertices.ToArray();
			GUIMesh.uv = uvs.ToArray();
			GUIMesh.triangles = indices.ToArray();
			GUIMesh.RecalculateNormals();
			GUIMesh.RecalculateBounds();

			// Create material.
			GUIMaterial = new Material(Shader.Find("Unlit/Texture"));
			GUIMaterial.mainTexture = null;

			// Setup mesh.
			if(UnityEditor.EditorApplication.isPlaying)
			{
				MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
				if(meshFilter == null)
				{
					meshFilter = gameObject.AddComponent<MeshFilter>();
				}
				meshFilter.mesh = GUIMesh;
				MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
				if(renderer == null)
				{
					renderer = gameObject.AddComponent<MeshRenderer>();
				}
				renderer.material = GUIMaterial;
			}
		}

	}
	
	void Update()
	{
		if(IsDirty)
		{
			ReDraw();
			IsDirty = false;
		}
		if(!UnityEditor.EditorApplication.isPlaying)
		{
			Graphics.DrawMesh(GUIMesh, transform.localToWorldMatrix, GUIMaterial, 0);
		}
	}
}
