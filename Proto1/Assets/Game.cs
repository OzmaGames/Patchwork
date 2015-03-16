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

	public Texture2D BGTexture;

	public GUISkin GameGUISkin;

	public int NumberofPatchesPerPlayer = 8;
	public int NumberOfDecorationsPerPlayer = 2;
	public Vector2 PlayAreaHalfSize = new Vector2(25.0f, 25.0f);

	public int NumRounds = 12;

	[System.Serializable]
	public class PlayerPalette
	{
		public Gradient[] Colors;
		public Gradient ComplementColor;
	}
	public PlayerPalette[] Palette;

	[System.Serializable]
	public class PlayerSetting
	{
		public string Name;
		public Texture2D[] PatchPatterns;
		public Texture2D[] Decorations;
		public int PaletteIndex;
	}
	public PlayerSetting[] PlayerSettings;

	class PlayerInfo
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
	Playfield Playfield;

	abstract class State
	{
		public Game ActiveGame;

		public abstract void Start();
		public abstract void Stop();
		public abstract void Update();
		public abstract void OnGUI();
	}

	public GameObject MenuBGPrefab;
	public GameObject LogoPrefab;
	class IntroGameState : State
	{
		const float WAIT_TIME = 2.0f;
		float waitTimer = 0.0f;
		PatchworkLogo logo;

		public override void Start()
		{
			ActiveGame.MenuBGPrefab.SetActive(true);
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
		
		public override void OnGUI()
		{
		}
	}

	public GameObject WelcomePrefab;
	class StartGameState : State
	{
		UIWelcome uiWelcome;
		List<PlayerInfo> Players = new List<PlayerInfo>();

		public override void Start()
		{
			uiWelcome = ActiveGame.WelcomePrefab.GetComponent<UIWelcome>();
			uiWelcome.gameObject.SetActive(true);
			/*// Add players.
			for(int i = 0; i < ActiveGame.PlayerSettings.Length; ++i)
			{
				AddPlayer(ActiveGame.PlayerSettings[i]);
			}*/
		}

		void OnSubmit(string name, int palette)
		{
			ActiveGame.PlayerSettings[Players.Count].Name = name;
			ActiveGame.PlayerSettings[Players.Count].PaletteIndex = palette;
			AddPlayer(ActiveGame.PlayerSettings[Players.Count]);
			if(Players.Count == 1)
			{
				uiWelcome.WelcomePlayerTwo(OnSubmit);
			}
			if(Players.Count >= ActiveGame.PlayerSettings.Length)
			{
				uiWelcome.Hide();
			}
		}

		public override void Stop()
		{
			ActiveGame.WelcomePrefab.SetActive(false);
		}
		
		public override void Update()
		{
			switch(uiWelcome.Visible)
			{
			case UIWelcome.VisibleState.Visible:
				break;
				
			case UIWelcome.VisibleState.Hidden:
				if(Players.Count == 0)
				{
					uiWelcome.WelcomePlayerOne(OnSubmit);
				}
				else if(Players.Count == 1)
				{
					uiWelcome.WelcomePlayerTwo(OnSubmit);
				}
				else if(Players.Count >= ActiveGame.PlayerSettings.Length)
				{
					// Start the game.
					uiWelcome.gameObject.SetActive(false);
					ActiveGame.MenuBGPrefab.SetActive(false);
					ActiveGame.SetState(new MainGameState(Players));
				}
				break;
			}

		}
		
		string textFieldString = "";
		public override void OnGUI()
		{
			/*int width = 200;
			int height = 120;
			GUILayout.BeginArea(new Rect((Screen.width / 2) - (width / 2), (Screen.height / 2) - (height / 2), width, height), "", "box");
			GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
			if(Players.Count < ActiveGame.PlayerSettings.Length)
			{
				GUILayout.Label("Player " + (Players.Count + 1) + " Name");
				textFieldString = GUILayout.TextField(textFieldString);
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Next"))
				{
					ActiveGame.PlayerSettings[Players.Count].Name = textFieldString;
					AddPlayer(ActiveGame.PlayerSettings[Players.Count]);
					textFieldString = "";
					//Application.LoadLevel(1);
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();*/
		}

		void AddPlayer(PlayerSetting playerSetting)
		{
			GameObject playerObject = new GameObject(playerSetting.Name);
			Player player = playerObject.AddComponent<Player>();

			player.ActiveGame = ActiveGame;
			player.Colors = ActiveGame.Palette[playerSetting.PaletteIndex].Colors;
			player.ComplementColor = ActiveGame.Palette[playerSetting.PaletteIndex].ComplementColor;
			player.PatternTextures = playerSetting.PatchPatterns;
			player.Decorations = playerSetting.Decorations;
			
			Players.Add(new PlayerInfo(player));
		}
	}

	public GameObject PlayerStatsPrefab;
	class MainGameState : State
	{
		int CurrentRound = 0;
		List<PlayerInfo> Players = new List<PlayerInfo>();
		List<PlayerInfo>.Enumerator ActivePlayer;

		UnityEngine.UI.Text txtPlayer1Name;
		UnityEngine.UI.Text txtPlayer1Score;
		UnityEngine.UI.Text txtPlayer2Name;
		UnityEngine.UI.Text txtPlayer2Score;

		public MainGameState(List<PlayerInfo> players)
		{
			Players = players;
		}

		public override void Start()
		{
			ActiveGame.PlayerStatsPrefab.SetActive(true);
			txtPlayer1Name = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Name").GetComponent<UnityEngine.UI.Text>();
			txtPlayer1Score = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Score").GetComponent<UnityEngine.UI.Text>();
			txtPlayer2Name = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player2").FindChild("Name").GetComponent<UnityEngine.UI.Text>();
			txtPlayer2Score = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player2").FindChild("Score").GetComponent<UnityEngine.UI.Text>();
		}

		public override void Stop()
		{
		}
		
		public override void Update()
		{
			// Handle camera movement.
			if((ActiveGame.someX != 0.0f) || (ActiveGame.someY != 0.0f))
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
			}
			
			if(CurrentRound >= ActiveGame.NumRounds)
			{
				// GAME OVER!!!!
				ActiveGame.SetState(new GameOverState());
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

			// Update UI.
			txtPlayer1Name.text = Players[0].Player.gameObject.name;
			txtPlayer1Score.text = Players[0].Player.Score.ToString();
			txtPlayer2Name.text = Players[1].Player.gameObject.name;
			txtPlayer2Score.text = Players[1].Player.Score.ToString();
		}
		
		public override void OnGUI()
		{
			/*float width = 170.0f;
			float height = 50.0f;
			float x = (Screen.width * 0.5f) - (width * 0.5f);
			float y = 10.0f;

			GUI.BeginGroup(new Rect(x, y, width, height), "", "box");
			GUI.Label(new Rect(10.0f, 10.0f, width, height), "Round: " + (CurrentRound + 1) + " / " + ActiveGame.NumRounds);
			GUI.EndGroup();

			width = 150.0f;
			height = 80.0f;
			x = (Screen.width - (width + 10.0f));
			y = 10.0f;
			
			GUILayout.BeginArea(new Rect(x, y, width, height), "", "box");
			GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
			for(int i = 0; i < Players.Count; ++i)
			{
				Player player = Players[i].Player;
				GUILayout.BeginHorizontal();
				GUILayout.Label(player.gameObject.name);
				GUILayout.Label(player.Score.ToString());
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();*/
		}

		void ActivatePlayer(PlayerInfo playerInfo)
		{
			playerInfo.Player.ActivateTurn();
			
			// Let the circles advance one more segment.
			ActiveGame.Playfield.ActivatePlayer(playerInfo.Player, true);
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
		public override void Start()
		{
		}
		
		public override void Stop()
		{
		}
		
		public override void Update()
		{
		}
		
		public override void OnGUI()
		{
			float width = 150.0f;
			float height = 50.0f;
			float x = (Screen.width * 0.5f) - (width * 0.5f);
			float y = 180.0f;
			GUI.BeginGroup(new Rect(x, y, width, height), "", "box");
			GUI.Label(new Rect(10.0f, 10.0f, width, height), "Game Over!");
			GUI.EndGroup();
		}
	}

	State CurrentState;
	void SetState(State state)
	{
		if(CurrentState != null)
		{
			CurrentState.Stop();
			CurrentState.ActiveGame = null;
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
		
		// Create playfield.
		GameObject playfieldObject = new GameObject("Playfield");
		playfieldObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.5f);
		Playfield = playfieldObject.AddComponent<Playfield>();
		Playfield.Generate(PlayAreaHalfSize.x - 1.0f, PlayAreaHalfSize.y - 1.0f, 10.0f, BGTexture);

		SetState(new IntroGameState());
	}

	public bool CanPlaceAt(Player player, GamePieceBase piece, Vector3 pos, out List<GamePieceBase> collidedPieces)
	{
		return Playfield.CanPlaceAt(player, piece, pos, out collidedPieces);
	}

	public void GetCollision(GamePieceBase piece, out List<GamePieceBase> collidedPieces)
	{
		Playfield.GetCollision(piece, out collidedPieces);
	}

	public void Place(Player player, GamePieceBase piece)
	{
		if(piece.GetComponent<CirclePatch>() != null)
		{
			Playfield.Place(player, piece.GetComponent<CirclePatch>());
		}
		piece.Place();
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
		GUI.skin = GameGUISkin;
		CurrentState.OnGUI();
	}
	
	void Update()
	{
		CurrentState.Update();
	}
}
