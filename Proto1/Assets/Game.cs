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

	public GameObject[] SymbolPrefabs;
	public GameObject[] PatchSizeNumberPrefabs;

	public GameObject DeckObjectPrefab;

	public Camera PlayerDeckPatch1RendererCamera;
	public Camera PlayerDeckPatch2RendererCamera;
	public Camera PlayerDeckDecorationRendererCamera;
	public Camera PatchRendererCamera;

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

	public GameObject MenuBGPrefab;
	public GameObject LogoPrefab;
	class IntroGameState : GameState
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
				ActiveGame.SetState(new MainMenuGameState());
				break;
			}
		}
	}

	public GameObject WindowPrefab;
	public GameObject MainMenuPrefab;
	public class MainMenuGameState : GameState
	{
		UIMenuBG menubg;
		UIWindow uiWindow;
		UIMainMenu uiMainMenu;

		public override void Start()
		{
			ActiveGame.QuitPrefab.SetActive(false);

			menubg = ActiveGame.MenuBGPrefab.GetComponent<UIMenuBG>();
			menubg.gameObject.SetActive(true);
			menubg.Show();

			uiMainMenu = ActiveGame.MainMenuPrefab.GetComponent<UIMainMenu>();
			uiWindow = ActiveGame.WindowPrefab.GetComponent<UIWindow>();
			uiWindow.gameObject.SetActive(true);
			uiWindow.OnSubmit = OnSubmit;
			uiWindow.Show(uiMainMenu);
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
				if(uiWindow.IsDone && (newGameState != null))
				{
					// Start the game.
					uiWindow.gameObject.SetActive(false);
					ActiveGame.MenuBGPrefab.SetActive(false);
					ActiveGame.SetState(newGameState);
				}
				break;
			}
		}

		GameState newGameState;
		void OnSubmit(UIPage page)
		{
			System.Type type = page.GetType();
			if(type == typeof(UIMainMenu))
			{
				UIMainMenu mainMenu = page as UIMainMenu;
				switch(mainMenu.option)
				{
				case 1:
					ActiveGame.PlayerSettings[0].Name = "Player";
					ActiveGame.PlayerSettings[0].Palette = ActiveGame.Palette[0];
					newGameState = new SingleplayerMainGameState();
					uiWindow.Hide();
					break;
				case 2:
					newGameState = new MultiplayerMainGameState();
					uiWindow.Hide();
					break;
				case 3:
					break;
				case 4:
					break;
				}
			}
			else if(type == typeof(UIPlayerConfig))
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
					newGameState = new MultiplayerMainGameState();
				}
				ActiveGame.PlayerSettings[player].Name = playerConfig.PlayerConfig.Name;
				ActiveGame.PlayerSettings[player].Palette = ActiveGame.Palette[playerConfig.PlayerConfig.Palette];
			}
		}
	}

	public bool abortGameSession = false;
	public GameObject QuitPrefab;
	public GameObject PlayerStatsPrefab;
	public GameObject TurnPrefab;
	public GameObject HelpPrefab;
	public GameObject ConfirmPlacementPrefab;

	public GameObject GameOverPrefab;
	public class GameOverState : GameState
	{
		List<PlayerInfo> Players;
		Playfield ActivePlayfield;
		UIWindow uiWindow;
		UIGameOver uiGameOver;

		public GameOverState(List<PlayerInfo> players, Playfield playfield)
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

			uiGameOver = ActiveGame.GameOverPrefab.GetComponent<UIGameOver>();
			uiWindow = ActiveGame.WindowPrefab.GetComponent<UIWindow>();
			uiWindow.gameObject.SetActive(true);
			uiWindow.OnSubmit = OnSubmit;
			uiGameOver.Winner = winner;
			uiGameOver.Loser = loser;
			uiWindow.Show(uiGameOver);
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
					ActiveGame.SetState(new MainMenuGameState());
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

	GameState CurrentState;
	public void SetState(GameState state)
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
		Symbol.s_Symbols = SymbolPrefabs;

		// Create circle patch mesh.
		CirclePatch.GenerateSegments(8, 1.0f, SymbolPrefabs, PatchSizeNumberPrefabs, PatchRendererCamera);

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

	public void AbortGameSession()
	{
		abortGameSession = true;

	}

	public void Quit()
	{
		Application.Quit();
	}
}
