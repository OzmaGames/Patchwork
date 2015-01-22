using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	//public GameObject PatchPrefab;
	[System.NonSerialized]
	public Texture2D[] PatternTextures;

	[System.NonSerialized]
	public GameObject GUINamePrefab;
	[System.NonSerialized]
	public GameObject GUIScorePrefab;
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

	GamePieceBase activePiece;	
	bool myTurn = false;
	bool isDone = false;
	public bool IsDone
	{
		get { return isDone; }
	}

	// Use this for initialization
	void Start () {
		Deck = gameObject.AddComponent<PlayerDeck>();
	}

	CirclePatch CreatePatch(int segments)
	{
		GameObject patchObject = new GameObject(gameObject.name + "_Patch");
		CirclePatch circlePatch = patchObject.AddComponent<CirclePatch>();
		circlePatch.Generate(segments, PatternTextures, Colors, ComplementColor);
		circlePatch.SetOwner(this);
		return circlePatch;
	}

	DecorationCircleStopper CreateDecorationCircleStopper()
	{
		GameObject decorationObject = new GameObject(gameObject.name + "_Decoration");
		DecorationCircleStopper decorationCircleStopper = decorationObject.AddComponent<DecorationCircleStopper>();
		Texture2D decorationTexture = Decorations[Random.Range(0, Decorations.Length)];
		decorationCircleStopper.Generate(1.0f, 1.0f, 1.0f, decorationTexture);
		return decorationObject.GetComponent<DecorationCircleStopper>();
	}

	const int MAX_TURNS_BEFORE_DECORATION = 2;
	int decorationRandomCounter = 0;
	GamePieceBase GetGamePiece()
	{
		bool shouldGiveDecoration = false;
		int r = Random.Range(0, MAX_TURNS_BEFORE_DECORATION);
		if((decorationRandomCounter == 0) || (r != 0))
		{
			++decorationRandomCounter;
		}
		if(decorationRandomCounter == MAX_TURNS_BEFORE_DECORATION)
		{
			shouldGiveDecoration = true;
		}
		if(shouldGiveDecoration)
		{
			decorationRandomCounter = 0;

			// Force a decoration
			return CreateDecorationCircleStopper();
		}

		int segments = Random.Range(2, 7);
		return CreatePatch(segments);
	}

	public void ActivateTurn()
	{
		isDone = false;
		myTurn = true;
		activePiece = GetGamePiece();

		// Highlight text.
		GUINamePrefab.GetComponent<TextMesh>().color = Color.red;
	}

	public void TurnOver()
	{
		myTurn = false;
		activePiece = null;

		// Normal text.
		GUINamePrefab.GetComponent<TextMesh>().color = Color.white;
	}

	public void AddScore(int amount)
	{
		Score += amount;

		// Update score.
		GUIScorePrefab.GetComponent<TextMesh>().text = "Score: " + Score;
	}

	// Update is called once per frame
	void Update () {
		if(myTurn && (!isDone))
		{
			if(activePiece != null)
			{
				Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				pz.z = 0.0f;
				activePiece.SetPosition(pz.x, pz.y);
			}
			if(Input.GetMouseButtonDown(0))
			{
				if(activePiece != null)
				{
					if(ActiveGame.CanPlaceAt(this, activePiece, activePiece.transform.position))
					{
						ActiveGame.Place(this, activePiece);
						activePiece = null;
						isDone = true;
					}
				}
			}
		}
	}
}
