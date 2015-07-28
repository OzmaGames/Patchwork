using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Playfield : MonoBehaviour
{
	public abstract bool CanPlaceAt(Player player, GamePieceBase piece, Vector3 pos, out List<GamePieceBase> collidedPieces);

	public abstract void GetCollision(GamePieceBase piece, out List<GamePieceBase> collidedPieces);

	public abstract void Place(Player player, GamePieceBase piece);

	public abstract bool IsInsideGrid(Vector3 pos);

	public abstract bool CanPlaceDecoration();

	public abstract void ShowSymbols();
	
	public abstract void HideSymbols();

	// Move this to game state?
	public abstract void ActivatePlayer(Player player, bool hideOthersSymbols);

	public abstract void PieceDone(GamePieceBase piece);
	
	public abstract bool IsDone();
}
