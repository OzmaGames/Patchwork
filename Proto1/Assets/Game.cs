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
	public Texture2D[] CellSymbolIcons;

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
					menubg.gameObject.SetActive(false);
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
				Debug.Log("HOW DID I GET HERE? " + mainMenu.option);
			}
			else if(type == typeof(UILevelSelect))
			{
				UILevelSelect levelSelect = page as UILevelSelect;
				switch(levelSelect.option)
				{
				case 1:
					ActiveGame.PlayerSettings[0].Name = "Score";
					ActiveGame.PlayerSettings[0].Palette = ActiveGame.Palette[0];
					newGameState = new SingleplayerMainGameState(levelSelect.SelectedLevel);
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

	public GameObject SinglePlayerGameOverPrefab;
	public GameObject MultiplayerGameOverPrefab;

	GameState CurrentState;
	GameState NextState;
	public void SetState(GameState state)
	{
		NextState = state;
	}

	void UpdateState()
	{
		if(NextState != null)
		{
			// Stop old state.
			if(CurrentState != null)
			{
				CurrentState.Stop();
				CurrentState.ActiveGame = null;
				//System.GC.Collect();
			}

			// Switch to new state.
			CurrentState = NextState;
			NextState = null;
			CurrentState.ActiveGame = this;
			CurrentState.Start();
		}
		CurrentState.Update();
	}

	// Use this for initialization
	void Start () {
		Symbol.s_Symbols = SymbolPrefabs;

		// Create circle patch mesh.
		Texture2D[] patternTextures = new Texture2D[PlayerSettings[0].PatchPatterns.Length + PlayerSettings[1].PatchPatterns.Length];
		for(int i = 0; i < PlayerSettings[0].PatchPatterns.Length; ++i)
		{
			patternTextures[i] = PlayerSettings[0].PatchPatterns[i];
		}
		for(int i = PlayerSettings[0].PatchPatterns.Length; i < PlayerSettings[1].PatchPatterns.Length; ++i)
		{
			patternTextures[i] = PlayerSettings[1].PatchPatterns[i];
		}
		CirclePatch.GenerateSegments(8, 1.0f, PatchSizeNumberPrefabs, PatchRendererCamera, patternTextures);

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
		UpdateState();
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
