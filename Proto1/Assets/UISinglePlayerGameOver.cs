using UnityEngine;
using System.Collections;

public class UISinglePlayerGameOver : UIPage
{
	public Player Player;


	public override bool IsValid()
	{
		return true;
	}
	
	public override void Show()
	{
		UnityEngine.UI.Text lblScore = transform.FindChild("Star").FindChild("Score").GetComponent<UnityEngine.UI.Text>();
		lblScore.text = Player.Score.ToString();
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
	
	public void PlayNextLevel()
	{
		option = 2;
		Window.OnSubmit(this);
	}

	public void OpenLevelSelect()
	{
		option = 3;
		Window.OnSubmit(this);
	}
}
