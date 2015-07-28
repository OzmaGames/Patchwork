using UnityEngine;
using System.Collections;

public class UIConfirmPlacement : MonoBehaviour
{
	public Player ActivePlayer;

	public void Confirm()
	{
		ActivePlayer.ConfirmPlacement();
	}

	public void Decline()
	{
		ActivePlayer.DeclinePlacement();
	}
}
