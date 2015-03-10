using UnityEngine;
using System.Collections;

public class GUI_PlayerStats : MonoBehaviour {

	public GameObject GUINamePrefab;
	public GameObject GUIScorePrefab;
	public TextAnchor Anchor = TextAnchor.UpperLeft;

	void Start()
	{
		// Fix aspect ratio and size of the text.
		float pixelRatio = (Camera.main.orthographicSize * 2.0f) / Camera.main.pixelHeight;
		GUINamePrefab.transform.localScale = new Vector3(
			GUINamePrefab.transform.localScale.x * pixelRatio * 10.0f,
			GUINamePrefab.transform.localScale.y * pixelRatio * 10.0f,
			GUINamePrefab.transform.localScale.z * pixelRatio * 0.1f);
		GUIScorePrefab.transform.localScale = new Vector3(
			GUIScorePrefab.transform.localScale.x * pixelRatio * 10.0f,
			GUIScorePrefab.transform.localScale.y * pixelRatio * 10.0f,
			GUIScorePrefab.transform.localScale.z * pixelRatio * 0.1f);

		// Set text size.
		GUINamePrefab.GetComponent<TextMesh>().fontSize = 40;
		GUIScorePrefab.GetComponent<TextMesh>().fontSize = 40;

		// Set text anchor.
		GUINamePrefab.GetComponent<TextMesh>().anchor = Anchor;
		GUIScorePrefab.GetComponent<TextMesh>().anchor = Anchor;

		// Set text color.
		GUINamePrefab.GetComponent<TextMesh>().color = Color.white;
		GUIScorePrefab.GetComponent<TextMesh>().color = Color.white;
	}

	void Update()
	{
	}

	public void SetHighlight(bool highlight)
	{
		if(highlight)
		{
			GUINamePrefab.GetComponent<TextMesh>().color = Color.red;
		}
		else
		{
			GUINamePrefab.GetComponent<TextMesh>().color = Color.white;
		}
	}

	public void SetName(string name)
	{
		GUINamePrefab.GetComponent<TextMesh>().text = name;
	}

	public void SetScore(string score)
	{
		GUIScorePrefab.GetComponent<TextMesh>().text = score;
	}
	
	public void Show()
	{
		gameObject.SetActive(true);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}
	
}
