using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SinglePlayerPlayfield : Playfield
{
	struct Cell
	{
		public Rect Rect;
		public Symbol Symbol;
	}
	List<Cell> PlayfieldCells;

	bool Dirty = false;
	float HalfWidth = 0.0f;
	float HalfHeight = 0.0f;
	
	Mesh GeneratedMesh;
	Texture2D BGTexture;
	
	
	class PlacedPatch
	{
		public PlacedPatch(Player owner, CirclePatch patch)
		{
			Owner = owner;
			Patch = patch;
		}
		
		public Player Owner;
		public CirclePatch Patch;
	}
	List<PlacedPatch> Patches = new List<PlacedPatch>();
	
	void Start()
	{
	}
	
	void OnDestroy()
	{
		for(int i = 0; i < Patches.Count; ++i)
		{
			Destroy(Patches[i].Patch.gameObject);
		}
		Patches.Clear();
		Patches = null;
		GeneratedMesh = null;
		BGTexture = null;
		PlayfieldCells = null; 
	}

	Material highlightedCellMaterial;
	Color highlightColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
	void Update()
	{
		CheckForCollision();

		Vector3 mousePosition = Input.mousePosition;
		
		// Make sure mouse position is inside screen.
		mousePosition.x = Mathf.Clamp(mousePosition.x, 0.0f, Screen.width);
		mousePosition.y = Mathf.Clamp(mousePosition.y, 0.0f, Screen.height);
		
		Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
		mouseWorldPosition.z = 0.0f;

		Material cellMaterial = null;
		for(int i = 0; i < PlayfieldCells.Count; ++i)
		{
			Cell cell = PlayfieldCells[i];
			if(cell.Rect.Contains(mouseWorldPosition))
			{
				MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
				cellMaterial = meshRenderer.materials[i];
				break;
			}
		}

		if(cellMaterial != highlightedCellMaterial)
		{
			if(highlightedCellMaterial != null)
			{
				highlightedCellMaterial.SetColor("_AddColor", Color.clear);
			}
			if(cellMaterial != null)
			{
				highlightedCellMaterial = cellMaterial;
				highlightedCellMaterial.SetColor("_AddColor", highlightColor);
			}
		}

		////////////////////////////////////////////////////////////////
		Dirty = true;
		////////////////////////////////////////////////////////////////
		
		if(Dirty)
		{
			// Regenerate mesh.
			//GenerateMesh();
			Dirty = false;
		}
	}

	void CheckForCollision()
	{
	}


	public override bool CanPlaceAt(Player player, GamePieceBase piece, Vector3 pos, out List<GamePieceBase> collidedPieces)
	{
		collidedPieces = new List<GamePieceBase>();
		return true;
	}
	
	public override void GetCollision(GamePieceBase piece, out List<GamePieceBase> collidedPieces)
	{
		collidedPieces = new List<GamePieceBase>();
	}

	public override void Place(Player player, GamePieceBase piece)
	{
	}

	public override bool IsInsideGrid(Vector3 pos)
	{
		return false;
	}

	public override bool CanPlaceDecoration()
	{
		return false;
	}

	public override void ShowSymbols()
	{
		for(int p = 0; p < Patches.Count; ++p)
		{
			Patches[p].Patch.SetShowSymbol(true);
		}
	}
	
	public override void HideSymbols()
	{
		for(int p = 0; p < Patches.Count; ++p)
		{
			Patches[p].Patch.SetShowSymbol(false);
		}
	}	

	public override void Generate(float halfWidth, float halfHeight, float uvScale, Texture2D bgTexture)
	{
		HalfWidth = halfWidth;
		HalfHeight = halfHeight;
		BGTexture = bgTexture;	

		float scale = CirclePatch.SegmentScale * 2.0f;
		float size = scale;

		// Generate test playfield cells.
		PlayfieldCells = new List<Cell>();
		size = 3.0f * scale;
		PlayfieldCells.Add(GenerateCell(new Rect(-9.0f, -7.0f, size, size), null));
		size = 2.0f * scale;
		PlayfieldCells.Add(GenerateCell(new Rect(-9.0f, -1.0f, size, size), Symbol.Instantiate(Symbol.GetRandomSymbolType()).GetComponent<Symbol>()));
		size = 1.0f * scale;
		PlayfieldCells.Add(GenerateCell(new Rect(-5.0f, -1.0f, size, size), null));
		size = 2.0f * scale;
		PlayfieldCells.Add(GenerateCell(new Rect(-9.0f, 3.0f, size, size), null));
		size = 3.0f * scale;
		PlayfieldCells.Add(GenerateCell(new Rect(-5.0f, 1.0f, size, size), Symbol.Instantiate(Symbol.GetRandomSymbolType()).GetComponent<Symbol>()));
		size = 2.0f * scale;
		PlayfieldCells.Add(GenerateCell(new Rect(-3.0f, -3.0f, size, size), null));
		size = 3.0f * scale;
		PlayfieldCells.Add(GenerateCell(new Rect(-3.0f, -9.0f, size, size), null));
		size = 1.0f * scale;
		PlayfieldCells.Add(GenerateCell(new Rect(1.0f, -3.0f, size, size), null));
		size = 4.0f * scale;
		PlayfieldCells.Add(GenerateCell(new Rect(1.0f, -1.0f, size, size), Symbol.Instantiate(Symbol.GetRandomSymbolType()).GetComponent<Symbol>()));
		size = 3.0f * scale;
		PlayfieldCells.Add(GenerateCell(new Rect(3.0f, -7.0f, size, size), null));

		// Generate mesh from playfield cells.
		GenerateMeshFromCell();

	}

	Cell GenerateCell(Rect rect, Symbol symbol)
	{
		Cell cell = new Cell();
		cell.Rect = rect;
		cell.Symbol = symbol;
		return cell;
	}
	
	void GenerateMeshFromCell()
	{
		Vector3[] vertices = new Vector3[PlayfieldCells.Count * 6];
		Vector2[] uvs = new Vector2[PlayfieldCells.Count * 6];
		Color[] colors = new Color[PlayfieldCells.Count * 6];
		int[][] indices = new int[PlayfieldCells.Count][];
		
		// Generate cells.
		int v = 0;
		int index = 0;
		for(int i = 0; i < PlayfieldCells.Count; ++i)
		{
			Cell cell = PlayfieldCells[i];
			
			float xPos = cell.Rect.position.x;
			float yPos = cell.Rect.position.y;
			float width = cell.Rect.width;
			float height = cell.Rect.height;
			Color color = new Color(Random.value, Random.value, Random.value, 0.2f);

			indices[i] = new int[6];

			vertices[v + 0].Set(xPos, yPos, 0.0f);
			vertices[v + 1].Set(xPos, yPos + height, 0.0f);
			vertices[v + 2].Set(xPos + width, yPos, 0.0f);
			uvs[v + 0].Set(0.0f, 0.0f);
			uvs[v + 1].Set(0.0f, 1.0f);
			uvs[v + 2].Set(1.0f, 0.0f);
			colors[v + 0].r = color.r;
			colors[v + 0].g = color.g;
			colors[v + 0].b = color.b;
			colors[v + 0].a = color.a;
			colors[v + 1].r = color.r;
			colors[v + 1].g = color.g;
			colors[v + 1].b = color.b;
			colors[v + 1].a = color.a;
			colors[v + 2].r = color.r;
			colors[v + 2].g = color.g;
			colors[v + 2].b = color.b;
			colors[v + 2].a = color.a;
			v += 3;
			indices[i][0] = index++;
			indices[i][1] = index++;
			indices[i][2] = index++;

			vertices[v + 0].Set(xPos, yPos + height, 0.0f);
			vertices[v + 1].Set(xPos + width, yPos + height, 0.0f);
			vertices[v + 2].Set(xPos + width, yPos, 0.0f);
			uvs[v + 0].Set(0.0f, 1.0f);
			uvs[v + 1].Set(1.0f, 1.0f);
			uvs[v + 2].Set(1.0f, 0.0f);
			colors[v + 0].r = color.r;
			colors[v + 0].g = color.g;
			colors[v + 0].b = color.b;
			colors[v + 0].a = color.a;
			colors[v + 1].r = color.r;
			colors[v + 1].g = color.g;
			colors[v + 1].b = color.b;
			colors[v + 1].a = color.a;
			colors[v + 2].r = color.r;
			colors[v + 2].g = color.g;
			colors[v + 2].b = color.b;
			colors[v + 2].a = color.a;
			v += 3;
			indices[i][3] = index++;
			indices[i][4] = index++;
			indices[i][5] = index++;
		}

		GeneratedMesh = new Mesh();
		GeneratedMesh.vertices = vertices;
		GeneratedMesh.uv = uvs;
		GeneratedMesh.colors = colors;
		GeneratedMesh.subMeshCount = indices.Length;
		for(int i = 0; i < GeneratedMesh.subMeshCount; ++i)
		{
			GeneratedMesh.SetTriangles(indices[i], i);
		}
		//GeneratedMesh.triangles = indices;
		GeneratedMesh.RecalculateNormals();
		GeneratedMesh.RecalculateBounds();

		// Setup mesh.
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = GeneratedMesh;
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.materials = new Material[GeneratedMesh.subMeshCount];
		Material material;
		for(int s = 0; s < GeneratedMesh.subMeshCount; ++s)
		{
			material = meshRenderer.materials[s];
			material.shader = Shader.Find("Custom/FabricPiece");
			material.mainTexture = BGTexture;//RandomPatternTextures[Random.Range(0,6)];
			material.SetColor("_AddColor", Color.clear);
		}
	}

	public override void ActivatePlayer(Player player, bool hideOthersSymbols)
	{
	}

	public override bool IsDone()
	{
		return false;
	}
}
