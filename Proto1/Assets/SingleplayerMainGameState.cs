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
		SinglePlayerPlayfield ActivePlayfield;
		UIWindow uiWindow;
		UISinglePlayerGameOver uiGameOver;
		GameState NextState;
		UILevelSelect uiLevelSelect;
		int NextLevel;

		public GameOverState(Game.PlayerInfo player, SinglePlayerPlayfield playfield, int nextLevel)
		{
			Player = player;
			ActivePlayfield = playfield;
			NextLevel = nextLevel;
		}
		
		public override void Start()
		{
			ActiveGame.QuitPrefab.SetActive(false);
			
			uiGameOver = ActiveGame.SinglePlayerGameOverPrefab.GetComponent<UISinglePlayerGameOver>();
			uiWindow = ActiveGame.WindowPrefab.GetComponent<UIWindow>();
			uiLevelSelect = uiWindow.gameObject.transform.FindChild("LevelSelect").GetComponent<UILevelSelect>();
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
			case UIWindow.VisibleState.Hidden:
				uiWindow.gameObject.SetActive(false);
				if(NextState != null)
				{
					ActiveGame.SetState(NextState);
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
					NextState = new Game.MainMenuGameState(ActiveGame.MainMenuPrefab.GetComponent<UIMainMenu>());
					uiWindow.Hide();
					break;

				case 2:
					// Play next level.
					Debug.Log("Next level: " + NextLevel);
					NextState = new SingleplayerMainGameState(NextLevel);
					uiWindow.Hide();
					break;

				case 3:
					// Level select.
					NextState = new Game.MainMenuGameState(uiLevelSelect);
					//uiWindow.Show(uiLevelSelect);
					uiWindow.Hide();
					break;
				}
			}
			else if(page.GetType() == typeof(UILevelSelect))
			{
				UILevelSelect levelSelect = page as UILevelSelect;
				switch(levelSelect.option)
				{
				case 1:
					ActiveGame.PlayerSettings[0].Name = "Score";
					ActiveGame.PlayerSettings[0].Palette = ActiveGame.Palette[0];
					NextState = new SingleplayerMainGameState(levelSelect.SelectedLevel);
					uiWindow.Hide();
					break;
				case 2:
					Debug.Log("ShowHelp");
					break;
				case 3:
					Debug.Log("ShowOptions");
					break;
				case 4:
					break;
				}
			}
		}
		
	}
	
	public SinglePlayerPlayfield.PlayfieldData[] Levels = new SinglePlayerPlayfield.PlayfieldData[] {
		new SinglePlayerPlayfield.PlayfieldData(new SinglePlayerPlayfield.PlayfieldData.Cell[] {
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-9.0f, -7.0f), 3.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-9.0f, -1.0f), 2.0f, Symbol.SymbolTypes.Needle, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-5.0f, -1.0f), 1.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-9.0f, 3.0f), 2.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-5.0f, 1.0f), 3.0f, Symbol.SymbolTypes.Scissor, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-3.0f, -3.0f), 2.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-3.0f, -9.0f), 3.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(1.0f, -3.0f), 1.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(1.0f, -1.0f), 4.0f, Symbol.SymbolTypes.Thread, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(3.0f, -7.0f), 3.0f, Symbol.SymbolTypes.Thread, false)
		}),
		new SinglePlayerPlayfield.PlayfieldData(new SinglePlayerPlayfield.PlayfieldData.Cell[] {
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-9.0f, -7.0f), 2.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-13.0f, -3.0f), 5.0f, Symbol.SymbolTypes.Needle, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-5.0f, -11.0f), 4.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-3.0f, -3.0f), 2.0f, Symbol.SymbolTypes.Scissor, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-3.0f, 1.0f), 3.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(1.0f, -1.0f), 1.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(1.0f, -3.0f), 1.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(3.0f, -1.0f), 4.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(3.0f, -7.0f), 3.0f, Symbol.SymbolTypes.Thread, false),
		}),
		new SinglePlayerPlayfield.PlayfieldData(new SinglePlayerPlayfield.PlayfieldData.Cell[] {
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-9.0f, -7.0f), 4.0f, Symbol.SymbolTypes.Needle, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-11.0f, 1.0f), 3.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-5.0f, 1.0f), 1.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-3.0f, 1.0f), 1.0f, Symbol.SymbolTypes.Scissor, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-5.0f, 3.0f), 4.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-1.0f, -7.0f), 3.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-1.0f, -1.0f), 2.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(2.0f, -1.0f), 4.0f, Symbol.SymbolTypes.Thread, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(5.0f, -9.0f), 2.0f, Symbol.SymbolTypes.Needle, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(5.0f, -5.0f), 2.0f, true),
		}),
		new SinglePlayerPlayfield.PlayfieldData(new SinglePlayerPlayfield.PlayfieldData.Cell[] {
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-9.0f, -11.0f), 5.0f, Symbol.SymbolTypes.Needle, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-9.0f, -1.0f), 2.0f, Symbol.SymbolTypes.Scissor, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-9.0f, 3.0f), 3.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-5.0f, -1.0f), 1.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-5.0f, 1.0f), 1.0f, Symbol.SymbolTypes.Thread, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-3.0f, -1.0f), 1.0f, Symbol.SymbolTypes.Needle, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-3.0f, 1.0f), 4.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(-1.0f, -1.0f), 1.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(1.0f, -11.0f), 4.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(1.0f, -3.0f), 2.0f, Symbol.SymbolTypes.Scissor, false),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(5.0f, -3.0f), 2.0f, true),
			new SinglePlayerPlayfield.PlayfieldData.Cell(new Vector2(5.0f, 1.0f), 3.0f, true),
		})
	};
	int ActiveLevel = 0;


	public SingleplayerMainGameState(int level)
	{
		Debug.Log("level: " + level);
		ActiveLevel = level;
	}

	public override void Start()
	{
		Game.ResetPos();

		ActiveGame.abortGameSession = false;
		ActiveGame.QuitPrefab.SetActive(true);

		// Create playfield.
		GameObject playfieldObject = new GameObject("Playfield");
		playfieldObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.5f);
		ActivePlayfield = playfieldObject.AddComponent<SinglePlayerPlayfield>();
		ActivePlayfield.Generate(ActiveGame.PlayAreaHalfSize.x - 1.0f, ActiveGame.PlayAreaHalfSize.y - 1.0f, 10.0f, ActiveGame.BGTexture, Levels[ActiveLevel], ActiveGame.PlayerSettings[0].PatchPatterns, ActiveGame.Palette);
		
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
			ActiveGame.SetState(new GameOverState(ActivePlayer, ActivePlayfield, (ActiveLevel + 1) % Levels.Length));
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
