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
		AddPlayer("Player1");
		AddPlayer("Player2");
		activePlayer = players.GetEnumerator();
		activePlayer.MoveNext();
		ActivatePlayer(activePlayer.Current);
	}

	void AddPlayer(string name)
	{
		GameObject playerObject = (GameObject)Instantiate(PlayerPrefab);
		Player player = playerObject.GetComponent<Player>();
		player.GetComponent<Player>().Name = name;
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
	}
}
