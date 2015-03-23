using UnityEngine;
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
	public PlayerDeck Deck;
	[System.NonSerialized]
	public static GameObject DeckObjectPrefab;
	public GameObject DeckObject;

	[System.NonSerialized]
	public Gradient[] Colors;
	[System.NonSerialized]
	public Gradient ComplementColor;
	[System.NonSerialized]
	public Texture2D[] Decorations;

	[System.NonSerialized]
	public GameObject ConfirmPlacementPrefab;

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
		DeckObject = Instantiate(DeckObjectPrefab);
		DeckObject.transform.SetParent(GameObject.Find("Canvas").transform, false);
		Deck = DeckObject.GetComponent<PlayerDeck>();
		Deck.Generate(20, 20, this);
	}
	
	public void ActivateTurn()
	{
		isDone = false;
		myTurn = true;

		// Activate deck.
		Deck.ActivateTurn();
		Deck.Show();
	}

	public void TurnOver()
	{
		myTurn = false;

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
		Colors = null;
		ComplementColor = null;
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

		var btnAccept = ConfirmPlacementPrefab.transform.FindChild("Accept").GetComponent<UnityEngine.UI.Button>();
		var btnDecline = ConfirmPlacementPrefab.transform.FindChild("Decline").GetComponent<UnityEngine.UI.Button>();
		btnAccept.onClick.RemoveAllListeners();
		btnDecline.onClick.RemoveAllListeners();
		ConfirmPlacementPrefab.SetActive(false);
	}

	public void DeclinePlacement()
	{
		PutBackInHand();

		var btnAccept = ConfirmPlacementPrefab.transform.FindChild("Accept").GetComponent<UnityEngine.UI.Button>();
		var btnDecline = ConfirmPlacementPrefab.transform.FindChild("Decline").GetComponent<UnityEngine.UI.Button>();
		btnAccept.onClick.RemoveAllListeners();
		btnDecline.onClick.RemoveAllListeners();
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

	void PlacePiece(Vector3 pos)
	{
		activePiece.SetPosition(pos.x, pos.y);

		// Place a piece.
		List<GamePieceBase> collidedPieces;
		if(ActivePlayfield.CanPlaceAt(this, activePiece, activePiece.transform.position, out collidedPieces))
		{
			var btnAccept = ConfirmPlacementPrefab.transform.FindChild("Accept").GetComponent<UnityEngine.UI.Button>();
			var btnDecline = ConfirmPlacementPrefab.transform.FindChild("Decline").GetComponent<UnityEngine.UI.Button>();
			btnAccept.onClick.AddListener(() => ConfirmPlacement());
			btnDecline.onClick.AddListener(() => DeclinePlacement());

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
			for(int i = 0; i < collidedPieces.Count; ++i)
			{
				collidedPieces[i].StartFlash(WARNING_FLASH_COLOR_1, WARNING_FLASH_COLOR_2, CirclePatch.FLASH_TIME);
			}
			PutBackInHand();
		}
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
			if(activePiece != null)
			{
				if(Input.GetMouseButton(0))
				{
					// Move piece.
					activePiece.SetPosition(mouseWorldPosition.x, mouseWorldPosition.y);
				}
				else
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
