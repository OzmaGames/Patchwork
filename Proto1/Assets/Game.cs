using UnityEngine;
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
	int CurrentRound = 0;

	[System.Serializable]
	public class PlayerSetting
	{
		public string Name;
		public GameObject GUINamePrefab;
		public GameObject GUIScorePrefab;
		public Gradient[] Colors;
		public Gradient ComplementColor;
		public Texture2D[] PatchPatterns;
		public Texture2D[] Decorations;
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
		public GameObject GUINamePrefab;
		public GameObject GUIScorePrefab;
	}
	List<PlayerInfo> players = new List<PlayerInfo>();
	List<PlayerInfo>.Enumerator activePlayer;
	
	GameObject Background;
	Playfield Playfield;

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

		// Fix aspect ratio of the text.
		float pixelRatio = (Camera.main.orthographicSize * 2.0f) / Camera.main.pixelHeight;
		GUIStatusPrefab.transform.localScale = new Vector3(pixelRatio * 10.0f, pixelRatio * 10.0f, pixelRatio * 0.1f);
		GUIStatusPrefab.GetComponent<TextMesh>().fontSize = 40;
		GUIRoundPrefab.transform.localScale = new Vector3(pixelRatio * 10.0f, pixelRatio * 10.0f, pixelRatio * 0.1f);
		GUIRoundPrefab.GetComponent<TextMesh>().fontSize = 40;
		GUIRoundPrefab.GetComponent<TextMesh>().text = "Round: " + (CurrentRound + 1) + " / " + NumRounds;

		// Add players.
		for(int i = 0; i < PlayerSettings.Length; ++i)
		{
			AddPlayer(PlayerSettings[i]);
		}	
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
	
	void AddPlayer(PlayerSetting playerSetting)
	{
		GameObject playerObject = new GameObject(playerSetting.Name);
		Player player = playerObject.AddComponent<Player>();

		player.GUINamePrefab = playerSetting.GUINamePrefab;
		// Fix aspect ratio of the text.
		float pixelRatio = (Camera.main.orthographicSize * 2.0f) / Camera.main.pixelHeight;
		player.GUINamePrefab.transform.localScale = new Vector3(pixelRatio * 10.0f, pixelRatio * 10.0f, pixelRatio * 0.1f);
		player.GUINamePrefab.GetComponent<TextMesh>().fontSize = 40;
		player.GUINamePrefab.GetComponent<TextMesh>().text = playerSetting.Name;

		player.GUIScorePrefab = playerSetting.GUIScorePrefab;
		// Fix aspect ratio of the text.
		pixelRatio = (Camera.main.orthographicSize * 2.0f) / Camera.main.pixelHeight;
		player.GUIScorePrefab.transform.localScale = new Vector3(pixelRatio * 10.0f, pixelRatio * 10.0f, pixelRatio * 0.1f);
		player.GUIScorePrefab.GetComponent<TextMesh>().fontSize = 40;
		player.GUIScorePrefab.GetComponent<TextMesh>().text = "Score: " + player.Score;

		player.ActiveGame = this;
		player.Colors = playerSetting.Colors;
		player.ComplementColor = playerSetting.ComplementColor;
		player.PatternTextures = playerSetting.PatchPatterns;
		player.Decorations = playerSetting.Decorations;

		players.Add(new PlayerInfo(player));
	}
	
	void ActivatePlayer(PlayerInfo playerInfo)
	{
		playerInfo.Player.ActivateTurn();

		// Let the circles advance one more segment.
		Playfield.ActivatePlayer(playerInfo.Player, true);
	}

	void NextPlayer()
	{
		// Activate next player.
		if(!activePlayer.MoveNext())
		{
			// Last player so rest back to first.
			activePlayer = players.GetEnumerator();
			activePlayer.MoveNext();
		}
		if((activePlayer.Current != null) && (activePlayer.Current.Round < 12))
		{
			ActivatePlayer(activePlayer.Current);
		}
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
	
	// Update is called once per frame
	bool done = false;
	void Update () {
		// Handle camera movement.
		if((someX != 0.0f) || (someY != 0.0f))
		{
			Vector3 pos = Camera.main.transform.position;
			Vector3 newPos = new Vector3(pos.x + someX, pos.y + someY, pos.z);
			if(newPos.x > PlayAreaHalfSize.x)
			{
				newPos.x = PlayAreaHalfSize.x;
			}
			else if(newPos.x < (-PlayAreaHalfSize.x))
			{
				newPos.x = -PlayAreaHalfSize.x;
			}
			if(newPos.y > PlayAreaHalfSize.y)
			{
				newPos.y = PlayAreaHalfSize.y;
			}
			else if(newPos.y < (-PlayAreaHalfSize.y))
			{
				newPos.y = -PlayAreaHalfSize.y;
			}
			Camera.main.transform.position = newPos;
		}

		if(CurrentRound >= NumRounds)
		{
			// GAME OVER!!!!
			if(!done)
			{
				GUIStatusPrefab.GetComponent<TextMesh>().text = "Game Over!";
				GUIStatusPrefab.SetActive(true);
				done = true;
			}
		}
		else
		{
			// Handle players turn.
			PlayerInfo currentPlayerInfo = activePlayer.Current;
			if(currentPlayerInfo != null)
			{
				if(currentPlayerInfo.Player.IsDone)
				{
					// Signal turn as done.
					currentPlayerInfo.Player.TurnOver();
					++currentPlayerInfo.Round;
					
					// Activate next player.
					if(!activePlayer.MoveNext())
					{
						// Last player so rest back to first.
						activePlayer = players.GetEnumerator();
						activePlayer.MoveNext();

						++CurrentRound;
						GUIRoundPrefab.GetComponent<TextMesh>().text = "Round: " + (CurrentRound + 1) + " / " + NumRounds;
					}
					if(CurrentRound < NumRounds)
					{
						ActivatePlayer(activePlayer.Current);
					}
				}
			}
			else
			{
				activePlayer = players.GetEnumerator();
				activePlayer.MoveNext();
				ActivatePlayer(activePlayer.Current);
			}
		}
	}
}
