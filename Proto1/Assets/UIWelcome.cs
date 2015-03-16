using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class UIWelcome : MonoBehaviour
{
	public enum VisibleState
	{
		Hidden,
		Showing,
		Visible,
		Hiding
	}
	VisibleState visible = VisibleState.Hidden;
	public VisibleState Visible
	{
		get { return visible; }
	}


	void Start()
	{
	}
	
	void Update()
	{
	}

	public delegate void SubmitDelegate(string name, int palette);
	SubmitDelegate OnSubmit;
	public void OnClickSubmit()
	{
		UnityEngine.UI.InputField txtName = transform.FindChild("Content").FindChild("PlayerCommon").FindChild("Welcome_Name").GetComponent<UnityEngine.UI.InputField>();
		UnityEngine.UI.ToggleGroup tglgPalette = transform.FindChild("Content").FindChild("PlayerCommon").FindChild("Welcome_ThemeGroup").GetComponent<UnityEngine.UI.ToggleGroup>();
		if(tglgPalette.AnyTogglesOn() && (txtName.text.Length > 0))
		{
			int palette = 0;
			string name = txtName.text;
			System.Collections.Generic.IEnumerable<UnityEngine.UI.Toggle> tglPalettes = tglgPalette.ActiveToggles();
			foreach(UnityEngine.UI.Toggle tglPalette in tglPalettes)
			{
				if(tglPalette.name == "Welcome_ThemeKaleido")
				{
					palette = 0;
					tglPalette.enabled = false;
					tglPalette.gameObject.SetActive(false);
					tglgPalette.UnregisterToggle(tglPalette);
					break;
				}
				else if(tglPalette.name == "Welcome_ThemePeacock")
				{
					palette = 1;
					tglPalette.enabled = false;
					tglPalette.gameObject.SetActive(false);
					tglgPalette.UnregisterToggle(tglPalette);
					break;
				}
			}
			Debug.Log("onSubmit(" + name + ", " + palette);
			OnSubmit(txtName.text, palette);
		}
	}

	public void WelcomePlayerOne(SubmitDelegate submitHandler)
	{
		UnityEngine.UI.InputField txtName = transform.FindChild("Content").FindChild("PlayerCommon").FindChild("Welcome_Name").GetComponent<UnityEngine.UI.InputField>();

		OnSubmit = submitHandler;
		transform.FindChild("Content").FindChild("PlayerCommon").gameObject.SetActive(true);
		transform.FindChild("Content").FindChild("Player1").gameObject.SetActive(true);
		transform.FindChild("Content").FindChild("Player2").gameObject.SetActive(false);
		Show();

	}
	
	public void WelcomePlayerTwo(SubmitDelegate submitHandler)
	{
		UnityEngine.UI.InputField txtName = transform.FindChild("Content").FindChild("PlayerCommon").FindChild("Welcome_Name").GetComponent<UnityEngine.UI.InputField>();
		txtName.text = "";

		OnSubmit = submitHandler;
		transform.FindChild("Content").FindChild("PlayerCommon").gameObject.SetActive(true);
		transform.FindChild("Content").FindChild("Player1").gameObject.SetActive(false);
		transform.FindChild("Content").FindChild("Player2").gameObject.SetActive(true);
		Show();
	}

	void ResetUI()
	{
		UnityEngine.UI.ToggleGroup tglgPalette = transform.FindChild("Content").FindChild("PlayerCommon").FindChild("Welcome_ThemeGroup").GetComponent<UnityEngine.UI.ToggleGroup>();
		tglgPalette.SetAllTogglesOff();
		UnityEngine.UI.Toggle[] toggles = tglgPalette.GetComponentsInChildren<UnityEngine.UI.Toggle>();
		if(toggles.Length == 1)
		{
			toggles[0].isOn = true;
		}

		UnityEngine.UI.InputField txtName = transform.FindChild("Content").FindChild("PlayerCommon").FindChild("Welcome_Name").GetComponent<UnityEngine.UI.InputField>();
		txtName.text = "";
		txtName.Select();
		//UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(txtName.gameObject, null);
	}

	public void Show()
	{
		ResetUI();
		if((visible != VisibleState.Visible) || (visible != VisibleState.Showing))
		{
			visible = VisibleState.Showing;
			Animator animator = GetComponent<Animator>();
			animator.SetBool("Visible", true);
			animator.Play("welcome_show");
		}
	}
	
	public void Hide()
	{
		visible = VisibleState.Hiding;
		Animator animator = GetComponent<Animator>();
		animator.SetBool("Visible", false);
		animator.Play("welcome_hide");
	}
	
	public void OnVisible()
	{
		visible = VisibleState.Visible;
	}
	
	public void OnHidden()
	{
		visible = VisibleState.Hidden;
		Debug.Log("askdl");
	}
}
