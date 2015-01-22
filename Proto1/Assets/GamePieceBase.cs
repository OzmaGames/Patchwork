using UnityEngine;
using System.Collections;

public class GamePieceBase : MonoBehaviour {

	public const float ZPosAdd = -0.001f;
	public static float ZPos = -1.0f + ZPosAdd;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public virtual void CanPlaceAt()
	{
	}

	public virtual void Place()
	{
	}

	public virtual void SetPosition(float x, float y)
	{
	}
}
