using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	public Texture2D BGTexture;

	public int NumberofPatchesPerPlayer = 8;
	public int NumberOfDecorationsPerPlayer = 2;
	public Vector2 PlayAreaHalfSize = new Vector2(25.0f, 25.0f);

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

	List<Player> players = new List<Player>();
	List<Player>.Enumerator activePlayer;
	
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

		// Add players.
		for(int i = 0; i < PlayerSettings.Length; ++i)
		{
			AddPlayer(PlayerSettings[i]);
		}	
	}

	public bool CanPlaceAt(Player player, GamePieceBase piece, Vector3 pos)
	{
		return Playfield.CanPlaceAt(player, piece, pos);
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

		players.Add(player);
	}
	
	void ActivatePlayer(Player player)
	{
		player.ActivateTurn();

		// Let the circles advance one more segment.
		Playfield.ActivatePlayer(player, true);
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

		// Handle players turn.
		Player currentPlayer = activePlayer.Current;
		if(currentPlayer != null)
		{
			if(currentPlayer.IsDone)
			{
				// Signal turn as done.
				currentPlayer.TurnOver();

				// Activate next player.
				if(!activePlayer.MoveNext())
				{
					// Lasy player so rest back to first.
					activePlayer = players.GetEnumerator();
					activePlayer.MoveNext();
				}
				ActivatePlayer(activePlayer.Current);
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
