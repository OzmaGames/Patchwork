using UnityEditor;
using UnityEngine;
using System.Collections;

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
		SaveMeshToFile(Helpers.GenerateQuad(Width, Height, UVScale), "", false, true); 
	}
	
	void OnWizardUpdate()
	{
		helpString = "Please enter settings for the quad";
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
