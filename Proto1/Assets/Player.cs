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

		/*GameObject deckObj = new GameObject(gameObject.name + "_Deck");
		deckObj.transform.localPosition = new Vector3(0.0f, -5.0f, 0.0f);
		Deck = deckObj.AddComponent<PlayerDeck>();
		Deck.Generate(20, 20, 2, 1, this);*/
	}
	
	public void ActivateTurn()
	{
		isDone = false;
		myTurn = true;

		// Highlight text.
		//GUIStats.SetHighlight(true);

		// Activate deck.
		Deck.ActivateTurn();
		Deck.Show();
	}

	public void TurnOver()
	{
		myTurn = false;

		// Normal text.
		//GUIStats.SetHighlight(false);

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

	void Update()
	{
		if(myTurn && (!isDone) && (ConfirmPlacementPrefab.activeSelf == false))
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
					if(ActivePlayfield.CanPlaceAt(this, activePiece, activePiece.transform.position, out collidedPieces))
					{
						/*Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
						Bounds bounds = activePiece.GetComponent<Renderer>().bounds;
						bool collide = GeometryUtility.TestPlanesAABB(planes, bounds);
						Debug.Log(collide);*/

						var btnAccept = ConfirmPlacementPrefab.transform.FindChild("Accept").GetComponent<UnityEngine.UI.Button>();
						var btnDecline = ConfirmPlacementPrefab.transform.FindChild("Decline").GetComponent<UnityEngine.UI.Button>();
						btnAccept.onClick.AddListener(() => ConfirmPlacement());
						btnDecline.onClick.AddListener(() => DeclinePlacement());

						Vector3 confirmPos = new Vector3(pz.x + 1.0f, pz.y - 0.25f, ConfirmPlacementPrefab.transform.position.z);
						Vector3 vp = Camera.main.WorldToViewportPoint(confirmPos);
						if(vp.x > 0.95f)
						{
							confirmPos.x -= 2.0f;
						}
						if(vp.y < 0.025f)
						{
							confirmPos.y += 0.5f;
						}
						Debug.Log(vp.y);
						ConfirmPlacementPrefab.transform.position = confirmPos;
						
						ConfirmPlacementPrefab.SetActive(true);
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
							if(collidedPieces.Count == 0)
							{
								PutBackInHand();
							}
							else
							{
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
				}
			}
			else
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
