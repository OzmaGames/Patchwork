using UnityEngine;
using System.Collections;

public class CellPiece : MonoBehaviour
{
	public bool IsDone = false;
	public Rect Rect;
	public float Size = 0.0f;
	public Symbol Symbol;
	public GamePieceBase Piece;
	public bool BelongsToPlayer = false;
	Texture2D BGTexture;
	Color Color;

	void Start()
	{
	
	}
	

	void Update () {
	
	}

	public void Generate(float size, Symbol symbol, bool belongsToPlayer, Color color , Texture2D bgTexture)
	{
		float rectSize = size * (CirclePatch.SegmentScale * 2.0f);
		
		IsDone = false;
		Rect = new Rect(0.0f, 0.0f, rectSize, rectSize);
		Size = size;
		Symbol = symbol;
		BelongsToPlayer = belongsToPlayer;
		BGTexture = bgTexture;
		Color = color;
		if(symbol != null)
		{
			symbol.transform.SetParent(gameObject.transform, false);
			symbol.transform.localPosition = new Vector3(Rect.center.x, Rect.center.y, Game.ZPosAdd * 0.25f );
		}

		// Setup mesh.
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = Helpers.GenerateQuad(rectSize, rectSize, 1.0f);
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = new Material(Shader.Find("Custom/FabricPiece"));
		meshRenderer.material.mainTexture = BGTexture;//RandomPatternTextures[Random.Range(0,6)];
		meshRenderer.material.color = Color;
		meshRenderer.material.SetColor("_AddColor", Color.clear);
	}
}
