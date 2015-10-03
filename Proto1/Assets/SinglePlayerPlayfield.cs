using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*using System.IO;
using System.Runtime.Serialization.Formatters.Binary;*/

public class SinglePlayerPlayfield : Playfield
{
	List<CellPiece> PlayfieldCells;

	float HalfWidth = 0.0f;
	float HalfHeight = 0.0f;
	
	Texture2D BGTexture;
	Texture2D[] CellTextures;
	Game.PlayerPalette[] Palettes;

	List<GameObject> Seams;
	Texture2D[] StitchTextures;

	GameObject BGCoverL;
	GameObject BGCoverR;

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
		for(int i = 0; i <Seams.Count; ++i)
		{
			Destroy(Seams[i]);
		}
		Seams.Clear();
		Seams = null;

		Destroy(BGCoverL);
		BGCoverL = null;
		Destroy(BGCoverR);
		BGCoverR = null;

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
	Color highlightColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);
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
						cell.IsDone = true;
						cell.GiveScore = false;
						piece.SetCollided(true);
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
							cell.SwapColors(circlePatch.Palette);
							break;
						case Symbol.CompareResult.Draw:
							Debug.Log("NOT MY TYPE - BUT WE LOOK THE SAME!");
							cell.GiveScore = false;
							break;
						case Symbol.CompareResult.Lose:
							Debug.Log("NOT MY TYPE - WHY DID YOU PLACE ME HERE?!");
							circlePatch.SwapColors(cell.Palette);
							cell.GiveScore = false;
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
		piece.ActivePlayfield = this;
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

	public class PlayfieldData
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

	public void Generate(float halfWidth, float halfHeight, float uvScale, Texture2D bgTexture, PlayfieldData level, Texture2D[] cellTextures, Game.PlayerPalette[] palettes, Texture2D[] stichTextures)
	{
		HalfWidth = halfWidth;
		HalfHeight = halfHeight;
		BGTexture = bgTexture;
		CellTextures = cellTextures;
		Palettes = palettes;
		StitchTextures = stichTextures;

		MeshFilter meshFilter;
		MeshRenderer meshRenderer;
		
		BGCoverL = new GameObject("BGColverL");
		meshFilter = BGCoverL.AddComponent<MeshFilter>();
		meshFilter.mesh = Helpers.GenerateQuad(14.0f, 14.0f, 15.0f);
		meshRenderer = BGCoverL.AddComponent<MeshRenderer>();
		meshRenderer.material = new Material(Shader.Find("Unlit/Texture"));
		meshRenderer.material.mainTexture = BGTexture;
		meshRenderer.material.color = Color.cyan;
		BGCoverL.transform.position = new Vector3(-16.0f, 0.0f, -9.0f);

		BGCoverR = new GameObject("BGColverR");
		meshFilter = BGCoverR.AddComponent<MeshFilter>();
		meshFilter.mesh = Helpers.GenerateQuad(14.0f, 14.0f, 15.0f);
		meshRenderer = BGCoverR.AddComponent<MeshRenderer>();
		meshRenderer.material = new Material(Shader.Find("Unlit/Texture"));
		meshRenderer.material.mainTexture = BGTexture;
		meshRenderer.material.color = Color.cyan;
		BGCoverR.transform.position = new Vector3(16.0f, 0.0f, -9.0f);

		// Generate test playfield cells.
		PlayfieldCells = new List<CellPiece>();
		for(int i = 0; i < level.Cells.Length; ++i)
		{
			PlayfieldData.Cell cell = level.Cells[i];
			PlayfieldCells.Add(GenerateCell(cell.Pos, cell.Size, cell.NoSymbol ? null : Symbol.Instantiate(cell.Symbol, Symbol.SymbolColor.White).GetComponent<Symbol>(), cell.BelongToPlayer));
		}

		Seams = new List<GameObject>();
		GameObject seam;
		for(int i = 0; i < level.Cells.Length; ++i)
		{
			float size = level.Cells[i].Size * CirclePatch.SegmentScale * 2.0f;
			Vector3 pos = level.Cells[i].Pos;
			float offset = (size * CirclePatch.SegmentScale) * 0.5f;

			seam = new GameObject("SeamH" + i);
			meshFilter = seam.AddComponent<MeshFilter>();
			meshFilter.mesh = Helpers.GenerateQuad(size, 0.15f, (size * 8.0f), 1.0f);
			meshRenderer = seam.AddComponent<MeshRenderer>();
			meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
			meshRenderer.material.mainTexture = StitchTextures[0];
			seam.transform.position = new Vector3(pos.x + offset, pos.y + offset + offset, Game.ZPosAdd * 0.26f);
			Seams.Add(seam);

			seam = new GameObject("SeamV" + i);
			meshFilter = seam.AddComponent<MeshFilter>();
			meshFilter.mesh = Helpers.GenerateQuad(0.15f, size, 1.0f, (size * 8.0f));
			meshRenderer = seam.AddComponent<MeshRenderer>();
			meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
			meshRenderer.material.mainTexture = StitchTextures[1];
			seam.transform.position = new Vector3(pos.x + offset + offset, pos.y + offset, Game.ZPosAdd * 0.26f);
			Seams.Add(seam);
		}
	}

	CellPiece GenerateCell(Vector2 pos, float size, Symbol symbol, bool belongsToPlayer)
	{
		float offset = (size * CirclePatch.SegmentScale);
		
		GameObject cellPieceObj = new GameObject("CellPiece_" + PlayfieldCells.Count);
		CellPiece cellPiece = cellPieceObj.AddComponent<CellPiece>();
		cellPiece.Generate(size, symbol, belongsToPlayer, belongsToPlayer ? Palettes[0] : Palettes[1], CellTextures[Random.Range(0, CellTextures.Length)]);
		cellPieceObj.transform.position = new Vector3(pos.x + offset, pos.y + offset, Game.ZPosAdd * 0.25f);
		if(symbol != null)
		{
			symbol.transform.SetParent(cellPieceObj.transform, false);
			symbol.transform.localPosition =  new Vector3(0.0f, 0.0f, Game.ZPosAdd * 0.25f );
		}
		return cellPiece;
	}

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
	
	public override void PieceDone(GamePieceBase piece)
	{
		System.Type type = piece.GetType();
		if(type == typeof(CirclePatch))
		{
			CirclePatch patch = piece as CirclePatch;
			DecorationCircleStopper decoration = patch.GetDecoration();
			bool alreadyDoneGrowing = patch.HasStoppedGrowing();

			// Find cell for patch.
			for(int i = 0; i < PlayfieldCells.Count; ++i)
			{
				CellPiece cell = PlayfieldCells[i];
				if((!cell.IsDone) && (cell.Piece != null))
				{
					if(cell.GiveScore)
					{
						if(cell.Piece.gameObject == patch.gameObject)
						{
							int scoreToGive = 0;
							if(decoration != null)
							{
								scoreToGive = alreadyDoneGrowing ? 2 : (int)patch.GetSize();
								if(decoration.GetOwner() != patch.GetOwner())
								{
									scoreToGive = scoreToGive / 2;
									decoration.GetOwner().AddScore(scoreToGive);
								}
								else if(alreadyDoneGrowing)
								{
									decoration.Owner.AddScore(scoreToGive);
								}
							}
							if(!alreadyDoneGrowing)
							{
								patch.Owner.AddScore((int)patch.GetSize());
								scoreToGive += (int)patch.GetSize();
							}
							patch.ShowScoreMessage(scoreToGive, scoreToGive > 0 ? Color.white : Color.red);
							cell.GiveScore = false;
							cell.IsDone = true;
						}
					}
					else
					{
						patch.ShowScoreMessage(0, Color.red);
					}
				}
			}
		}
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
