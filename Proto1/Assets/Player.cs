using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	public GameObject PatchPrefab;
	public Texture2D[] PatternTextures;
	public Texture2D GradientTexture;

	public string Name;

	List<GameObject> patches = new List<GameObject>();
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
		patches.Add(patchObject);
		return patchObject.GetComponent<CirclePatch>();
	}

	public void ActivateTurn()
	{
		isDone = false;
		myTurn = true;
		int segments = Random.Range(2, 5);
		activePatch = CreatePatch(segments);
		for(int i = 0; i < patches.Count; ++i)
		{
			patches[i].GetComponent<CirclePatch>().NextSegment();
		}
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
				activePatch.transform.position = pz;
			}
			if(Input.GetMouseButtonDown(0))
			{
				isDone = true;
				if(activePatch != null)
				{
					activePatch.Place();
				}
			}
		}
	}
}
