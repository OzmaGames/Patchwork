using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	public GameObject PatchPrefab;
	public Texture2D[] PatternTextures;
	public Texture2D GradientTexture;

	public Game ActiveGame;
	public string Name;


	CirclePatch activePatch;
	public Color[] Colors;

	bool myTurn = false;
	bool isDone = false;
	public bool IsDone
	{
		get { return isDone; }
	}

	static int UID = 0;

	// Use this for initialization
	void Start () {
		if(Name.Length == 0)
		{
			Name = "Player" + UID;
			++UID;
		}
	}

	CirclePatch CreatePatch(int segments)
	{
		GameObject patchObject = (GameObject)Instantiate(PatchPrefab);
		CirclePatch circlePatch = patchObject.AddComponent<CirclePatch>();
		circlePatch.Generate(segments, PatternTextures, Colors, GradientTexture);
		return patchObject.GetComponent<CirclePatch>();
	}

	public void ActivateTurn()
	{
		isDone = false;
		myTurn = true;
		int segments = 7;//Random.Range(2, 5);
		activePatch = CreatePatch(segments);
	}

	public void TurnOver()
	{
		myTurn = false;
		activePatch = null;
	}

	// Update is called once per frame
	void Update () {
		if(myTurn && (!isDone))
		{
			if(activePatch != null)
			{
				Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				pz.z = 0.0f;
				activePatch.transform.position = new Vector3(pz.x, pz.y, activePatch.transform.position.z);
			}
			if(Input.GetMouseButtonDown(0))
			{
				isDone = true;
				if(activePatch != null)
				{
					ActiveGame.Place(this, activePatch);
					activePatch = null;
				}
			}
		}
	}
}
