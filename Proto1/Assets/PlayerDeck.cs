using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerDeck : MonoBehaviour
{

	Player Owner;
	public int NumPatches = 0;
	public int NumDecorations = 0;
	public Stack<CirclePatch.PatchConfig> PatchConfigs;

	public GameObject PatchPrefab;
	public GameObject DecorationPrefab;
	public GameObject[] PatchesOnHand;
	public GameObject[] DecorationsOnHand;

	class HandSlot
	{
		public GamePieceBase Piece;
		public GameObject uiButton;

		public HandSlot()
		{
		}
	}
	HandSlot[] ActiveHand;


	void Start()
	{
	}
	
	void OnDestroy()
	{
		Owner = null;
		PatchConfigs.Clear();
		PatchConfigs = null;
		for(int i = 0; i < ActiveHand.Length; ++i)
		{
			if(ActiveHand[i].Piece != null)
			{
				Destroy(ActiveHand[i].Piece.gameObject);
				ActiveHand[i].Piece = null;
			}
		}
		ActiveHand = null;
	}
	
	void Update()
	{
	}

	public void Show()
	{
		if(!Owner.ActivePlayfield.CanPlaceDecoration())
		{
			for(int i = 0; i < DecorationsOnHand.Length; ++i)
			{
				DecorationsOnHand[i].GetComponent<UnityEngine.UI.Button>().interactable = false;
			}
		}
		else
		{
			for(int i = 0; i < DecorationsOnHand.Length; ++i)
			{
				DecorationsOnHand[i].GetComponent<UnityEngine.UI.Button>().interactable = true;
			}
		}
		for(int i = 0; i < (PatchesOnHand.Length + DecorationsOnHand.Length); ++i)
		{
			GamePieceBase piece = ActiveHand[i].Piece;
			if(piece != null)
			{
				piece.gameObject.SetActive(true);
			}
		}
		gameObject.SetActive(true);
	}

	public void Hide()
	{
		for(int i = 0; i < (PatchesOnHand.Length + DecorationsOnHand.Length); ++i)
		{
			HandSlot slot = ActiveHand[i];
			if(slot.Piece != null)
			{
				slot.Piece.gameObject.SetActive(false);
			}
		}
		gameObject.SetActive(false);
	}

	public void ActivateTurn()
	{
		FillActiveHand();
	}

	static void SetLayerRecursively(GameObject obj, int layer)
	{
		obj.layer = layer;
		for(int i = 0; i < obj.transform.childCount; ++i)
		{
			SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
		}
	}

	HandSlot selectedSlot;
	public void SelectPiece(int index)
	{
		HandSlot slot = ActiveHand[index];
		if((slot.Piece != null) && (slot.uiButton.GetComponent<UnityEngine.UI.Button>().interactable))
		{
			selectedSlot = slot;
		}
	}

	public GamePieceBase GetPiece(Vector2 position)
	{
		if(selectedSlot != null)
		{
			GamePieceBase piece = selectedSlot.Piece;
			SetLayerRecursively(piece.gameObject, 0);
			piece.SetPosition(position.x, position.y);
			piece.transform.parent = null;
			piece.RemoveFromDeck();

			// Remove from hand.
			selectedSlot.Piece = null;

			// Deselect.
			selectedSlot = null;

			// And return the selected piece.
			return piece;
		}

		return null;
	}

	CirclePatch CreatePatch(CirclePatch.PatchConfig patchConfig)
	{
		GameObject patchObject = Instantiate(PatchPrefab);
		patchObject.name = gameObject.name + "_Patch";
		CirclePatch circlePatch = patchObject.GetComponent<CirclePatch>();
		circlePatch.Generate(patchConfig, Owner.PatternTextures, Owner.Colors, Owner.ComplementColor);
		circlePatch.SetOwner(Owner);
		return circlePatch;
	}
	
	DecorationCircleStopper CreateDecorationCircleStopper()
	{
		GameObject decorationObject = Instantiate(DecorationPrefab);
		decorationObject.name = gameObject.name + "_Decoration";
		DecorationCircleStopper decorationCircleStopper = decorationObject.GetComponent<DecorationCircleStopper>();
		Texture2D decorationTexture = Owner.Decorations[Random.Range(0, Owner.Decorations.Length)];
		decorationCircleStopper.Generate(1.0f, 1.0f, 1.0f, decorationTexture);
		decorationCircleStopper.SetOwner(Owner);
		return decorationCircleStopper;
	}

	public bool AddToHand(GamePieceBase piece)
	{
		bool roomToAdd = false;

		CirclePatch patch = piece.GetComponent<CirclePatch>();
		if(patch != null)
		{
			for(int i = 0; i < PatchesOnHand.Length; ++i)
			{
				HandSlot slot = ActiveHand[i];
				if(slot.Piece == null)
				{
					SetLayerRecursively(piece.gameObject, 8 + i);
					piece.SetPosition(0.0f, 0.0f); 
					piece.AddToDeck();
					slot.Piece = piece;
					roomToAdd = true;
					break;
				}
			}
		}

		DecorationCircleStopper decoration = piece.GetComponent<DecorationCircleStopper>();
		if(decoration != null)
		{
			for(int i = 0; i < DecorationsOnHand.Length; ++i)
			{
				HandSlot slot = ActiveHand[PatchesOnHand.Length + i];
				if(slot.Piece == null)
				{
					SetLayerRecursively(piece.gameObject, 10);
					piece.SetPosition(0.0f, 0.0f); 
					piece.AddToDeck();
					slot.Piece = piece;
					roomToAdd = true;
					break;
				}
			}
		}

		return roomToAdd;
	}
	
	void FillActiveHand()
	{
		// Fill patches.
		for(int i = 0; i < PatchesOnHand.Length; ++i)
		{
			HandSlot slot = ActiveHand[i];
			if((slot.Piece == null) && (PatchConfigs.Count > 0))
			{
				CirclePatch.PatchConfig patchConfig = PatchConfigs.Pop();
				CirclePatch newPatch = CreatePatch(patchConfig);
				SetLayerRecursively(newPatch.gameObject, 8 + i);
				newPatch.SetPosition(0.0f, 0.0f); 
				newPatch.AddToDeck();
				slot.Piece = newPatch;
			}
		}

		// Fill decorations.
		for(int i = 0; i < DecorationsOnHand.Length; ++i)
		{
			HandSlot slot = ActiveHand[PatchesOnHand.Length + i];
			if((slot.Piece == null) && (NumDecorations > 0))
			{
				--NumDecorations;
				DecorationCircleStopper newDecoration = CreateDecorationCircleStopper();
				SetLayerRecursively(newDecoration.gameObject, 10);
				newDecoration.SetPosition(0.0f, 0.0f); 
				newDecoration.AddToDeck();
				slot.Piece = newDecoration;
			}
		}
	}

	public void Generate(int numPatches, int numDecorations, Player owner)
	{
		Owner = owner;

		// Generate patch configs.
		NumPatches = numPatches;
		NumDecorations = numDecorations;
		PatchConfigs = new Stack<CirclePatch.PatchConfig>();
		for(int i = 0; i < NumPatches; ++i)
		{
			int segments = Random.Range(2, 7);
			PatchConfigs.Push(new CirclePatch.PatchConfig(segments, 1.0f, owner.Colors.Length, CirclePatch.MAX_PATTERNS));
		}

		// Setup active hand.
		ActiveHand = new HandSlot[PatchesOnHand.Length + DecorationsOnHand.Length];
		for(int i = 0; i < PatchesOnHand.Length; ++i)
		{
			ActiveHand[i] = new HandSlot();
			ActiveHand[i].uiButton = PatchesOnHand[i];
		}
		for(int i = 0; i < DecorationsOnHand.Length; ++i)
		{
			ActiveHand[i + PatchesOnHand.Length] = new HandSlot();
			ActiveHand[i + PatchesOnHand.Length].uiButton = DecorationsOnHand[i];
		}

		// Start hidden.
		Hide();
	}
}
