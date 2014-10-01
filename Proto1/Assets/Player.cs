using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	public GameObject PatchPrefab;

	public string Name;

	List<GameObject> patches = new List<GameObject>();
	CirclePatch activePatch;

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

	CirclePatch CreatePatch(float innerRadius, float outerRadius)
	{
		GameObject patchObject = (GameObject)Instantiate(PatchPrefab);
		CirclePatch circlePatch = patchObject.AddComponent<CirclePatch>();
		circlePatch.Generate(innerRadius, outerRadius);
		patches.Add(patchObject);
		return patchObject.GetComponent<CirclePatch>();
	}

	public void ActivateTurn()
	{
		isDone = false;
		myTurn = true;
		float innerRadius = 0.0f;//Random.Range(0.0f, 2.5f);
		float outerRadius = Random.Range(innerRadius, innerRadius + 0.1f + 3.9f);
		activePatch = CreatePatch(innerRadius, outerRadius);
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
			}
		}
	}
}
