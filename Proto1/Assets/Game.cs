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
			new Color(0.753906f, 0.160156f, 0.257812f),
			new Color(0.558594f, 0.0703125f, 0.441406f),
			new Color(0.386719f, 0.136719f, 0.457031f),
			new Color(0.847656f, 0.550781f, 0.808594f)
		});
		AddPlayer("Player2", new Color[]{
			new Color(0.9f, 0.1f, 0.8f),
			new Color(0.9f, 0.1f, 0.2f),
			new Color(0.9f, 0.0f, 0.6f),
			new Color(0.9f, 0.9f, 0.8f)
		});
	}

	void AddPlayer(string name, Color[] colors)
	{
		GameObject playerObject = (GameObject)Instantiate(PlayerPrefab);
		Player player = playerObject.GetComponent<Player>();
		player.GetComponent<Player>().Name = name;
		player.GetComponent<Player>().Colors = colors;

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
