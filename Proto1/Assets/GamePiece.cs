using UnityEngine;
using System.Collections;

public class GamePiece : GamePieceBase
{
	public Color AddColor = Color.black;
	bool doEffect = false;
	public GamePieceBase TargetPiece;
	public GameObject TargetObject;

	void Update()
	{
		if(doEffect)
		{
			UpdateEffect(AddColor);
		}
	}

	public void StartEffect(string name)
	{
		Animator animator = GetComponent<Animator>();
		animator.Play(name);
	}

	public void StopEffect()
	{
		Animator animator = GetComponent<Animator>();
		animator.SetBool("StopEffect", true);
	}

	public void OnEffectStart()
	{
		Debug.Log("OnEffectStart");
		doEffect = true;
	}

	public void OnEffectEnd()
	{
		Debug.Log("OnEffectEnd");
		doEffect = false;
	}

	public void OnUpdateEffect()
	{
		UpdateEffect(AddColor);
	}

	public override void AddToDeck()
	{
		transform.position = Vector3.zero;
	}
	
	public override void RemoveFromDeck()
	{
		transform.position = new Vector3(transform.position.x, transform.position.y, Game.BGZPos);
	}
	
	public override void Place()
	{
		PlaceChilds();
	}

	public override void UpdateEffect(Color addColor)
	{
		TargetPiece.UpdateChildsEffect(addColor);
	}

	public override void SetPosition(float x, float y)
	{
		//transform.position = new Vector3(x, y, transform.position.z);
	}

	public override Player GetOwner()
	{
		return null;
	}
	
	public override void SetOwner(Player player)
	{
	}

	public override Bounds GetBounds()
	{
		return new Bounds();
	}

	public override void StartFlash(Color startColor, Color endColor, float time)
	{
	}

	public override void SetHighlight(bool enable, Color color)
	{
	}
}
