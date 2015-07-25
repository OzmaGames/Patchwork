using UnityEngine;
using System.Collections;

public class CellPiece : MonoBehaviour
{
	public bool IsDone = false;
	Rect Rect;
	public float Size = 0.0f;
	public Symbol Symbol;
	public GamePieceBase Piece;
	public bool BelongsToPlayer = false;
	Texture2D BGTexture;

	[System.NonSerialized]
	public Game.PlayerPalette Palette;
	
	public void Generate(float size, Symbol symbol, bool belongsToPlayer, Game.PlayerPalette palette, Texture2D bgTexture)
	{
		float rectHalfSize = size * CirclePatch.SegmentScale;
		float rectSize = rectHalfSize * 2.0f;

		IsDone = false;
		Rect = new Rect(-rectHalfSize, -rectHalfSize, rectSize, rectSize);
		Size = size;
		Symbol = symbol;
		BelongsToPlayer = belongsToPlayer;
		BGTexture = bgTexture;
		Palette = palette;
		if(symbol != null)
		{
			symbol.transform.SetParent(gameObject.transform, false);
			symbol.transform.localPosition = new Vector3(Rect.center.x, Rect.center.y, Game.ZPosAdd * 0.25f );
		}

		// Setup mesh.
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = Helpers.GenerateQuad(rectSize, rectSize, size * 0.5f);
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = new Material(Shader.Find("Custom/FabricPiece"));
		meshRenderer.material.mainTexture = BGTexture;//RandomPatternTextures[Random.Range(0,6)];
		meshRenderer.material.SetColor("_BaseColor1", Palette.Colors[Random.Range(0, Palette.Colors.Length)].colorKeys[0].color);
		meshRenderer.material.SetColor("_BaseColor2", Palette.Colors[Random.Range(0, Palette.Colors.Length)].colorKeys[1].color);
		meshRenderer.material.SetColor("_ComplementColor1", Palette.ComplementColor.colorKeys[1].color);
		meshRenderer.material.SetColor("_ComplementColor2", Palette.ComplementColor.colorKeys[1].color);
		meshRenderer.material.SetColor("_AddColor", Color.clear);
	}

	public void SwapColors(Game.PlayerPalette palette)
	{
		Palette = palette;

		MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
		meshRenderer.material.SetColor("_BaseColor1", Palette.Colors[Random.Range(0, Palette.Colors.Length)].colorKeys[0].color);
		meshRenderer.material.SetColor("_BaseColor2", Palette.Colors[Random.Range(0, Palette.Colors.Length)].colorKeys[1].color);
		meshRenderer.material.SetColor("_ComplementColor1", Palette.ComplementColor.colorKeys[1].color);
		meshRenderer.material.SetColor("_ComplementColor2", Palette.ComplementColor.colorKeys[1].color);
	}
	
	public bool Contains(Vector2 pos)
	{
		Rect rect = Rect;
		rect.center = transform.position;
		return rect.Contains(pos);
	}
}
