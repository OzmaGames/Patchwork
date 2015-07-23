//#define HIDE_SYMBOLS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MultiplayerPlayfield : Playfield
{
	const float CELL_SIZE = 2.0f;
	
	class QuadTree
	{
		bool Dirty = false;
		Bounds Boundaries;
		int MaxDepth = 1;
		List<GamePieceBase> GamePieces;
		
		QuadTree TopLeft;
		QuadTree TopRight;
		QuadTree BottomLeft;
		QuadTree BottomRight;
		
		public QuadTree(float x, float y, float halfWidth, float halfHeight, int maxDepth)
		{
			Boundaries = new Bounds(new Vector3(x, y, 0.0f), new Vector3(halfWidth, halfHeight, 1.0f));
			MaxDepth = maxDepth;
		}
		
		public bool Add(Vector2 position, float size, GamePieceBase gamePiece)
		{
			//gamePiece.
			if(!Boundaries.Contains(position))
			{
				return false;
			}
			
			/*			Bounds bounds = gamePiece.GetBounds();
			if(Boundaries.Encapsulate(bounds))
			{
				Bounds topLeft = new Bounds(bounds.center, bounds.size / 2.0f);
				Bounds topRight = new Bounds(bounds.center, bounds.size / 2.0f);
				Bounds bottomLeft = new Bounds(bounds.center, bounds.size / 2.0f);
				Bounds bottomRight = new Bounds(bounds.center, bounds.size / 2.0f);
			}*/
			return true;
		}
		
		public void Split()
		{
		}
		
		public void Merge()
		{
		}
	}
	
	struct Cell
	{
		public bool Visible;// = false;
	}
	Cell[,] playfieldCells;

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
		playfieldCells = null; 
	}

	void Update()
	{
		CheckForCollision();
		
		////////////////////////////////////////////////////////////////
		int cols = playfieldCells.GetLength(1);
		int rows = playfieldCells.GetLength(0);
		for(int p = 0; p < Patches.Count; ++p)
		{
			CirclePatch patch = Patches[p].Patch;
			// Find cells.
			Vector3 pos = patch.transform.position;
			int col = (int)((pos.x + HalfWidth) / (1.0f / CELL_SIZE));
			int row = (int)((pos.y + HalfHeight) / (1.0f / CELL_SIZE));
			for(int y = row - ((int)(patch.GetSize() * CELL_SIZE)); y <= (row + ((int)(patch.GetSize() * CELL_SIZE))); ++y)
			{
				if((y < 0) || (y >= rows))
				{
					continue;
				}
				for(int x = col - ((int)(patch.GetSize() * CELL_SIZE)); x <= (col + ((int)(patch.GetSize() * CELL_SIZE))); ++x)
				{
					if((x < 0) || (x >= cols))
					{
						continue;
					}
					playfieldCells[y, x].Visible = true;
				}
			}
		}
		
		Dirty = true;
		////////////////////////////////////////////////////////////////
		
		if(Dirty)
		{
			// Regenerate mesh.
			//GenerateMesh();
			Dirty = false;
		}
	}

	public void Generate(float halfWidth, float halfHeight, float uvScale, Texture2D bgTexture)
	{
		// Generate playfield cells.
		BGTexture = bgTexture;	
		//GeneratedMesh = new Mesh();
		HalfHeight = halfHeight;
		HalfWidth = halfWidth;
		int width = (int)(HalfHeight) * (2 * (int)(CELL_SIZE));
		int height = (int)(HalfHeight) * (2 * (int)(CELL_SIZE));
		playfieldCells = new Cell[height, width];
		
		// Initial generation of mesh.
		/*GeneratedMesh.MarkDynamic();
		GenerateMesh();

		// Setup mesh.
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = GeneratedMesh;
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material.shader = Shader.Find("Custom/Playfield");
		meshRenderer.material.mainTexture = BGTexture;*/
	}
	
	void GenerateMesh()
	{
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Color> colors = new List<Color>();
		List<int> indices = new List<int>();
		
		// Generate cells.
		int index = 0;
		int width = (int)(HalfHeight) * (2 * (int)(CELL_SIZE));
		int height = (int)(HalfHeight) * (2 * (int)(CELL_SIZE));
		float adv = 1.0f / CELL_SIZE;
		float yPos = -HalfHeight;
		for(int row = 0; row < height; ++row)
		{
			float xPos = -HalfWidth;
			for(int col = 0; col < width; ++col)
			{
				bool visible = playfieldCells[row, col].Visible;
				
				vertices.Add(new Vector3(xPos, yPos));
				vertices.Add(new Vector3(xPos, yPos + adv));
				vertices.Add(new Vector3(xPos + adv, yPos));
				uvs.Add(new Vector3(0.0f, 0.0f));
				uvs.Add(new Vector3(0.0f, 0.0f));
				uvs.Add(new Vector3(0.0f, 0.0f));
				colors.Add(new Color(1.0f, 1.0f, 1.0f, visible ? 1.0f : 0.0f));
				colors.Add(new Color(1.0f, 1.0f, 1.0f, visible ? 1.0f : 0.0f));
				colors.Add(new Color(1.0f, 1.0f, 1.0f, visible ? 1.0f : 0.0f));
				indices.Add(index++);
				indices.Add(index++);
				indices.Add(index++);
				
				vertices.Add(new Vector3(xPos, yPos + adv));
				vertices.Add(new Vector3(xPos + adv, yPos + adv));
				vertices.Add(new Vector3(xPos + adv, yPos));
				uvs.Add(new Vector3(0.0f, 1.0f));
				uvs.Add(new Vector3(0.0f, 1.0f));
				uvs.Add(new Vector3(0.0f, 1.0f));
				colors.Add(new Color(1.0f, 1.0f, 1.0f, visible ? 1.0f : 0.0f));
				colors.Add(new Color(1.0f, 1.0f, 1.0f, visible ? 1.0f : 0.0f));
				colors.Add(new Color(1.0f, 1.0f, 1.0f, visible ? 1.0f : 0.0f));
				indices.Add(index++);
				indices.Add(index++);
				indices.Add(index++);
				
				xPos += adv;
			}
			yPos += adv;
		}
		
		GeneratedMesh.vertices = vertices.ToArray();
		GeneratedMesh.uv = uvs.ToArray();
		GeneratedMesh.triangles = indices.ToArray();
		GeneratedMesh.colors = colors.ToArray();
		GeneratedMesh.RecalculateNormals();
		GeneratedMesh.RecalculateBounds();
	}
	
	public override bool CanPlaceAt(Player player, GamePieceBase piece, Vector3 pos, out List<GamePieceBase> collidedPieces)
	{
		collidedPieces = new List<GamePieceBase>();
		
		if(!IsInsideGrid(pos))
		{
			return false;
		}
		
		if(piece.GetComponent<CirclePatch>() != null)
		{
			CirclePatch patch = piece.GetComponent<CirclePatch>();
			for(int p = 0; p < Patches.Count; ++p)
			{
				CirclePatch patch2 = Patches[p].Patch;
				if(patch == patch2)
				{
					continue;
				}
				if(patch.CollidesAgainst(patch2))
				{
					collidedPieces.Add(patch2);
				}
			}
			
			return collidedPieces.Count == 0;
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
		
		return false;
	}

	public override void Place(Player player, GamePieceBase piece)
	{
		if(piece.GetComponent<CirclePatch>() != null)
		{
			CirclePatch patch = piece.GetComponent<CirclePatch>();
			Patches.Add(new PlacedPatch(player, patch));
		}
		piece.Place();
	}

	public override bool IsInsideGrid(Vector3 pos)
	{
		if((Mathf.Abs(pos.x) >= HalfWidth) || (Mathf.Abs(pos.y) >= HalfHeight))
		{
			return false;
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
					if((bestCollider == null) || (bestCollider.transform.position.z > patch.transform.position.z))
					{
						bestCollider = patch;
						hasCollided = true;
					}
				}
			}
			if(bestCollider)
			{
				collidedPieces.Add(bestCollider);
			}
		}
	}

	void CheckForCollision()
	{
		// Check for collision.
		for(int p1 = 0; p1 < Patches.Count; ++p1)
		{
			CirclePatch patch1 = Patches[p1].Patch;
			Player owner1 = Patches[p1].Owner;
			if(patch1.HasStoppedGrowing())
			{
				continue;
			}
			
			for(int p2 = 0; p2 < Patches.Count; ++p2)
			{
				CirclePatch patch2 = Patches[p2].Patch;
				Player owner2 = Patches[p2].Owner;
				if((patch1 == patch2) || (owner1 == owner2) || patch2.HasStoppedGrowing())
				{
					continue;
				}
				
				if(patch1.CollidesAgainst(patch2))
				{
					// Take over
					switch(patch1.GetSymbol().Compare(patch2.GetSymbol()))
					{
					case Symbol.CompareResult.Win:
						Patches[p2].Owner = owner1;
						patch2.SetOwner(owner1);
						break;
					case Symbol.CompareResult.Lose:
						Patches[p1].Owner = owner2;
						patch1.SetOwner(owner2);
						break;
					case Symbol.CompareResult.Draw:
						break;
					}
					// DISABLED DON'T TAKE OVER USING SIZE.
					/*if(patch1.GetSize() > patch2.GetSize())
					{
						patches[p2].Owner = owner1;
						patch2.SwapColors(owner1.Colors, owner1.ComplementColor);
					}
					else
					{
						patches[p1].Owner = owner2;
						patch1.SwapColors(owner2.Colors, owner2.ComplementColor);
					}*/
					// DISABLED STOP GROW ON COLLIDE.
					//patch1.SetCollided(true);
					//patch2.SetCollided(true);
				}
			}
		}
	}
	
	public override bool CanPlaceDecoration()
	{
		// Check for collision.
		for(int p = 0; p < Patches.Count; ++p)
		{
			CirclePatch patch = Patches[p].Patch;
			if(patch.GetDecoration() == null)
			{
				return true;
			}
		}
		
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
