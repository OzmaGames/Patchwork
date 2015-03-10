﻿using UnityEngine;
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

	public GameObject GUIStatusPrefab;
	public GameObject GUIRoundPrefab;

	public Texture2D BGTexture;

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
		public GameObject GUIStatsPrefab;
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

		// GUI.
		//public GameObject GUINamePrefab;
		//public GameObject GUIScorePrefab;
	}

	GameObject Background;
	Playfield Playfield;

	abstract class State
	{
		public Game ActiveGame;

		public abstract void Start();
		public abstract void Stop();
		public abstract void Update();
	}

	class IntroGameState : State
	{
		public override void Start()
		{
		}

		public override void Stop()
		{
		}
		
		public override void Update()
		{
			ActiveGame.SetState(new StartGameState());
		}
	}

	class StartGameState : State
	{
		List<PlayerInfo> Players = new List<PlayerInfo>();

		public override void Start()
		{
			// Add players.
			for(int i = 0; i < ActiveGame.PlayerSettings.Length; ++i)
			{
				AddPlayer(ActiveGame.PlayerSettings[i]);
			}	
		}
		
		public override void Stop()
		{
		}
		
		public override void Update()
		{
			ActiveGame.SetState(new MainGameState(Players));
		}

		void AddPlayer(PlayerSetting playerSetting)
		{
			GameObject playerObject = new GameObject(playerSetting.Name);
			Player player = playerObject.AddComponent<Player>();

			player.GUIStats = playerSetting.GUIStatsPrefab.GetComponent<GUI_PlayerStats>();
			player.GUIStats.SetName(playerSetting.Name);
			player.GUIStats.SetScore("Score: " + player.Score);

			player.ActiveGame = ActiveGame;
			player.Colors = ActiveGame.Palette[playerSetting.PaletteIndex].Colors;
			player.ComplementColor = ActiveGame.Palette[playerSetting.PaletteIndex].ComplementColor;
			player.PatternTextures = playerSetting.PatchPatterns;
			player.Decorations = playerSetting.Decorations;
			
			Players.Add(new PlayerInfo(player));
		}
	}

	class MainGameState : State
	{
		int CurrentRound = 0;
		List<PlayerInfo> Players = new List<PlayerInfo>();
		List<PlayerInfo>.Enumerator ActivePlayer;

		public MainGameState(List<PlayerInfo> players)
		{
			Players = players;
		}

		public override void Start()
		{
			// Show GUI.
			// Fix aspect ratio of the text and activate.
			float pixelRatio = (Camera.main.orthographicSize * 2.0f) / Camera.main.pixelHeight;
			ActiveGame.GUIRoundPrefab.transform.localScale = new Vector3(pixelRatio * 10.0f, pixelRatio * 10.0f, pixelRatio * 0.1f);
			ActiveGame.GUIRoundPrefab.GetComponent<TextMesh>().fontSize = 40;
			ActiveGame.GUIRoundPrefab.GetComponent<TextMesh>().text = "Round: " + (CurrentRound + 1) + " / " + ActiveGame.NumRounds;
			ActiveGame.GUIRoundPrefab.SetActive(true);
			for(int i = 0; i < Players.Count; ++i)
			{
				Players[i].Player.GUIStats.Show();
			}
		}

		public override void Stop()
		{
			// Hide GUI.
			ActiveGame.GUIRoundPrefab.SetActive(false);
			for(int i = 0; i < Players.Count; ++i)
			{
				Players[i].Player.GUIStats.Hide();
			}
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
							ActiveGame.GUIRoundPrefab.GetComponent<TextMesh>().text = "Round: " + (CurrentRound + 1) + " / " + ActiveGame.NumRounds;
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
			// Fix aspect ratio of the text.
			float pixelRatio = (Camera.main.orthographicSize * 2.0f) / Camera.main.pixelHeight;
			ActiveGame.GUIStatusPrefab.transform.localScale = new Vector3(pixelRatio * 10.0f, pixelRatio * 10.0f, pixelRatio * 0.1f);
			ActiveGame.GUIStatusPrefab.GetComponent<TextMesh>().fontSize = 40;
			ActiveGame.GUIStatusPrefab.GetComponent<TextMesh>().text = "Game Over!";
			ActiveGame.GUIStatusPrefab.SetActive(true);
		}
		
		public override void Stop()
		{
			ActiveGame.GUIStatusPrefab.SetActive(false);
		}
		
		public override void Update()
		{
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
		bg.Generate(PlayAreaHalfSize.x - 1.0f, PlayAreaHalfSize.y - 1.0f, 10.0f, BGTexture);		
		
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
	}
	
	void Update()
	{
		CurrentState.Update();
	}
}
