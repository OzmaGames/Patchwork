﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerMainGameState : GameState
{
	int CurrentRound = 0;
	List<Game.PlayerInfo> Players = new List<Game.PlayerInfo>();
	List<Game.PlayerInfo>.Enumerator ActivePlayer;
	Playfield ActivePlayfield;
	
	UnityEngine.UI.Text txtPlayer1Name;
	UnityEngine.UI.Text txtPlayer1Score;
	UnityEngine.UI.Text txtPlayer2Name;
	UnityEngine.UI.Text txtPlayer2Score;
	UnityEngine.UI.Text txtTurn;
	
	public override void Start()
	{
		ActiveGame.abortGameSession = false;
		
		ActiveGame.QuitPrefab.SetActive(true);
		
		// Create playfield.
		GameObject playfieldObject = new GameObject("Playfield");
		playfieldObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.5f);
		ActivePlayfield = playfieldObject.AddComponent<MultiplayerPlayfield>();
		ActivePlayfield.Generate(ActiveGame.PlayAreaHalfSize.x - 1.0f, ActiveGame.PlayAreaHalfSize.y - 1.0f, 10.0f, ActiveGame.BGTexture);
		
		// Create players.
		for(int i = 0; i < ActiveGame.PlayerSettings.Length; ++i)
		{
			AddPlayer(ActiveGame.PlayerSettings[i]);
		}
		
		// Setup UI.
		ActiveGame.PlayerStatsPrefab.SetActive(true);
		txtPlayer1Name = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Name").GetComponent<UnityEngine.UI.Text>();
		txtPlayer1Score = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player1").FindChild("Score").GetComponent<UnityEngine.UI.Text>();
		txtPlayer2Name = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player2").FindChild("Name").GetComponent<UnityEngine.UI.Text>();
		txtPlayer2Score = ActiveGame.PlayerStatsPrefab.transform.FindChild("Player2").FindChild("Score").GetComponent<UnityEngine.UI.Text>();
		ActiveGame.TurnPrefab.SetActive(true);
		txtTurn = ActiveGame.TurnPrefab.GetComponent<UnityEngine.UI.Text>();
		//			ActiveGame.HelpPrefab.SetActive(true);
		UpdateUI();
	}
	
	public override void Stop()
	{
		//			ActiveGame.HelpPrefab.SetActive(false);
		ActiveGame.PlayerStatsPrefab.SetActive(false);
		ActiveGame.TurnPrefab.SetActive(false);
		ActiveGame.QuitPrefab.SetActive(false);
	}
	
	public override void Update()
	{
		// Handle camera movement.
		/*if((ActiveGame.someX != 0.0f) || (ActiveGame.someY != 0.0f))
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
			}*/
		
		if((CurrentRound >= ActiveGame.NumRounds) || (ActiveGame.abortGameSession))
		{
			Game.PlayerInfo currentPlayerInfo = ActivePlayer.Current;
			if(currentPlayerInfo != null)
			{
				currentPlayerInfo.Player.TurnOver();
			}
			
			// GAME OVER!!!!
			ActivePlayfield.HideSymbols();
			ActiveGame.SetState(new Game.GameOverState(Players, ActivePlayfield));
			return;
		}
		else
		{
			// Handle players turn.
			Game.PlayerInfo currentPlayerInfo = ActivePlayer.Current;
			if(currentPlayerInfo != null)
			{
				if(currentPlayerInfo.Player.IsDone())
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
		
		UpdateUI();
	}
	
	void UpdateUI()
	{
		if(ActivePlayer.Current == Players[0])
		{
			txtPlayer1Name.color = ActiveGame.PlayerHighlightColor;
			txtPlayer2Name.color = ActiveGame.PlayerNormalColor;
		}
		else if(ActivePlayer.Current == Players[1])
		{
			txtPlayer1Name.color = ActiveGame.PlayerNormalColor;
			txtPlayer2Name.color = ActiveGame.PlayerHighlightColor;
		}
		else
		{
			txtPlayer1Name.color = ActiveGame.PlayerNormalColor;
			txtPlayer2Name.color = ActiveGame.PlayerNormalColor;
		}
		txtPlayer1Name.text = Players[0].Player.gameObject.name;
		txtPlayer1Score.text = Players[0].Player.Score.ToString();
		txtPlayer2Name.text = Players[1].Player.gameObject.name;
		txtPlayer2Score.text = Players[1].Player.Score.ToString();
		txtTurn.text = "turn " + (CurrentRound + 1) + "/" + ActiveGame.NumRounds;
	}
	
	void AddPlayer(Game.PlayerSetting playerSetting)
	{
		GameObject playerObject = new GameObject(playerSetting.Name);
		Player player = playerObject.AddComponent<Player>();
		
		player.ActivePlayfield = ActivePlayfield;
		player.Colors = playerSetting.Palette.Colors;
		player.ComplementColor = playerSetting.Palette.ComplementColor;
		player.PatternTextures = playerSetting.PatchPatterns;
		player.Decorations = playerSetting.Decorations;
		player.ConfirmPlacementPrefab = ActiveGame.ConfirmPlacementPrefab;
		player.ActiveGame = ActiveGame;
		
		Players.Add(new Game.PlayerInfo(player));
	}
	
	void ActivatePlayer(Game.PlayerInfo playerInfo)
	{
		playerInfo.Player.ActivateTurn();
		
		// Let the circles advance one more segment.
		ActivePlayfield.ActivatePlayer(playerInfo.Player, true);
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
