using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIWindow : MonoBehaviour
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

	public delegate void SubmitDelegate(UIPage page);
	SubmitDelegate OnSubmit;
	public void OnClickSubmit()
	{
		UnityEngine.UI.InputField txtName = transform.FindChild("PlayerCommon").FindChild("Welcome_Name").GetComponent<UnityEngine.UI.InputField>();
		UnityEngine.UI.ToggleGroup tglgPalette = transform.FindChild("PlayerCommon").FindChild("Welcome_ThemeGroup").GetComponent<UnityEngine.UI.ToggleGroup>();
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
//			OnSubmit(txtName.text, palette);
		}
	}

	public void Submit()
	{
		UIPage page = activePage.GetComponent<UIPage>();
		if(page.IsValid())
		{
			activePage.GetComponent<UIPage>().Submit();
			OnSubmit(activePage.GetComponent<UIPage>());
			NextPage();
		}
	}

	GameObject activePage;
	int currentPage = 0;
	public void NextPage()
	{
		if(currentPage < transform.childCount)
		{
			if((currentPage + 1) >= transform.childCount)
			{
				isDone = true;
				Hide();
			}
			else
			{
				activePage.GetComponent<UIPage>().Hide();
				activePage.SetActive(false);
			}
			++currentPage;
			if(currentPage < transform.childCount)
			{
				activePage = transform.GetChild(currentPage).gameObject;
				activePage.SetActive(true);
				activePage.GetComponent<UIPage>().Show();
			}
		}
	}

	bool isDone = false;
	public bool IsDone
	{
		get { return isDone; }
	}

	public void Show(string pageName, SubmitDelegate onSubmit)
	{
		isDone = false;
		if(pageName.Length > 0)
		{
			activePage = transform.FindChild(pageName).gameObject;
			currentPage = activePage.transform.GetSiblingIndex();
		}
		else
		{
			activePage = transform.GetChild(currentPage).gameObject;
		}
		activePage.SetActive(true);
		activePage.GetComponent<UIPage>().Show();
		OnSubmit = onSubmit;

		if((visible != VisibleState.Visible) || (visible != VisibleState.Showing))
		{
			visible = VisibleState.Showing;
			Animator animator = GetComponent<Animator>();
			animator.SetBool("Visible", true);
			animator.Play("window_show");
		}
	}
	
	public void Hide()
	{
		visible = VisibleState.Hiding;
		Animator animator = GetComponent<Animator>();
		animator.SetBool("Visible", false);
		animator.Play("window_hide");
	}
	
	public void OnVisible()
	{
		visible = VisibleState.Visible;
	}
	
	public void OnHidden()
	{
		activePage.GetComponent<UIPage>().Hide();
		activePage.SetActive(false);

		visible = VisibleState.Hidden;
	}
}
