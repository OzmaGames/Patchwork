using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SingleplayerMainGameState : GameState
{
	Game.PlayerInfo ActivePlayer;
	SinglePlayerPlayfield ActivePlayfield;

	UnityEngine.UI.Text txtPlayer1Name;
	UnityEngine.UI.Text txtPlayer1Score;

	class GameOverState : GameState
	{
		Game.PlayerInfo Player;
		Playfield ActivePlayfield;
		UIWindow uiWindow;
		UISinglePlayerGameOver uiGameOver;
		
		public GameOverState(Game.PlayerInfo player, Playfield playfield)
		{
			Player = player;
			ActivePlayfield = playfield;
		}
		
		public override void Start()
		{
			ActiveGame.QuitPrefab.SetActive(false);
			
			uiGameOver = ActiveGame.SinglePlayerGameOverPrefab.GetComponent<UISinglePlayerGameOver>();
			uiWindow = ActiveGame.WindowPrefab.GetComponent<UIWindow>();
			uiWindow.gameObject.SetActive(true);
			uiWindow.OnSubmit = OnSubmit;
			uiGameOver.Player = Player.Player;
			uiWindow.Show(uiGameOver);
		}
		
		public override void Stop()
		{
			GameObject.Destroy(ActivePlayfield.gameObject);
			ActivePlayfield = null;
			GameObject.Destroy(Player.Player.gameObject);
			Player = null;
		}
		
		public override void Update()
		{
			switch(uiWindow.Visible)
			{
			case UIWindow.VisibleState.Visible:
				if(uiWindow.IsDone)
				{
					uiWindow.Hide();
				}
				break;
				
			case UIWindow.VisibleState.Hidden:
				if(uiWindow.IsDone)
				{
					// Re-start.
					ActiveGame.SetState(new Game.MainMenuGameState());
				}
				break;
			}
		}

		void OnSubmit(UIPage page)
		{
			if(page.GetType() == typeof(UISinglePlayerGameOver))
			{
				UISinglePlayerGameOver gameOver = page as UISinglePlayerGameOver;
				switch(gameOver.option)
				{
				case 1:
					// Main menu.
					ActiveGame.SetState(new Game.MainMenuGameState());
					break;

				case 2:
					// Play next level.
					Debug.Log("Next level: " + gameOver.CurrentLevel + 1);
					break;
				}
			}
		}
		
	}
	
	public SingleplayerMainGameState(int level)
	{
		Debug.Log("level: " + level);
	}

	public override void Start()
	{
		ActiveGame.abortGameSession = false;
		ActiveGame.QuitPrefab.SetActive(true);

		// Create playfield.
		GameObject playfieldObject = new GameObject("Playfield");
		playfieldObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.5f);
		ActivePlayfield = playfieldObject.AddComponent<SinglePlayerPlayfield>();
		ActivePlayfield.Generate(ActiveGame.PlayAreaHalfSize.x - 1.0f, ActiveGame.PlayAreaHalfSize.y - 1.0f, 10.0f, ActiveGame.BGTexture, ActiveGame.PlayerSettings[0].PatchPatterns, ActiveGame.Palette);
		
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
			ActiveGame.SetState(new GameOverState(ActivePlayer, ActivePlayfield));
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
		player.Palette = playerSetting.Palette;
		player.PatternTextures = playerSetting.PatchPatterns;
		player.Decorations = playerSetting.Decorations;
		player.ConfirmPlacementPrefab = ActiveGame.ConfirmPlacementPrefab;
		player.ActiveGame = ActiveGame;
		
		return new Game.PlayerInfo(player);
	}
}
