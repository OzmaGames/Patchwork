﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	[System.NonSerialized]
	public Texture2D[] PatternTextures;

	[System.NonSerialized]
	public GameObject DecorationPrefab;
	[System.NonSerialized]
	public Playfield ActivePlayfield;
	[System.NonSerialized]
	public int MinPatchSize = 2;
	[System.NonSerialized]
	public int NumPatchesInDeck = 20;
	[System.NonSerialized]
	public int NumDecorationsInDeck = 20;
	[System.NonSerialized]
	public PlayerDeck Deck;
	[System.NonSerialized]
	public static GameObject DeckObjectPrefab;
	public GameObject DeckObject;

	[System.NonSerialized]
	public Game ActiveGame;

	[System.NonSerialized]
	public Game.PlayerPalette Palette;
	[System.NonSerialized]
	public Texture2D[] Decorations;

	[System.NonSerialized]
	public GameObject ConfirmPlacementPrefab;
	[System.NonSerialized]
	public GameObject CancelPlacementPrefab;

	public int Score = 0;

	static Color WARNING_FLASH_COLOR_1 = new Color(0.5f, 0.0f, 0.0f);
	static Color WARNING_FLASH_COLOR_2 = new Color(-0.5f, -0.5f, -0.5f);

	GamePieceBase activePiece;	
	bool myTurn = false;
	bool isDone = false;

	void Start () {
		//GetNewDeck();
	}

	public void GetNewDeck()
	{
		// Create deck.
		DeckObject = Instantiate(DeckObjectPrefab);
		DeckObject.transform.SetParent(GameObject.Find("Canvas").transform, false);
		Deck = DeckObject.GetComponent<PlayerDeck>();
		Deck.Generate(MinPatchSize, NumPatchesInDeck, NumDecorationsInDeck, this);
	}
	
	public void ActivateTurn()
	{
		isDone = false;
		myTurn = true;

		// Activate deck.
		if(DeckObject == null)
		{
			GetNewDeck();
		}
		Deck.ActivateTurn();
		Deck.Show();
	}

	public void TurnOver()
	{
		isDone = true;
		myTurn = false;

		// Clean up.
		if(activePiece != null)
		{
			if(Deck.AddToHand(activePiece))
			{
				activePiece = null;
			}

			ConfirmPlacementPrefab.GetComponent<UIConfirmPlacement>().ActivePlayer = null;
			ConfirmPlacementPrefab.SetActive(false);
			CancelPlacementPrefab.GetComponent<UIConfirmPlacement>().ActivePlayer = null;
			CancelPlacementPrefab.SetActive(false);
		}

		// Hide deck.
		Deck.Hide();
	}

	public void AddScore(int amount)
	{
		Score += amount;
	}

	void OnDestroy()
	{
		PatternTextures = null;
		DecorationPrefab = null;
		ActivePlayfield = null;
		Destroy(DeckObject);
		Deck = null;
		Decorations = null;
		if(activePiece != null)
		{
			Destroy(activePiece.gameObject);
			activePiece = null;
		}
	}

	public void ConfirmPlacement()
	{
		// Save local refernece to piece and clear active piece.
		GamePieceBase piece = activePiece;
		activePiece = null;

		// Place and tell game we're done.
		ActivePlayfield.Place(this, piece);
		isDone = true;

		ConfirmPlacementPrefab.SetActive(false);
	}

	public void DeclinePlacement()
	{
		PutBackInHand();
		ConfirmPlacementPrefab.SetActive(false);
	}

	void PutBackInHand()
	{
		if(Deck.AddToHand(activePiece))
		{
			activePiece = null;
			Deck.Show();
		}
	}

	bool illegalPlacement = false;
	void PlacePiece(Vector3 pos)
	{
		activePiece.SetPosition(pos.x, pos.y);

		// Place a piece.
		List<GamePieceBase> collidedPieces;
		if(ActivePlayfield.CanPlaceAt(this, activePiece, activePiece.transform.position, out collidedPieces))
		{
			ConfirmPlacementPrefab.GetComponent<UIConfirmPlacement>().ActivePlayer = this;

			Vector3 confirmPos = new Vector3(pos.x + 1.0f, pos.y - 0.25f, ConfirmPlacementPrefab.transform.position.z);
			Vector3 vp = Camera.main.WorldToViewportPoint(confirmPos);
			if(vp.x > 0.95f)
			{
				confirmPos.x -= 2.0f;
			}
			if(vp.y < 0.025f)
			{
				confirmPos.y += 0.5f;
			}
			ConfirmPlacementPrefab.transform.position = confirmPos;
			
			ConfirmPlacementPrefab.SetActive(true);
		}
		else
		{
			illegalPlacement = true;
			for(int i = 0; i < collidedPieces.Count; ++i)
			{
				collidedPieces[i].StartFlash(WARNING_FLASH_COLOR_1, WARNING_FLASH_COLOR_2, CirclePatch.FLASH_TIME);
			}
			//PutBackInHand();
		}
	}

	public bool IsDone()
	{
		return isDone && ActivePlayfield.IsDone();
	}

	void Update()
	{
		if(myTurn && (!isDone) && (ConfirmPlacementPrefab.activeSelf == false))
		{
			Vector3 mousePosition = Input.mousePosition;

			// Make sure mouse position is inside screen.
			mousePosition.x = Mathf.Clamp(mousePosition.x, 0.0f, Screen.width);
			mousePosition.y = Mathf.Clamp(mousePosition.y, 0.0f, Screen.height);

			Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
			mouseWorldPosition.z = 0.0f;
			/*Vector3 vp = Camera.main.WorldToViewportPoint(mouseWorldPosition);
			vp.x = Mathf.Clamp(vp.x, 0.045f, 0.955f);
			vp.y = Mathf.Clamp(vp.y, 0.07f, 0.93f);
			Vector3 vpWorld = Camera.main.ViewportToWorldPoint(vp);
			mouseWorldPosition.x = vpWorld.x;
			mouseWorldPosition.y = vpWorld.y;*/
			if(activePiece != null)
			{
				if(Input.GetMouseButton(0))
				{
					// Move piece.
					activePiece.SetPosition(mouseWorldPosition.x, mouseWorldPosition.y);
					illegalPlacement = false;
				}
				else if(!illegalPlacement)
				{
					// Try to place.
					PlacePiece(mouseWorldPosition);
				}
			}
			else
			{
				// Grab a piece.
				activePiece = Deck.GetPiece(mouseWorldPosition);
				if(activePiece != null)
				{
					Deck.Hide();
				}
			}
		}
	}
}
