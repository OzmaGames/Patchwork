using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	public GameObject PlayerPrefab;

	List<Player> players = new List<Player>();
	List<Player>.Enumerator activePlayer;

	public GameObject PlayerName;

	// Use this for initialization
	void Start () {
		AddPlayer("Player1", new Color[]{
			new Color(193.0f / 255.0f,	41.0f / 255.0f,		66.0f / 255.0f),
			new Color(143.0f / 255.0f,	18.0f / 255.0f,		113.0f / 255.0f),
			new Color(99.0f / 255.0f,	35.0f / 255.0f,		117.0f / 255.0f),
			new Color(217.0f / 255.0f,	141.0f / 255.0f,	207.0f / 255.0f),
			new Color(219.0f / 255.0f,	232.0f / 255.0f,	109.0f / 255.0f),
		});
		AddPlayer("Player2", new Color[]{
			new Color(44.0f / 255.0f,	47.0f / 255.0f,		135.0f / 255.0f),
			new Color(37.0f / 255.0f,	91.0f / 255.0f,		156.0f / 255.0f),
			new Color(83.0f / 255.0f,	183.0f / 255.0f,	217.0f / 255.0f),
			new Color(64.0f / 255.0f,	163.0f / 255.0f,	57.0f / 255.0f),
			new Color(243.0f / 255.0f,	143.0f / 255.0f,	25.0f / 255.0f),
		});
	}

	void AddPlayer(string name, Color[] colors)
	{
		GameObject playerObject = (GameObject)Instantiate(PlayerPrefab);
		Player player = playerObject.GetComponent<Player>();
		player.GetComponent<Player>().Name = name;
		player.GetComponent<Player>().Colors = colors;
		player.GradientTexture = CirclePatch.CreateGradientTexture(colors);

		players.Add(player);
	}

	void ActivatePlayer(Player player)
	{
		player.ActivateTurn();
		
		// Set active players name.
		PlayerName.GetComponent<TextMesh>().text = player.Name;
	}

	// Update is called once per frame
	void Update () {
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
