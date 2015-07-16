using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SingleplayerMainGameState : GameState
{
	int CurrentRound = 0;
	Game.PlayerInfo ActivePlayer;
	Playfield ActivePlayfield;

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

	}

	public override void Stop()
	{
		GameObject.Destroy(ActivePlayfield.gameObject);
		ActivePlayfield = null;
		GameObject.Destroy(ActivePlayer.Player.gameObject);
		ActivePlayer = null;
		ActiveGame.QuitPrefab.SetActive(false);
	}

	bool first = true;
	public override void Update()
	{
		if((CurrentRound >= ActiveGame.NumRounds) || (ActiveGame.abortGameSession))
		{
			ActivePlayer.Player.TurnOver();

			// GAME OVER!!!!
			ActivePlayfield.HideSymbols();
//			ActiveGame.SetState(new Game.GameOverState(null, ActivePlayfield));
			ActiveGame.SetState(new Game.MainMenuGameState());
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
				++CurrentRound;
				if(CurrentRound < ActiveGame.NumRounds)
				{
					ActivatePlayer(ActivePlayer);
				}
			}
		}
	}

	void ActivatePlayer(Game.PlayerInfo playerInfo)
	{
		playerInfo.Player.ActivateTurn();
		
		// Let the circles advance one more segment.
		ActivePlayfield.ActivatePlayer(playerInfo.Player, true);
	}
	
	Game.PlayerInfo CreatePlayer(Game.PlayerSetting playerSetting)
	{
		GameObject playerObject = new GameObject(playerSetting.Name);
		Player player = playerObject.AddComponent<Player>();
		
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
