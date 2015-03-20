using UnityEngine;
using System.Collections;

public class UIGameOver : UIPage
{
	public class InitDataGameOver : UIPage.InitData
	{
		public InitDataGameOver(Player winner, Player loser)
		{
			Winner = winner;
			Loser = loser;
		}

		public Player Winner;
		public Player Loser;
	}

	void Start()
	{
	}
	
	void Update()
	{
	}
	
	public override bool IsValid()
	{
		return true;
	}
	
	public override void Submit()
	{
	}
	
	public override void Show(InitData data)
	{
		if(data.GetType() == typeof(InitDataGameOver))
		{
			UnityEngine.UI.Text lblWinner = transform.FindChild("Winner").GetComponent<UnityEngine.UI.Text>();
			UnityEngine.UI.Text lblScore = transform.FindChild("Star").FindChild("Score").GetComponent<UnityEngine.UI.Text>();
			UnityEngine.UI.Text lblLoser = transform.FindChild("Loser").GetComponent<UnityEngine.UI.Text>();

			InitDataGameOver gameOverData = data as InitDataGameOver;
			lblWinner.text = gameOverData.Winner.gameObject.name.ToUpper() + " IS THE WINNER!";
			lblScore.text = gameOverData.Winner.Score.ToString();
			lblLoser.text = "over " + gameOverData.Loser.gameObject.name + "'s " + gameOverData.Loser.Score.ToString();
		}
	}
	
	public override void Hide()
	{
	}
}
