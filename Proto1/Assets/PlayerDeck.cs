using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerDeck : MonoBehaviour {

	Mesh GeneratedMesh;
	Texture2D BGTexture;

	Player Owner;
	public int NumDecorations = 0;
	public Stack<CirclePatch.PatchConfig> PatchConfigs;

	List<CirclePatch> CirclePatches;
	List<DecorationCircleStopper> Decorations;


	void Start()
	{
	}
	
	void Update()
	{
	}

	public void Show()
	{
		transform.parent = Camera.main.transform;
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 1.0f);
		gameObject.SetActive(true);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public GamePieceBase GetPiece(Vector2 position)
	{
		// Check patches.
		for(int i = 0; i < CirclePatches.Count; ++i)
		{
			CirclePatch patch = CirclePatches[i];

			float x1 = position.x;
			float y1 = position.y;
			float x2 = patch.transform.position.x;
			float y2 = patch.transform.position.y;
			float r1 = 0.1f;
			float r2 = patch.GetSize() * 1.0f;//SegmentScale;
			
			float distance = ((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1));
			float sumRadius = ((r1 + r2) * (r1 + r2));
			if(distance <= sumRadius)
			{
				// Add a new patch to the visible hand.
				if(PatchConfigs.Count > 0)
				{
					CirclePatch.PatchConfig patchConfig = PatchConfigs.Pop();
					CirclePatch newPatch = CreatePatch(patchConfig);
					newPatch.transform.parent = transform;
					newPatch.transform.localPosition = new Vector3(patch.transform.localPosition.x, patch.transform.localPosition.y, patch.transform.localPosition.z);
					CirclePatches.Add(newPatch);
				}

				// Remove from deck.
				CirclePatches.RemoveAt(i);
				patch.transform.parent = null;

				// And return the selected piece.
				return patch;
			}
		}

		// Check decorations.
		for(int i = 0; i < Decorations.Count; ++i)
		{
			DecorationCircleStopper decoration = Decorations[i];
			
			float x1 = position.x;
			float y1 = position.y;
			float x2 = decoration.transform.position.x;
			float y2 = decoration.transform.position.y;
			float r1 = 0.1f;
			float r2 = 0.5f;
			
			float distance = ((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1));
			float sumRadius = ((r1 + r2) * (r1 + r2));
			if(distance <= sumRadius)
			{
				// Add a new patch to the visible hand.
				if(NumDecorations > 0)
				{
					DecorationCircleStopper newDecoration = CreateDecorationCircleStopper();
					newDecoration.transform.parent = transform;
					newDecoration.transform.localPosition = new Vector3(decoration.transform.localPosition.x, decoration.transform.localPosition.y, decoration.transform.localPosition.z);
					Decorations.Add(newDecoration);
					--NumDecorations;
				}
				
				// Remove from deck.
				Decorations.RemoveAt(i);
				decoration.transform.parent = null;
				
				// And return the selected piece.
				decoration.active = true;
				return decoration;
			}
		}
		
		return null;
	}

	CirclePatch CreatePatch(CirclePatch.PatchConfig patchConfig)
	{
		GameObject patchObject = new GameObject(gameObject.name + "_Patch");
		CirclePatch circlePatch = patchObject.AddComponent<CirclePatch>();
		circlePatch.Generate(patchConfig, Owner.PatternTextures, Owner.Colors, Owner.ComplementColor);
		circlePatch.SetOwner(Owner);
		return circlePatch;
	}
	
	DecorationCircleStopper CreateDecorationCircleStopper()
	{
		GameObject decorationObject = new GameObject(gameObject.name + "_Decoration");
		DecorationCircleStopper decorationCircleStopper = decorationObject.AddComponent<DecorationCircleStopper>();
		Texture2D decorationTexture = Owner.Decorations[Random.Range(0, Owner.Decorations.Length)];
		decorationCircleStopper.Generate(1.0f, 1.0f, 1.0f, decorationTexture);
		decorationCircleStopper.SetOwner(Owner);
		return decorationObject.GetComponent<DecorationCircleStopper>();
	}
	
	public void Generate(int numPatches, int numDecorations, int numVisiblePatches, Player owner)
	{
		Owner = owner;

		// Generate background.
		BGTexture = null;//bgTexture;

		GeneratedMesh = new Mesh();
		
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> indices = new List<int>();

		float width = 0.1f + (numVisiblePatches * 2.0f) + 0.1f + (1.0f * 2.0f) + 0.1f;
		float height = 0.1f + 2.0f + 0.1f;
		float halfWidth = width / 2.0f;
		float halfHeight = height / 2.0f;

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
		renderer.material.color = new Color(0.0f, 0.0f, 0.0f, 0.4f);
		renderer.material.shader = Shader.Find("Transparent/Diffuse");

		// Generate patch configs.
		NumDecorations = numDecorations;
		PatchConfigs = new Stack<CirclePatch.PatchConfig>();
		for(int i = 0; i < numPatches; ++i)
		{
			int segments = Random.Range(2, 7);
			PatchConfigs.Push(new CirclePatch.PatchConfig(segments, 1.0f, owner.Colors.Length, CirclePatch.MAX_PATTERNS));
		}

		// Create patches.
		CirclePatches = new List<CirclePatch>();
		float posx = -halfWidth + 1.0f;
		posx += 0.1f;
		for(int i = 0; i < numVisiblePatches; ++i)
		{
			CirclePatch.PatchConfig patchConfig = PatchConfigs.Pop();
			CirclePatch circlePatch = CreatePatch(patchConfig);
			circlePatch.transform.parent = transform;
			circlePatch.transform.localPosition = new Vector3(posx, 0.0f, 0.0f);
			CirclePatches.Add(circlePatch);
			posx += 2.0f;
		}

		// Create decoration.
		Decorations = new List<DecorationCircleStopper>();
		posx += 0.1f;
		for(int i = 0; i < 1; ++i)
		{
			DecorationCircleStopper decoration = CreateDecorationCircleStopper();
			decoration.transform.parent = transform;
			decoration.transform.localPosition = new Vector3(posx, 0.0f, 0.0f);
			Decorations.Add(decoration);
			--NumDecorations;
			posx += 2.0f;
		}

		// Start hidden.
		Hide();
	}
}
