using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	public GameObject PlayerPrefab;
	public GameObject BackgroundPrefab;
	public Texture2D BGTexture;

	List<Player> players = new List<Player>();
	List<Player>.Enumerator activePlayer;

	struct PlacedPatch
	{
		public PlacedPatch(Player owner, CirclePatch patch)
		{
			Owner = owner;
			Patch = patch;
		}

		public Player Owner;
		public CirclePatch Patch;
	}
	List<PlacedPatch> patches = new List<PlacedPatch>();

	GameObject Background;

	public GameObject PlayerName;

	// Use this for initialization
	void Start () {
		CirclePatch.GenerateSegments(8, 1.0f);
		AddPlayer("Player1", new Color[]{
			new Color(77.0f / 255.0f,	179.0f / 255.0f,	64.0f / 255.0f),	// Limegul (komplement)
			new Color(236.0f / 255.0f,	248.0f / 255.0f,	127.0f / 255.0f),
			new Color(167.0f / 255.0f,	19.0f / 255.0f,		43.0f / 255.0f),	// Röd
			new Color(238.0f / 255.0f,	77.0f / 255.0f,		85.0f / 255.0f),
			new Color(150.0f / 255.0f,	15.0f / 255.0f,		92.0f / 255.0f),	// Rosa
			new Color(238.0f / 255.0f,	77.0f / 255.0f,		141.0f / 255.0f),
			new Color(111.0f / 255.0f,	16.0f / 255.0f,		100.0f / 255.0f),	// Violett
			new Color(253.0f / 255.0f,	110.0f / 255.0f,	229.0f / 255.0f),
			new Color(71.0f / 255.0f,	8.0f / 255.0f,		88.0f / 255.0f),	// Lila
			new Color(234.0f / 255.0f,	177.0f / 255.0f,	249.0f / 255.0f),
/*			new Color(193.0f / 255.0f,	41.0f / 255.0f,		66.0f / 255.0f),
			new Color(143.0f / 255.0f,	18.0f / 255.0f,		113.0f / 255.0f),
			new Color(99.0f / 255.0f,	35.0f / 255.0f,		117.0f / 255.0f),
			new Color(217.0f / 255.0f,	141.0f / 255.0f,	207.0f / 255.0f),
			new Color(219.0f / 255.0f,	232.0f / 255.0f,	109.0f / 255.0f),*/
		});
		AddPlayer("Player2", new Color[]{
			new Color(243.0f / 255.0f,	143.0f / 255.0f,	25.0f / 255.0f),	// Brandgul (komplement)
			new Color(249.0f / 255.0f,	245.0f / 255.0f,	130.0f / 255.0f),
			new Color(31.0f / 255.0f,	105.0f / 255.0f,	20.0f / 255.0f),	// Grön
			new Color(187.0f / 255.0f,	244.0f / 255.0f,	144.0f / 255.0f),
			new Color(17.0f / 255.0f,	138.0f / 255.0f,	125.0f / 255.0f),	// Jade
			new Color(129.0f / 255.0f,	245.0f / 255.0f,	197.0f / 255.0f),
			new Color(9.0f / 255.0f,	127.0f / 255.0f,	171.0f / 255.0f),	// Turkos
			new Color(127.0f / 255.0f,	232.0f / 255.0f,	241.0f / 255.0f),
			new Color(14.0f / 255.0f,	22.0f / 255.0f,		103.0f / 255.0f),	// Blå
			new Color(94.0f / 255.0f,	168.0f / 255.0f,	236.0f / 255.0f),
/*			new Color(44.0f / 255.0f,	47.0f / 255.0f,		135.0f / 255.0f),
			new Color(37.0f / 255.0f,	91.0f / 255.0f,		156.0f / 255.0f),
			new Color(83.0f / 255.0f,	183.0f / 255.0f,	217.0f / 255.0f),
			new Color(64.0f / 255.0f,	163.0f / 255.0f,	57.0f / 255.0f),
			new Color(243.0f / 255.0f,	143.0f / 255.0f,	25.0f / 255.0f),*/
		});

		Background = (GameObject)Instantiate(BackgroundPrefab);
		Background bg = Background.GetComponent<Background>();
		bg.Generate(50.0f, 50.0f, 10.0f, BGTexture);
	}

	public void Place(Player player, CirclePatch patch)
	{
		patches.Add(new PlacedPatch(player, patch));
		patch.Place();
	}

	void AddPlayer(string name, Color[] colors)
	{
		GameObject playerObject = (GameObject)Instantiate(PlayerPrefab);
		Player player = playerObject.GetComponent<Player>();
		player.ActiveGame = this;
		player.Name = name;
		player.Colors = colors;
		player.GradientTexture = CirclePatch.CreateGradientTexture(colors);

		players.Add(player);
	}

	void CheckForCollision()
	{
		// Check for collision.
		for(int p1 = 0; p1 < patches.Count; ++p1)
		{
			CirclePatch patch1 = patches[p1].Patch;
			for(int p2 = 0; p2 < patches.Count; ++p2)
			{
				CirclePatch patch2 = patches[p2].Patch;
				if((patch1 != patch2) && patch1.CollidesAgainst(patch2))
				{
					patch1.SetCollided(true);
					patch2.SetCollided(true);
					//Debug.Log("COLLIDE!");
				}
			}
		}
	}

	void ActivatePlayer(Player player)
	{
		player.ActivateTurn();

		//CheckForCollision();

		// Let the circles advance one more segment.
		for(int p = 0; p < patches.Count; ++p)
		{
			if(patches[p].Owner == player)
			{
				CirclePatch patch = patches[p].Patch;
				if(!patch.HasCollided())
				{
					patch.NextSegment();
				}
			}
		}

		// Set active players name.
		PlayerName.GetComponent<TextMesh>().text = player.Name;
	}
	
	// Update is called once per frame
	void Update () {
		CheckForCollision();

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
