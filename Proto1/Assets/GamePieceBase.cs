using UnityEngine;
using System.Collections;

public abstract class GamePieceBase : MonoBehaviour
{
	public const float FLASH_SPEED = 4.0f;

	public abstract void AddToDeck();
	public abstract void RemoveFromDeck();

//	public abstract void CanPlaceAt();
	public abstract void Place();
	public abstract void SetPosition(float x, float y);
//	public abstract void GetBounds(out float x, out float y, out float size);
	public abstract Bounds GetBounds();

	public abstract void StartFlash(Color startColor, Color endColor, float time);
	public abstract void SetHighlight(bool enable, Color color);
}
