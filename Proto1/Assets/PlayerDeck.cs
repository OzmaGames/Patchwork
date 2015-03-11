using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerDeck : MonoBehaviour {

	Mesh GeneratedMesh;
	Texture2D BGTexture;

	Player Owner;
	public int NumPatches = 0;
	public int NumDecorations = 0;
	public int NumPatchesOnHand = 0;
	public int NumDecorationsOnHand = 0;
	public Stack<CirclePatch.PatchConfig> PatchConfigs;
	
	class HandSlot
	{
		public Vector2 Pos = Vector2.zero;
		public bool Free = true;
		public GamePieceBase Piece = null;

		public HandSlot()
		{
		}
	}
	HandSlot[] ActiveHand;


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

	public void ActivateTurn()
	{
		FillActiveHand();
	}

	public GamePieceBase GetPiece(Vector2 position)
	{
		for(int i = 0; i < ActiveHand.Length; ++i)
		{
			HandSlot slot = ActiveHand[i];
			if((!slot.Free) && (slot.Piece != null))
			{
				CirclePatch patch = slot.Piece.GetComponent<CirclePatch>();
				if(patch != null)
				{
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
						// Remove from hand.
						slot.Free = true;
						slot.Piece = null;
						patch.transform.parent = null;
						
						// And return the selected piece.
						return patch;
					}
				}

				DecorationCircleStopper decoration = slot.Piece.GetComponent<DecorationCircleStopper>();
				if(decoration != null)
				{
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
						// Remove from hand.
						slot.Free = true;
						slot.Piece = null;
						decoration.transform.parent = null;
						
						// And return the selected piece.
						decoration.isActive = true;
						return decoration;
					}
				}
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

	public bool AddToHand(GamePieceBase piece)
	{
		bool roomToAdd = false;

		CirclePatch patch = piece.GetComponent<CirclePatch>();
		if(patch != null)
		{
			for(int i = 0; i < NumPatchesOnHand; ++i)
			{
				HandSlot slot = ActiveHand[i];
				if(slot.Free)
				{
					piece.transform.parent = transform;
					piece.transform.localPosition = new Vector3(slot.Pos.x, slot.Pos.y, 0.0f);
					slot.Free = false;
					slot.Piece = piece;
					roomToAdd = true;
					break;
				}
			}
		}

		DecorationCircleStopper decoration = piece.GetComponent<DecorationCircleStopper>();
		if(decoration != null)
		{
			for(int i = 0; i < NumDecorationsOnHand; ++i)
			{
				HandSlot slot = ActiveHand[NumPatchesOnHand + i];
				if(slot.Free)
				{
					piece.transform.parent = transform;
					piece.transform.localPosition = new Vector3(slot.Pos.x, slot.Pos.y, 0.0f);
					slot.Free = false;
					slot.Piece = piece;
					roomToAdd = true;
					break;
				}
			}
		}

		return roomToAdd;
	}
	
	void FillActiveHand()
	{
		// Fill patches.
		for(int i = 0; i < NumPatchesOnHand; ++i)
		{
			HandSlot slot = ActiveHand[i];
			if((slot.Free) && (PatchConfigs.Count > 0))
			{
				CirclePatch.PatchConfig patchConfig = PatchConfigs.Pop();
				CirclePatch newPatch = CreatePatch(patchConfig);
				newPatch.transform.parent = transform;
				newPatch.transform.localPosition = new Vector3(slot.Pos.x, slot.Pos.y, 0.0f);
				slot.Free = false;
				slot.Piece = newPatch;
			}
		}

		// Fill decorations.
		for(int i = 0; i < NumDecorationsOnHand; ++i)
		{
			HandSlot slot = ActiveHand[NumPatchesOnHand + i];
			if((slot.Free) && (NumDecorations > 0))
			{
				--NumDecorations;
				DecorationCircleStopper newDecoration = CreateDecorationCircleStopper();
				newDecoration.transform.parent = transform;
				newDecoration.transform.localPosition = new Vector3(slot.Pos.x, slot.Pos.y, 0.0f);
				slot.Free = false;
				slot.Piece = newDecoration;
			}
		}
	}

	public void Generate(int numPatches, int numDecorations, int numPatchesOnHand, int numDecorationsOnHand, Player owner)
	{
		Owner = owner;

		// Generate background.
		BGTexture = null;//bgTexture;

		GeneratedMesh = new Mesh();
		
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> indices = new List<int>();

		float width = 0.1f + (numPatchesOnHand * 2.0f) + 0.1f + (1.0f * 2.0f) + 0.1f;
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
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material.mainTexture = BGTexture;
		meshRenderer.material.color = new Color(0.0f, 0.0f, 0.0f, 0.4f);
		meshRenderer.material.shader = Shader.Find("Transparent/Diffuse");

		// Generate patch configs.
		NumPatches = numPatches;
		NumDecorations = numDecorations;
		NumPatchesOnHand = numPatchesOnHand;
		NumDecorationsOnHand = numDecorationsOnHand;
		PatchConfigs = new Stack<CirclePatch.PatchConfig>();
		for(int i = 0; i < NumPatches; ++i)
		{
			int segments = Random.Range(2, 7);
			PatchConfigs.Push(new CirclePatch.PatchConfig(segments, 1.0f, owner.Colors.Length, CirclePatch.MAX_PATTERNS));
		}

		// Setup active hand.
		ActiveHand = new HandSlot[NumPatchesOnHand + NumDecorationsOnHand];
		float posx = -halfWidth + 1.0f;
		posx += 0.1f;
		for(int i = 0; i < NumPatchesOnHand; ++i)
		{
			ActiveHand[i] = new HandSlot();
			ActiveHand[i].Pos = new Vector2(posx, 0.0f);
			posx += 2.0f;
		}
		posx += 0.1f;
		for(int i = 0; i < NumDecorationsOnHand; ++i)
		{
			ActiveHand[NumPatchesOnHand + i] = new HandSlot();
			ActiveHand[NumPatchesOnHand + i].Pos = new Vector2(posx, 0.0f);
			posx += 2.0f;
		}

		// Start hidden.
		Hide();
	}
}
