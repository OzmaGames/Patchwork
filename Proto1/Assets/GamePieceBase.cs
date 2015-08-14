using UnityEngine;
using System.Collections;

public abstract class GamePieceBase : MonoBehaviour
{
	public const float FLASH_SPEED = 4.0f;

	public Playfield ActivePlayfield;

	public abstract void AddToDeck();
	public abstract void RemoveFromDeck();

//	public abstract void CanPlaceAt();
	public abstract void Place();
	public abstract void SetPosition(float x, float y);
//	public abstract void GetBounds(out float x, out float y, out float size);
	public abstract Bounds GetBounds();

	public abstract void StartFlash(Color startColor, Color endColor, float time);
	public abstract void SetHighlight(bool enable, Color color);

	public abstract void UpdateEffect(Color addColor);

	public abstract Player GetOwner();
	public abstract void SetOwner(Player player);

	protected void PlaceChilds()
	{
		for(int i = 0; i < transform.childCount; ++i)
		{
			GamePieceBase piece = transform.GetChild(i).GetComponent<GamePieceBase>();
			if(piece != null)
			{
				piece.Place();
			}
		}
	}

	public void UpdateChildsEffect(Color addColor)
	{
		for(int i = 0; i < transform.childCount; ++i)
		{
			GamePieceBase piece = transform.GetChild(i).GetComponent<GamePieceBase>();
			if(piece != null)
			{
				piece.UpdateEffect(addColor);
			}
		}
	}
}
