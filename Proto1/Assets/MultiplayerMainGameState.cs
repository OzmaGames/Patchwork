using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerMainGameState : GameState
{
	int CurrentRound = 0;
	List<Game.PlayerInfo> Players = new List<Game.PlayerInfo>();
	List<Game.PlayerInfo>.Enumerator ActivePlayer;
	MultiplayerPlayfield ActivePlayfield;
	
	UnityEngine.UI.Text txtPlayer1Name;
	UnityEngine.UI.Text txtPlayer1Score;
	UnityEngine.UI.Text txtPlayer2Name;
	UnityEngine.UI.Text txtPlayer2Score;
	UnityEngine.UI.Text txtTurn;
	
	class GameOverState : GameState
	{
		List<Game.PlayerInfo> Players;
		Playfield ActivePlayfield;
		UIWindow uiWindow;
		UIMultiplayerGameOver uiGameOver;
		GameState NextGameState;

		public GameOverState(List<Game.PlayerInfo> players, Playfield playfield)
		{
			Players = players;
			ActivePlayfield = playfield;
		}
		
		public override void Start()
		{
			ActiveGame.QuitPrefab.SetActive(false);
			
			// Find winner and loser.
			Player winner;
			Player loser;
			if(Players[0].Player.Score >= Players[1].Player.Score)
			{
				winner = Players[0].Player;
				loser = Players[1].Player;
			}
			else
			{
				winner = Players[1].Player;
				loser = Players[0].Player;
			}
			
			uiGameOver = ActiveGame.MultiplayerGameOverPrefab.GetComponent<UIMultiplayerGameOver>();
			uiWindow = ActiveGame.WindowPrefab.GetComponent<UIWindow>();
			uiWindow.gameObject.SetActive(true);
			uiWindow.OnSubmit = OnSubmit;
			uiGameOver.Winner = winner;
			uiGameOver.Loser = loser;
			uiWindow.Show(uiGameOver);
		}

		public override void Stop()
		{
			GameObject.Destroy(ActivePlayfield.gameObject);
			ActivePlayfield = null;
			for(int i = 0; i < Players.Count; ++i)
			{
				GameObject.Destroy(Players[i].Player.gameObject);
				Players[i].Player = null;
				Players[i] = null;
			}
			Players.Clear();
			Players = null;
		}
		
		public override void Update()
		{
			switch(uiWindow.Visible)
			{
			case UIWindow.VisibleState.Hidden:
				uiWindow.gameObject.SetActive(false);
				if(NextGameState != null)
				{
					ActiveGame.SetState(NextGameState);
				}
				break;
			}
		}
		
		void OnSubmit(UIPage page)
		{
			if(page.GetType() == typeof(UIMultiplayerGameOver))
			{
				UIMultiplayerGameOver gameOver = page as UIMultiplayerGameOver;
				switch(gameOver.option)
				{
				case 1:
					// Main menu.
					NextGameState = new Game.MainMenuGameState(ActiveGame.MainMenuPrefab.GetComponent<UIMainMenu>());
					uiWindow.Hide();
					break;
					
				case 2:
					// Play again.
					NextGameState = new MultiplayerMainGameState();
					uiWindow.Hide();
					break;
				}
			}
		}
	}

	public override void Start()
	{
		Game.ResetPos();

		ActiveGame.abortGameSession = false;
		
		ActiveGame.QuitPrefab.SetActive(true);
		
		// Create playfield.
		GameObject playfieldObject = new GameObject("Playfield");
		playfieldObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.5f);
		ActivePlayfield = playfieldObject.AddComponent<MultiplayerPlayfield>();
		ActivePlayfield.Generate(ActiveGame.PlayAreaHalfSize.x - 1.0f, ActiveGame.PlayAreaHalfSize.y - 1.0f, 10.0f, ActiveGame.BGTexture);
		
		// Create players.
		for(int i = 0; i < ActiveGame.PlayerSettings.Length; ++i)
		{
			AddPlayer(ActiveGame.PlayerSettings[i]);
		}
		
		// Setup UI.
		ActiveGame.PlayerStatsPrefab.SetActive(true);
		txtPlayer1Name = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Name").GetComponent<UnityEngine.UI.Text>();
		txtPlayer1Score = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Score").GetComponent<UnityEngine.UI.Text>();
		txtPlayer2Name = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player2").FindChild("Name").GetComponent<UnityEngine.UI.Text>();
		txtPlayer2Score = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player2").FindChild("Score").GetComponent<UnityEngine.UI.Text>();
		ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Name").gameObject.SetActive(true);
		ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Score").gameObject.SetActive(true);
		ActiveGame.PlayerStatsPrefab.transform.FindChild("Player2").FindChild("Name").gameObject.SetActive(true);
		ActiveGame.PlayerStatsPrefab.transform.FindChild("Player2").FindChild("Score").gameObject.SetActive(true);
		ActiveGame.TurnPrefab.SetActive(true);
		txtTurn = ActiveGame.TurnPrefab.GetComponent<UnityEngine.UI.Text>();
		UpdateUI();
	}
	
	public override void Stop()
	{
		ActiveGame.PlayerStatsPrefab.SetActive(false);
		ActiveGame.TurnPrefab.SetActive(false);
		ActiveGame.QuitPrefab.SetActive(false);
	}
	
	public override void Update()
	{
		// Handle camera movement.
		/*if((ActiveGame.someX != 0.0f) || (ActiveGame.someY != 0.0f))
			{
				Vector3 pos = Camera.main.transform.position;
				Vector3 newPos = new Vector3(pos.x + ActiveGame.someX, pos.y + ActiveGame.someY, pos.z);
				if(newPos.x > ActiveGame.PlayAreaHalfSize.x)
				{
					newPos.x = ActiveGame.PlayAreaHalfSize.x;
				}
				else if(newPos.x < (-ActiveGame.PlayAreaHalfSize.x))
				{
					newPos.x = -ActiveGame.PlayAreaHalfSize.x;
				}
				if(newPos.y > ActiveGame.PlayAreaHalfSize.y)
				{
					newPos.y = ActiveGame.PlayAreaHalfSize.y;
				}
				else if(newPos.y < (-ActiveGame.PlayAreaHalfSize.y))
				{
					newPos.y = -ActiveGame.PlayAreaHalfSize.y;
				}
				Camera.main.transform.position = newPos;
			}*/
		
		if((CurrentRound >= ActiveGame.NumRounds) || (ActiveGame.abortGameSession))
		{
			Game.PlayerInfo currentPlayerInfo = ActivePlayer.Current;
			if(currentPlayerInfo != null)
			{
				currentPlayerInfo.Player.TurnOver();
			}
			
			// GAME OVER!!!!
			ActivePlayfield.HideSymbols();
			ActiveGame.SetState(new GameOverState(Players, ActivePlayfield));
			return;
		}
		else
		{
			// Handle players turn.
			Game.PlayerInfo currentPlayerInfo = ActivePlayer.Current;
			if(currentPlayerInfo != null)
			{
				if(currentPlayerInfo.Player.IsDone())
				{
					// Signal turn as done.
					currentPlayerInfo.Player.TurnOver();
					++currentPlayerInfo.Round;
					
					// Activate next player.
					if(!ActivePlayer.MoveNext())
					{
						// Last player so rest back to first.
						ActivePlayer = Players.GetEnumerator();
						ActivePlayer.MoveNext();
						
						++CurrentRound;
					}
					if(CurrentRound < ActiveGame.NumRounds)
					{
						ActivatePlayer(ActivePlayer.Current);
					}
				}
			}
			else
			{
				ActivePlayer = Players.GetEnumerator();
				ActivePlayer.MoveNext();
				ActivatePlayer(ActivePlayer.Current);
			}
		}
		
		UpdateUI();
	}
	
	void UpdateUI()
	{
		if(ActivePlayer.Current == Players[0])
		{
			txtPlayer1Name.color = ActiveGame.PlayerHighlightColor;
			txtPlayer2Name.color = ActiveGame.PlayerNormalColor;
		}
		else if(ActivePlayer.Current == Players[1])
		{
			txtPlayer1Name.color = ActiveGame.PlayerNormalColor;
			txtPlayer2Name.color = ActiveGame.PlayerHighlightColor;
		}
		else
		{
			txtPlayer1Name.color = ActiveGame.PlayerNormalColor;
			txtPlayer2Name.color = ActiveGame.PlayerNormalColor;
		}
		txtPlayer1Name.text = Players[0].Player.gameObject.name;
		txtPlayer1Score.text = Players[0].Player.Score.ToString();
		txtPlayer2Name.text = Players[1].Player.gameObject.name;
		txtPlayer2Score.text = Players[1].Player.Score.ToString();
		txtTurn.text = "turn " + (CurrentRound + 1) + "/" + ActiveGame.NumRounds;
	}
	
	void AddPlayer(Game.PlayerSetting playerSetting)
	{
		GameObject playerObject = new GameObject(playerSetting.Name);
		Player player = playerObject.AddComponent<Player>();
		
		player.MinPatchSize = 2;
		player.NumPatchesInDeck = 20;
		player.NumDecorationsInDeck = 20;
		player.ActivePlayfield = ActivePlayfield;
		player.Palette = playerSetting.Palette;
		player.PatternTextures = playerSetting.PatchPatterns;
		player.Decorations = playerSetting.Decorations;
		player.ConfirmPlacementPrefab = ActiveGame.ConfirmPlacementPrefab;
		player.ActiveGame = ActiveGame;
		
		Players.Add(new Game.PlayerInfo(player));
	}
	
	void ActivatePlayer(Game.PlayerInfo playerInfo)
	{
		playerInfo.Player.ActivateTurn();
		
		// Let the circles advance one more segment.
		ActivePlayfield.ActivatePlayer(playerInfo.Player, true);
	}
	
	void NextPlayer()
	{
		// Activate next player.
		if(!ActivePlayer.MoveNext())
		{
			// Last player so rest back to first.
			ActivePlayer = Players.GetEnumerator();
			ActivePlayer.MoveNext();
		}
		if((ActivePlayer.Current != null) && (ActivePlayer.Current.Round < 12))
		{
			ActivatePlayer(ActivePlayer.Current);
		}
	}		
}
