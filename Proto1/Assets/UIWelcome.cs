using UnityEngine;
using System.Collections;

public class UIWelcome : MonoBehaviour
{
	void Start()
	{
		WelcomePlayerOne();
	}
	
	void Update()
	{
	}

	void OnClick(int player)
	{
	}

	void WelcomePlayerOne()
	{
		gameObject.SetActive(true);
		transform.FindChild("Welcome1").gameObject.SetActive(true);
		transform.FindChild("Welcome2").gameObject.SetActive(false);
		transform.FindChild("Welcome_PlayerOne").gameObject.SetActive(true);
		transform.FindChild("Welcome_PlayerTwo").gameObject.SetActive(false);
		transform.FindChild("Welcome_Name").GetComponent<UnityEngine.UI.InputField>().text = "";
		transform.FindChild("Welcome_Theme").gameObject.SetActive(true);
		transform.FindChild("Welcome_GetTheme").gameObject.SetActive(false);
		transform.FindChild("Welcome_Ok").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate{OnClick(0);});

	}
	
	void WelcomePlayerTwo()
	{
		gameObject.SetActive(true);
		transform.FindChild("Welcome1").gameObject.SetActive(false);
		transform.FindChild("Welcome2").gameObject.SetActive(true);
		transform.FindChild("Welcome_PlayerOne").gameObject.SetActive(false);
		transform.FindChild("Welcome_PlayerTwo").gameObject.SetActive(true);
		transform.FindChild("Welcome_Name").GetComponent<UnityEngine.UI.InputField>().text = "";
		transform.FindChild("Welcome_Theme").gameObject.SetActive(false);
		transform.FindChild("Welcome_GetTheme").gameObject.SetActive(true);
	}	
}
