using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DecorationCircleStopper : GamePieceBase {

	const float ZPosDecorationLayer = -5.0f;

	Texture2D BGTexture;
	Mesh GeneratedMesh;
	float HalfWidth = 0.0f;
	float HalfHeight = 0.0f;

	public Player Owner;

	public CirclePatch PatchTarget;

	public bool isActive = false;

	bool isPlaced = false;

	Color flashColorStart = Color.black;
	Color flashColorEnd = Color.white;
	bool isFlashing = false;
	float flashValue = 0.0f;
	float flashTimer = 0.0f;
	
	Color highlightColor = Color.black;
	bool doHighlight = false;

	void Start()
	{
	}

	public void Generate(float width, float height, float uvScale, Texture2D bgTexture)
	{
		HalfWidth = width;
		HalfHeight = height;
		transform.position = new Vector3(0.0f, 0.0f, 0.0f);//	Game.FGZPos);
//		Game.FGZPos += Game.ZPosAdd;

		BGTexture = bgTexture;
		
		GeneratedMesh = Helpers.GenerateQuad(width, height, uvScale);
		
		// Setup mesh.
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = GeneratedMesh;
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material.shader = Shader.Find("Custom/Decoration");
		meshRenderer.material.mainTexture = BGTexture;
	}

	public bool CollidesAgainst(CirclePatch patch)
	{
		float x1 = transform.position.x;
		float y1 = transform.position.y;
		float x2 = patch.transform.position.x;
		float y2 = patch.transform.position.y;
		float r1 = 0.125f;
		float r2 = patch.GetSize() * 1.0f;//SegmentScale;
		
		float distance = ((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1));
		float sumRadius = ((r1 + r2) * (r1 + r2));
		if(distance <= sumRadius)
		{
			PatchTarget = patch;
			return true;
		}
		
		return false;
	}

	public void SetCollider(CirclePatch collider)
	{
		PatchTarget = collider;
	}

	public override Player GetOwner()
	{
		return Owner;
	}

	public override void SetOwner(Player player)
	{
		Owner = player;
	}

	public override void AddToDeck()
	{
		isActive = false;
		transform.position = Vector3.zero;
	}
	
	public override void RemoveFromDeck()
	{
		transform.position = new Vector3(transform.position.x, transform.position.y, Game.BGZPos);
		isActive = true;
	}
	
	public override void Place()
	{
		PatchTarget.PlaceDecoration(this);
		//ActivePlayfield.PieceDone(this);

		/*bool alreadyDoneGrowing = PatchTarget.HasStoppedGrowing();
		PatchTarget.PlaceDecoration(this);

		int scoreToGive = alreadyDoneGrowing ? 2 : (int)PatchTarget.GetSize();
		if(Owner != PatchTarget.GetOwner())
		{
			scoreToGive = scoreToGive / 2;
			Owner.AddScore(scoreToGive);
		}
		else if(alreadyDoneGrowing)
		{
			Owner.AddScore(scoreToGive);
		}*/

		transform.parent = PatchTarget.transform;
		transform.localPosition = new Vector3(0.0f, 0.0f, ZPosDecorationLayer);
		PlaceChilds();
		isPlaced = true;
		PatchTarget = null;
	}

	public override void SetPosition(float x, float y)
	{
		transform.position = new Vector3(x, y, transform.position.z);
	}

	public override Bounds GetBounds()
	{
		return new Bounds(transform.position, new Vector3(HalfWidth, HalfHeight));
	}

	public override void SetHighlight(bool enable, Color color)
	{
		doHighlight = enable;
		highlightColor = color;
	}

	void UpdateHighlight()
	{
		MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
		if(!doHighlight)
		{
			meshRenderer.material.SetColor("_AddColor", Color.black);
			return;
		}
		meshRenderer.material.SetColor("_AddColor", highlightColor);
	}

	public override void StartFlash(Color startColor, Color endColor, float time)
	{
		flashColorStart = startColor;
		flashColorEnd = endColor;
		isFlashing = true;
		flashValue = 0.0f;
		flashTimer = time;
	}

	void UpdateFlash()
	{
		if(!isFlashing)
		{
			return;
		}
		if(flashTimer > 0.0f)
		{
			GetComponent<Renderer>().material.SetColor("_AddColor", Color.Lerp(flashColorStart, flashColorEnd, flashValue));
			flashTimer -= Time.deltaTime;
		}
		else
		{
			GetComponent<Renderer>().material.SetColor("_AddColor", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
			isFlashing = false;
		}
		flashValue += FLASH_SPEED * Time.deltaTime;
		if(flashValue > 1.0f)
		{
			flashValue = 0.0f;
		}
	}

	public override void UpdateEffect(Color addColor)
	{
		GetComponent<Renderer>().material.SetColor("_AddColor", addColor);
		UpdateChildsEffect(addColor);
	}

	void OnDestroy()
	{
		BGTexture = null;
		GeneratedMesh = null;
		Owner = null;
		PatchTarget = null;
	}

	void Update()
	{
		if(isPlaced)
		{
			if(doHighlight)
			{
				UpdateHighlight();
			}
			if(isFlashing)
			{
				UpdateFlash();
			}
		}
		else if(isActive)
		{
			List<GamePieceBase> collidedPieces;
			Owner.ActivePlayfield.GetCollision(this, out collidedPieces);
			for(int i = 0; i < collidedPieces.Count; ++i)
			{
				CirclePatch patch = collidedPieces[i].GetComponent<CirclePatch>();
				if(patch != null)
				{
					if(patch.GetDecoration() == null)
					{
						patch.StartFlash(new Color(0.5f, 0.5f, 0.5f), Color.black, 0.2f);
					}
					else
					{
					patch.StartFlash(new Color(0.5f, 0.0f, 0.0f), Color.black, 0.2f);
					}
				}
			}
		}
	}

}
