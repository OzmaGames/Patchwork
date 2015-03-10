using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	//public GameObject PatchPrefab;
	[System.NonSerialized]
	public Texture2D[] PatternTextures;

	[System.NonSerialized]
	public GUI_PlayerStats GUIStats;
	[System.NonSerialized]
	public GameObject DecorationPrefab;
	[System.NonSerialized]
	public Game ActiveGame;
	[System.NonSerialized]
	public PlayerDeck Deck;

	[System.NonSerialized]
	public Gradient[] Colors;
	[System.NonSerialized]
	public Gradient ComplementColor;
	[System.NonSerialized]
	public Texture2D[] Decorations;

	public int Score = 0;

	static Color WARNING_FLASH_COLOR_1 = new Color(0.5f, 0.0f, 0.0f);
	static Color WARNING_FLASH_COLOR_2 = new Color(-0.5f, -0.5f, -0.5f);

	GamePieceBase activePiece;	
	bool myTurn = false;
	bool isDone = false;
	public bool IsDone
	{
		get { return isDone; }
	}

	// Use this for initialization
	void Start () {
		// Create deck.
		GameObject deckObj = new GameObject(gameObject.name + "_Deck");
		deckObj.transform.localPosition = new Vector3(0.0f, -5.0f, 0.0f);
		Deck = deckObj.AddComponent<PlayerDeck>();
		Deck.Generate(20, 20, 2, 1, this);
	}
	
	public void ActivateTurn()
	{
		isDone = false;
		myTurn = true;

		// Highlight text.
		GUIStats.SetHighlight(true);

		// Activate deck.
		Deck.ActivateTurn();
		Deck.Show();
	}

	public void TurnOver()
	{
		myTurn = false;
		activePiece = null;

		// Normal text.
		GUIStats.SetHighlight(false);

		// Hide deck.
		Deck.Hide();
	}

	public void AddScore(int amount)
	{
		Score += amount;

		// Update score.
		GUIStats.SetScore("Score: " + Score);
	}

	void Update()
	{
		if(myTurn && (!isDone))
		{
			Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			pz.z = 0.0f;
			if(activePiece != null)
			{
				activePiece.SetPosition(pz.x, pz.y);

				if(Input.GetMouseButtonDown(0))
				{
					// Place a piece.
					List<GamePieceBase> collidedPieces;
					if(ActiveGame.CanPlaceAt(this, activePiece, activePiece.transform.position, out collidedPieces))
					{
						ActiveGame.Place(this, activePiece);
						activePiece = null;
						isDone = true;
					}
					else
					{
						CirclePatch myPatch = activePiece.GetComponent<CirclePatch>();
						if(myPatch != null)
						{
							for(int i = 0; i < collidedPieces.Count; ++i)
							{
								CirclePatch patch = collidedPieces[i].GetComponent<CirclePatch>();
								if(patch != null)
								{
									patch.StartFlash(WARNING_FLASH_COLOR_1, WARNING_FLASH_COLOR_2, CirclePatch.FLASH_TIME);
								}
							}
						}
						DecorationCircleStopper myDecoration = activePiece.GetComponent<DecorationCircleStopper>();
						if(myDecoration != null)
						{
							//myDecoration.SetHighlight(true, new Color(0.5f, 0.0f, 0.0f));
							for(int i = 0; i < collidedPieces.Count; ++i)
							{
								DecorationCircleStopper decoration = collidedPieces[i].GetComponent<DecorationCircleStopper>();
								if(decoration != null)
								{
									decoration.StartFlash(WARNING_FLASH_COLOR_1, WARNING_FLASH_COLOR_2, CirclePatch.FLASH_TIME);
								}
							}
						}
					}
				}
				if(Input.GetMouseButtonDown(1))
				{
					if(Deck.AddToHand(activePiece))
					{
						activePiece = null;
						Deck.Show();
					}
				}
			}
			else
			{
				if(Input.GetMouseButtonDown(0))
				{
					// Grab a piece.
					activePiece = Deck.GetPiece(pz);
					if(activePiece != null)
					{
						Deck.Hide();
					}
				}
			}
		}
	}
}
