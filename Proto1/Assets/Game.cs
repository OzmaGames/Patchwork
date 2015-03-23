using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	public const float BGLayerZ = -1.0f;
	public const float FGLayerZ = -1.0f;
	public const float UILayerZ = -1.0f;
	public const float ZPosAdd = -0.001f;

	public static float BGZPos = BGLayerZ + ZPosAdd;
	public static float FGZPos = FGLayerZ + ZPosAdd;
	public static float UIZPos = UILayerZ + ZPosAdd;

	public Color PlayerHighlightColor;
	public Color PlayerNormalColor;

	public Texture2D BGTexture;

	public Vector2 PlayAreaHalfSize = new Vector2(25.0f, 25.0f);

	public int NumRounds = 12;

	[System.Serializable]
	public class PlayerPalette
	{
		public Gradient[] Colors;
		public Gradient ComplementColor;
	}
	public PlayerPalette[] Palette;

	public Texture2D[] Symbols;
	public Texture2D[] PatchSizeNumbers;

	public GameObject DeckObjectPrefab;

	[System.Serializable]
	public class PlayerSetting
	{
		public string Name;
		public Texture2D[] PatchPatterns;
		public Texture2D[] Decorations;
		[System.NonSerialized]
		public PlayerPalette Palette;
	}
	public PlayerSetting[] PlayerSettings;

	public class PlayerInfo
	{
		public PlayerInfo(Player player)
		{
			Player = player;
		}

		public Player Player;
		public int Round = 0;
		public int Score = 0;
	}

	GameObject Background;

	abstract class State
	{
		public Game ActiveGame;

		public abstract void Start();
		public abstract void Stop();
		public abstract void Update();
	}

	public GameObject MenuBGPrefab;
	public GameObject LogoPrefab;
	class IntroGameState : State
	{
		const float WAIT_TIME = 2.0f;
		float waitTimer = 0.0f;
		PatchworkLogo logo;
		UIMenuBG menubg;

		public override void Start()
		{
			ActiveGame.QuitPrefab.SetActive(false);
			menubg = ActiveGame.MenuBGPrefab.GetComponent<UIMenuBG>();
			menubg.gameObject.SetActive(true);
			menubg.Show();
			logo = ActiveGame.LogoPrefab.GetComponent<PatchworkLogo>();
			logo.gameObject.SetActive(true);
			logo.Show();
			waitTimer = 0.0f;
		}

		public override void Stop()
		{
		}

		public override void Update()
		{
			switch(logo.Visible)
			{
			case PatchworkLogo.VisibleState.Visible:
				waitTimer += Time.deltaTime;
				if(waitTimer >= WAIT_TIME)
				{
					logo.Hide();
				}
				break;

			case PatchworkLogo.VisibleState.Hidden:
				logo.gameObject.SetActive(false);
				ActiveGame.SetState(new StartGameState());
				break;
			}
		}
	}

	public GameObject WindowPrefab;
	class StartGameState : State
	{
		UIWindow uiWindow;
		UIMenuBG menubg;

		public override void Start()
		{
			menubg = ActiveGame.MenuBGPrefab.GetComponent<UIMenuBG>();
			uiWindow = ActiveGame.WindowPrefab.GetComponent<UIWindow>();
			uiWindow.gameObject.SetActive(true);
			uiWindow.Show("PlayerConfig1", new UIPage.InitData(), OnSubmit);
		}

		void OnSubmit(UIPage page)
		{
			if(page.GetType() == typeof(UIPlayerConfig))
			{
				UIPlayerConfig playerConfig = page as UIPlayerConfig;
				int player = 0;
				if(playerConfig.gameObject.name == "PlayerConfig1")
				{
					uiWindow.transform.FindChild("PlayerConfig2").GetComponent<UIPlayerConfig>().DisablePalette(playerConfig.PlayerConfig.Palette);
					player = 0;
				}
				else
				{
					player = 1;
				}
				ActiveGame.PlayerSettings[player].Name = playerConfig.PlayerConfig.Name;
				ActiveGame.PlayerSettings[player].Palette = ActiveGame.Palette[playerConfig.PlayerConfig.Palette];
			}
		}

		public override void Stop()
		{
		}
		
		public override void Update()
		{
			switch(uiWindow.Visible)
			{
			case UIWindow.VisibleState.Visible:
				if(uiWindow.IsDone)
				{
					uiWindow.Hide();
					if((menubg.Visible == UIMenuBG.VisibleState.Showing) || (menubg.Visible == UIMenuBG.VisibleState.Visible))
					{
						menubg.Hide();
					}
				}
				break;
				
			case UIWindow.VisibleState.Hidden:
				if(uiWindow.IsDone)
				{
					// Start the game.
					uiWindow.gameObject.SetActive(false);
					ActiveGame.MenuBGPrefab.SetActive(false);
					ActiveGame.SetState(new MainGameState());
				}
				break;
			}
		}
	}

	public GameObject QuitPrefab;
	public GameObject PlayerStatsPrefab;
	public GameObject TurnPrefab;
	public GameObject HelpPrefab;
	public GameObject ConfirmPlacementPrefab;
	class MainGameState : State
	{
		int CurrentRound = 0;
		List<PlayerInfo> Players = new List<PlayerInfo>();
		List<PlayerInfo>.Enumerator ActivePlayer;
		Playfield ActivePlayfield;

		UnityEngine.UI.Text txtPlayer1Name;
		UnityEngine.UI.Text txtPlayer1Score;
		UnityEngine.UI.Text txtPlayer2Name;
		UnityEngine.UI.Text txtPlayer2Score;
		UnityEngine.UI.Text txtTurn;

		public override void Start()
		{
			ActiveGame.QuitPrefab.SetActive(true);

			// Create playfield.
			GameObject playfieldObject = new GameObject("Playfield");
			playfieldObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.5f);
			ActivePlayfield = playfieldObject.AddComponent<Playfield>();
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
			ActiveGame.TurnPrefab.SetActive(true);
			txtTurn = ActiveGame.TurnPrefab.GetComponent<UnityEngine.UI.Text>();
//			ActiveGame.HelpPrefab.SetActive(true);
			UpdateUI();
		}

		public override void Stop()
		{
//			ActiveGame.HelpPrefab.SetActive(false);
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
			
			if(CurrentRound >= ActiveGame.NumRounds)
			{
				// GAME OVER!!!!
				ActivePlayfield.HideSymbols();
				ActiveGame.SetState(new GameOverState(Players, ActivePlayfield));
				return;
			}
			else
			{
				// Handle players turn.
				PlayerInfo currentPlayerInfo = ActivePlayer.Current;
				if(currentPlayerInfo != null)
				{
					if(currentPlayerInfo.Player.IsDone)
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
			txtTurn.text = "turn " + CurrentRound + "/" + ActiveGame.NumRounds;
		}

		void AddPlayer(PlayerSetting playerSetting)
		{
			GameObject playerObject = new GameObject(playerSetting.Name);
			Player player = playerObject.AddComponent<Player>();

			player.ActivePlayfield = ActivePlayfield;
			player.Colors = playerSetting.Palette.Colors;
			player.ComplementColor = playerSetting.Palette.ComplementColor;
			player.PatternTextures = playerSetting.PatchPatterns;
			player.Decorations = playerSetting.Decorations;
			player.ConfirmPlacementPrefab = ActiveGame.ConfirmPlacementPrefab;

			Players.Add(new PlayerInfo(player));
		}

		void ActivatePlayer(PlayerInfo playerInfo)
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

	class GameOverState : State
	{
		List<PlayerInfo> Players;
		Playfield ActivePlayfield;
		UIWindow uiWindow;

		public GameOverState(List<PlayerInfo> players, Playfield playfield)
		{
			Players = players;
			ActivePlayfield = playfield;
		}

		public override void Start()
		{
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

			uiWindow = ActiveGame.WindowPrefab.GetComponent<UIWindow>();
			uiWindow.gameObject.SetActive(true);
			uiWindow.Show("GameOver", new UIGameOver.InitDataGameOver(winner, loser), OnSubmit);
		}
		
		public override void Stop()
		{
			Destroy(ActivePlayfield.gameObject);
			ActivePlayfield = null;
			for(int i = 0; i < Players.Count; ++i)
			{
				Destroy(Players[i].Player.gameObject);
			}
			Players.Clear();
			Players = null;
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
					ActiveGame.SetState(new StartGameState());
				}
				break;
			}
		}
		
		void OnSubmit(UIPage page)
		{
			if(page.GetType() == typeof(UIGameOver))
			{
				UIGameOver gameOver = page as UIGameOver;
			}
		}
		
	}

	State CurrentState;
	void SetState(State state)
	{
		if(CurrentState != null)
		{
			CurrentState.Stop();
			CurrentState.ActiveGame = null;
			//System.GC.Collect();
		}
		CurrentState = state;
		CurrentState.ActiveGame = this;
		CurrentState.Start();
	}

	// Use this for initialization
	void Start () {
		// Create circle patch mesh.
		CirclePatch.GenerateSegments(8, 1.0f);

		// Create background.
		Background = new GameObject("Background");
		Background.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		Background bg = Background.AddComponent<Background>();
		bg.Generate(PlayAreaHalfSize.x - 1.0f, PlayAreaHalfSize.y - 1.0f, 15.0f, BGTexture);

		// Set prefabs.
		Player.DeckObjectPrefab = DeckObjectPrefab;

		
		SetState(new IntroGameState());
	}

	// Handles MagicMouse scrolling.
	float someX = 0.0f;
	float someY = 0.0f;
	void OnGUI()
	{
		Event ev = Event.current;
		if (ev.type == EventType.ScrollWheel)
		{
			someX = ev.delta.x;
			someY = ev.delta.y;
		}
		else
		{
			someX = 0.0f;
			someY = 0.0f;
		}
	}
	
	void Update()
	{
		CurrentState.Update();
	}

	public void Quit()
	{
		Application.Quit();
	}
}
