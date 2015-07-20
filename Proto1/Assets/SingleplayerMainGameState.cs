using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SingleplayerMainGameState : GameState
{
	Game.PlayerInfo ActivePlayer;
	SinglePlayerPlayfield ActivePlayfield;

	UnityEngine.UI.Text txtPlayer1Name;
	UnityEngine.UI.Text txtPlayer1Score;


	public override void Start()
	{
		ActiveGame.abortGameSession = false;
		ActiveGame.QuitPrefab.SetActive(true);

		// Create playfield.
		GameObject playfieldObject = new GameObject("Playfield");
		playfieldObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.5f);
		ActivePlayfield = playfieldObject.AddComponent<SinglePlayerPlayfield>();
		ActivePlayfield.Generate(ActiveGame.PlayAreaHalfSize.x - 1.0f, ActiveGame.PlayAreaHalfSize.y - 1.0f, 10.0f, ActiveGame.BGTexture);
		
		// Create player.
		ActivePlayer = CreatePlayer(ActiveGame.PlayerSettings[0]);

		// Setup UI.
		ActiveGame.PlayerStatsPrefab.SetActive(true);
		txtPlayer1Name = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Name").GetComponent<UnityEngine.UI.Text>();
		txtPlayer1Score = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Score").GetComponent<UnityEngine.UI.Text>();
		ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Name").gameObject.SetActive(true);
		ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Score").gameObject.SetActive(true);
		ActiveGame.PlayerStatsPrefab.transform.FindChild("Player2").FindChild("Name").gameObject.SetActive(false);
		ActiveGame.PlayerStatsPrefab.transform.FindChild("Player2").FindChild("Score").gameObject.SetActive(false);
		UpdateUI();
	}

	public override void Stop()
	{
		GameObject.Destroy(ActivePlayfield.gameObject);
		ActivePlayfield = null;
		GameObject.Destroy(ActivePlayer.Player.gameObject);
		ActivePlayer = null;
		ActiveGame.PlayerStatsPrefab.SetActive(false);
		ActiveGame.QuitPrefab.SetActive(false);
	}

	bool first = true;
	public override void Update()
	{
		if(ActivePlayfield.IsFullAndDone() || (ActiveGame.abortGameSession))
		{
			ActivePlayer.Player.TurnOver();

			// GAME OVER!!!!
			ActivePlayfield.HideSymbols();
//			ActiveGame.SetState(new Game.GameOverState(null, ActivePlayfield));
			//ActiveGame.SetState(new Game.MainMenuGameState());
			return;
		}
		else
		{
			if(first)
			{
				ActivatePlayer(ActivePlayer);
				first = false;
			}
			// Handle players turn.
			if(ActivePlayer.Player.IsDone())
			{
				// Signal turn as done.
				ActivePlayer.Player.TurnOver();
				++ActivePlayer.Round;
				
				// Activate next round.
				ActivatePlayer(ActivePlayer);
			}
		}

		UpdateUI();
	}

	void UpdateUI()
	{
		txtPlayer1Name.color = ActiveGame.PlayerNormalColor;
		txtPlayer1Name.text = ActivePlayer.Player.gameObject.name;
		txtPlayer1Score.text = ActivePlayer.Player.Score.ToString();
	}

	void ActivatePlayer(Game.PlayerInfo playerInfo)
	{
		if(!ActivePlayfield.IsFull())
		{
			playerInfo.Player.ActivateTurn();
		}

		// Let the circles advance one more segment.
		ActivePlayfield.ActivatePlayer(playerInfo.Player, true);
	}
	
	Game.PlayerInfo CreatePlayer(Game.PlayerSetting playerSetting)
	{
		GameObject playerObject = new GameObject(playerSetting.Name);
		Player player = playerObject.AddComponent<Player>();

		player.MinPatchSize = 1;
		player.NumPatchesInDeck = 20;
		player.NumDecorationsInDeck = 20;
		player.ActivePlayfield = ActivePlayfield;
		player.Colors = playerSetting.Palette.Colors;
		player.ComplementColor = playerSetting.Palette.ComplementColor;
		player.PatternTextures = playerSetting.PatchPatterns;
		player.Decorations = playerSetting.Decorations;
		player.ConfirmPlacementPrefab = ActiveGame.ConfirmPlacementPrefab;
		player.ActiveGame = ActiveGame;
		
		return new Game.PlayerInfo(player);
	}
}
