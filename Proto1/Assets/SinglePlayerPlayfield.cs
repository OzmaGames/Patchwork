using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*using System.IO;
using System.Runtime.Serialization.Formatters.Binary;*/

public class SinglePlayerPlayfield : Playfield
{
	List<CellPiece> PlayfieldCells;

	bool Dirty = false;
	float HalfWidth = 0.0f;
	float HalfHeight = 0.0f;
	
	Texture2D BGTexture;
	Texture2D[] CellTextures;
	Game.PlayerPalette[] Palettes;

	
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
		for(int i = 0; i < PlayfieldCells.Count; ++i)
		{
			Destroy(PlayfieldCells[i].gameObject);
		}
		PlayfieldCells.Clear();
		PlayfieldCells = null;

		BGTexture = null;
	}

	/*void SaveLevel()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create (Application.persistentDataPath + "/savedGames.txt");
		//bf.Serialize(file, SaveLoad.savedGames);
		file.Close();
	}*/
	
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
		CellPiece cell;
		int cellIndex = FindCell(mouseWorldPosition, out cell);
		if(cellIndex >= 0)
		{
			MeshRenderer meshRenderer = cell.gameObject.GetComponent<MeshRenderer>();
			cellMaterial = meshRenderer.material;
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
		for(int i = 0; i < PlayfieldCells.Count; ++i)
		{
			CellPiece cell = PlayfieldCells[i];
			if(!cell.IsDone)
			{
				CirclePatch piece = cell.Piece as CirclePatch;
				if(piece != null)
				{
					if(piece.GetSize() > cell.Size)
					{
						piece.SetCollided(true);
						cell.IsDone = true;
					}
					else if(piece.HasStoppedGrowing())
					{
						cell.IsDone = true;
					}
				}
			}
		}
	}


	public override bool CanPlaceAt(Player player, GamePieceBase piece, Vector3 pos, out List<GamePieceBase> collidedPieces)
	{
		collidedPieces = new List<GamePieceBase>();
		
		if(!IsInsideGrid(pos))
		{
			return false;
		}
		
		if(piece.GetComponent<DecorationCircleStopper>() != null)
		{
			bool hasCollided = false;
			CirclePatch bestCollider = null;
			DecorationCircleStopper decoration = piece.GetComponent<DecorationCircleStopper>();
			for(int p = 0; p < Patches.Count; ++p)
			{
				CirclePatch patch = Patches[p].Patch;
				// Enabled placing decoration on completed patches.
				if(/*(!patch.HasStoppedGrowing()) &&*/ decoration.CollidesAgainst(patch))
				{
					if(patch.GetDecoration() == null)
					{
						if((bestCollider == null) || (bestCollider.transform.position.z > patch.transform.position.z))
						{
							bestCollider = patch;
							hasCollided = true;
						}
					}
					else
					{
						collidedPieces.Add(patch.GetDecoration());
					}
				}
			}
			
			decoration.SetCollider(bestCollider);
			return hasCollided;
		}

		return true;
	}
	
	public override void GetCollision(GamePieceBase piece, out List<GamePieceBase> collidedPieces)
	{
		collidedPieces = new List<GamePieceBase>();
		
		Vector3 pos = piece.transform.position;
		if(!IsInsideGrid(pos))
		{
			return;
		}
	}

	public override void Place(Player player, GamePieceBase piece)
	{
		CirclePatch circlePatch = piece.GetComponent<CirclePatch>();
		if(circlePatch != null)
		{
			CellPiece cell;
			int cellIndex = FindCell(piece.transform.position, out cell);
			if(cellIndex >= 0)
			{
				cell.Piece = circlePatch;
				piece.SetPosition(cell.transform.position.x, cell.transform.position.y);
				//piece.SetPosition(cell.Rect.center.x, cell.Rect.center.y);
				if(!cell.BelongsToPlayer)
				{
					if(cell.Symbol)
					{
						switch(circlePatch.GetSymbol().Compare(cell.Symbol))
						{
						case Symbol.CompareResult.Win:
							Debug.Log("NOT MY TYPE - BUT THAT'S OK, I WIN!");
							Material material = cell.GetComponent<MeshRenderer>().material;
							material.SetColor("_BaseColor1", player.Colors[Random.Range(0, player.Colors.Length)].colorKeys[0].color);
							material.SetColor("_BaseColor2", player.Colors[Random.Range(0, player.Colors.Length)].colorKeys[1].color);
							break;
						case Symbol.CompareResult.Draw:
							Debug.Log("NOT MY TYPE - BUT WE LOOK THE SAME!");
							break;
						case Symbol.CompareResult.Lose:
							Debug.Log("NOT MY TYPE - WHY DID YOU PLACE ME HERE?!");
							break;
						}
					}
					else
					{
						Debug.Log("NOT MY TYPE!");
					}
				}
			}

			CirclePatch patch = piece.GetComponent<CirclePatch>();
			Patches.Add(new PlacedPatch(player, patch));
		}
		piece.Place();
	}

	int FindCell(Vector2 pos, out CellPiece foundCell)
	{
		foundCell = null;
		for(int i = 0; i < PlayfieldCells.Count; ++i)
		{
			CellPiece cell = PlayfieldCells[i];
			if(cell.Contains(pos))
			{
				foundCell = cell;
				return i;
			}
		}
		return -1;
	}

	public override bool IsInsideGrid(Vector3 pos)
	{
		if((Mathf.Abs(pos.x) >= HalfWidth) || (Mathf.Abs(pos.y) >= HalfHeight))
		{
			return false;
		}
		return true;
	}

	public override bool CanPlaceDecoration()
	{
		return true;
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

	class PlayfieldData
	{
		public class Cell
		{
			public Vector2 Pos;
			public float Size;
			public bool NoSymbol;
			public Symbol.SymbolTypes Symbol;
			public bool BelongToPlayer;

			public Cell(Vector2 pos, float size, bool belongToPlayer)
			{
				Pos = pos;
				Size = size;
				NoSymbol = true;
				BelongToPlayer = belongToPlayer;
			}
			public Cell(Vector2 pos, float size, Symbol.SymbolTypes symbol, bool belongToPlayer)
			{
				Pos = pos;
				Size = size;
				NoSymbol = false;
				Symbol = symbol;
				BelongToPlayer = belongToPlayer;
			}
		}

		public Cell[] Cells;

		public PlayfieldData(Cell[] cells)
		{
			Cells = cells;
		}
	}
	PlayfieldData data = new PlayfieldData(new PlayfieldData.Cell[10] {
		new PlayfieldData.Cell(new Vector2(-9.0f, -7.0f), 3.0f, true),
		new PlayfieldData.Cell(new Vector2(-9.0f, -1.0f), 2.0f, Symbol.SymbolTypes.Needle, false),
		new PlayfieldData.Cell(new Vector2(-5.0f, -1.0f), 1.0f, true),
		new PlayfieldData.Cell(new Vector2(-9.0f, 3.0f), 2.0f, true),
		new PlayfieldData.Cell(new Vector2(-5.0f, 1.0f), 3.0f, Symbol.SymbolTypes.Scissor, true),
		new PlayfieldData.Cell(new Vector2(-3.0f, -3.0f), 2.0f, true),
		new PlayfieldData.Cell(new Vector2(-3.0f, -9.0f), 3.0f, true),
		new PlayfieldData.Cell(new Vector2(1.0f, -3.0f), 1.0f, true),
		new PlayfieldData.Cell(new Vector2(1.0f, -1.0f), 4.0f, Symbol.SymbolTypes.Thread, false),
		new PlayfieldData.Cell(new Vector2(3.0f, -7.0f), 3.0f, false)});

	public void Generate(float halfWidth, float halfHeight, float uvScale, Texture2D bgTexture, Texture2D[] cellTextures, Game.PlayerPalette[] palettes)
	{
		HalfWidth = halfWidth;
		HalfHeight = halfHeight;
		BGTexture = bgTexture;
		CellTextures = cellTextures;
		Palettes = palettes;

		// Generate test playfield cells.
		PlayfieldCells = new List<CellPiece>();
		for(int i = 0; i < data.Cells.Length; ++i)
		{
			PlayfieldData.Cell cell = data.Cells[i];
			PlayfieldCells.Add(GenerateCell(cell.Pos, cell.Size, cell.NoSymbol ? null : Symbol.Instantiate(cell.Symbol, Symbol.SymbolColor.White).GetComponent<Symbol>(), cell.BelongToPlayer));
		}
		/*PlayfieldCells.Add(GenerateCell(new Vector2(-9.0f, -7.0f), 3.0f, null, true));
		PlayfieldCells.Add(GenerateCell(new Vector2(-9.0f, -1.0f), 2.0f, Symbol.Instantiate(Symbol.GetRandomSymbolType()).GetComponent<Symbol>(), false));
		PlayfieldCells.Add(GenerateCell(new Vector2(-5.0f, -1.0f), 1.0f, null, true));
		PlayfieldCells.Add(GenerateCell(new Vector2(-9.0f, 3.0f), 2.0f, null, true));
		PlayfieldCells.Add(GenerateCell(new Vector2(-5.0f, 1.0f), 3.0f, Symbol.Instantiate(Symbol.GetRandomSymbolType()).GetComponent<Symbol>(), true));
		PlayfieldCells.Add(GenerateCell(new Vector2(-3.0f, -3.0f), 2.0f, null, true));
		PlayfieldCells.Add(GenerateCell(new Vector2(-3.0f, -9.0f), 3.0f, null, true));
		PlayfieldCells.Add(GenerateCell(new Vector2(1.0f, -3.0f), 1.0f, null, true));
		PlayfieldCells.Add(GenerateCell(new Vector2(1.0f, -1.0f), 4.0f, Symbol.Instantiate(Symbol.GetRandomSymbolType()).GetComponent<Symbol>(), false));
		PlayfieldCells.Add(GenerateCell(new Vector2(3.0f, -7.0f), 3.0f, null, false));*/

		// Generate mesh from playfield cells.
		//GenerateMeshFromCell();

	}

	CellPiece GenerateCell(Vector2 pos, float size, Symbol symbol, bool belongsToPlayer)
	{
		float rectSize = size * (CirclePatch.SegmentScale * 2.0f);
		
		GameObject cellPieceObj = new GameObject("CellPiece_" + PlayfieldCells.Count);
		CellPiece cellPiece = cellPieceObj.AddComponent<CellPiece>();
		cellPiece.Generate(size, symbol, belongsToPlayer, belongsToPlayer ? Palettes[0] : Palettes[1], CellTextures[0]);
		cellPieceObj.transform.position = new Vector3(pos.x + (rectSize / 2.0f), pos.y + (rectSize / 2.0f), Game.ZPosAdd * 0.25f);

/*		Cell cell = new Cell();
		cell.IsDone = false;
		cell.Rect = new Rect(pos.x, pos.y, rectSize, rectSize);
		cell.Size = size;
		cell.Symbol = symbol;
		cell.BelongsToPlayer = belongsToPlayer;*/
		if(symbol != null)
		{
			symbol.transform.SetParent(cellPieceObj.transform, false);
			symbol.transform.localPosition =  new Vector3(0.0f, 0.0f, Game.ZPosAdd * 0.25f );
		}
		return cellPiece;
	}
	
	/*void GenerateMeshFromCell()
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
			Color color = ;

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
	}*/

	public override void ActivatePlayer(Player player, bool hideOthersSymbols)
	{
		for(int p = 0; p < Patches.Count; ++p)
		{
			if(Patches[p].Owner == player)
			{
				CirclePatch patch = Patches[p].Patch;
				if(!patch.HasStoppedGrowing())
				{
#if HIDE_SYMBOLS
					patch.SetShowSymbol(true);
#endif
					patch.NextSegment();
				}
				else if(hideOthersSymbols)
				{
					// Only show symbol on growing patches. 
#if HIDE_SYMBOLS
					patch.SetShowSymbol(false);
#endif
				}
			}
			else if(hideOthersSymbols)
			{
#if HIDE_SYMBOLS
				Patches[p].Patch.SetShowSymbol(false);
#endif
			}
			else
			{
#if HIDE_SYMBOLS
				Patches[p].Patch.SetShowSymbol(true);
#endif
			}
		}
	}

	public bool IsFull()
	{
		for(int i = 0; i < PlayfieldCells.Count; ++i)
		{
			CellPiece cell = PlayfieldCells[i];
			if(cell.Piece == null)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsFullAndDone()
	{
		for(int i = 0; i < PlayfieldCells.Count; ++i)
		{
			CellPiece cell = PlayfieldCells[i];
			if(!cell.IsDone)
			{
				return false;
			}
		}
		return true;
	}
	
	public override bool IsDone()
	{
		for(int p = 0; p < Patches.Count; ++p)
		{
			CirclePatch patch = Patches[p].Patch;
			if(!patch.HasSegmentStoppedGrowing())
			{
				return false;
			}
		}
		return true;
	}
}
