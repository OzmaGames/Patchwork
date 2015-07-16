using UnityEditor;
using UnityEngine;
using System.Collections;

public static class Helpers
{
	public static Mesh GenerateQuad(float width, float height, float uvScale)
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
		uvs[0] = new Vector3(0.0f, uvScale);
		uvs[1] = new Vector3(0.0f, 0.0f);
		uvs[2] = new Vector3(uvScale, 0.0f);
		uvs[3] = new Vector3(uvScale, uvScale);
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

	[MenuItem("CONTEXT/MeshFilter/Save Mesh...")]
	public static void SaveMeshToFile(MenuCommand menuCommand)
	{
		MeshFilter meshFilter = menuCommand.context as MeshFilter;
		Mesh mesh = meshFilter.sharedMesh;
		SaveMeshToFile(mesh, mesh.name, false, true);
	}
	
	public static void SaveMeshToFile(Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh)
	{
		string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
		if(string.IsNullOrEmpty(path))
		{
			return;
		}
		path = FileUtil.GetProjectRelativePath(path);

		Mesh meshToSave = makeNewInstance ? Object.Instantiate(mesh) as Mesh : mesh;
		if(optimizeMesh)
		{
			meshToSave.Optimize();
		}
		
		AssetDatabase.CreateAsset(meshToSave, path);
		AssetDatabase.SaveAssets();
	}
}

public class WizardGenerateQuad : ScriptableWizard
{
	public float Width = 1.0f;
	public float Height = 1.0f;
	public float UVScale = 1.0f;
	
	[MenuItem("Assets/Create/Custom Quad")]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<WizardGenerateQuad>("Create Custom Quad", "Create");
	}
	
	void OnWizardCreate()
	{
		Helpers.SaveMeshToFile(Helpers.GenerateQuad(Width, Height, UVScale), "", false, true); 
	}
	
	void OnWizardUpdate()
	{
		helpString = "Please enter settings for the quad";
	}
}

