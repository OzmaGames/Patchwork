using UnityEngine;
using System.Collections;

public class UIMultiplayerGameOver : UIPage
{
	public Player Winner;
	public Player Loser;

	public override bool IsValid()
	{
		return true;
	}
	
	public override void Show()
	{
		UnityEngine.UI.Text lblWinner = transform.FindChild("Winner").GetComponent<UnityEngine.UI.Text>();
		UnityEngine.UI.Text lblScore = transform.FindChild("Star").FindChild("Score").GetComponent<UnityEngine.UI.Text>();
		UnityEngine.UI.Text lblLoser = transform.FindChild("Loser").GetComponent<UnityEngine.UI.Text>();

		lblWinner.text = Winner.gameObject.name.ToUpper() + " IS THE WINNER!";
		lblScore.text = Winner.Score.ToString();
		lblLoser.text = "over " + Loser.gameObject.name + "'s " + Loser.Score.ToString();
	}
	
	public override void Hide()
	{
	}

	public int option = 0;
	public void OpenMainMenu()
	{
		option = 1;
		Window.OnSubmit(this);
	}
	
	public void PlayAgain()
	{
		option = 2;
		Window.OnSubmit(this);
	}
}
