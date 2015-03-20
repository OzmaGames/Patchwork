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
	/*public event*/ SubmitDelegate OnSubmit;
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

	public void PlayNow()
	{
		OnSubmit = null;
		isDone = true;
	}
	
	public void Quit()
	{
		Application.Quit();
	}
	
	GameObject activePage;
	int currentPage = 0;
	public void NextPage()
	{
		if(currentPage < transform.childCount)
		{
			if((currentPage + 1) >= transform.childCount)
			{
				OnSubmit = null;
				isDone = true;
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
				activePage.GetComponent<UIPage>().Show(showData);
			}
		}
	}

	bool isDone = false;
	public bool IsDone
	{
		get { return isDone; }
	}

	UIPage.InitData showData;
	public void Show(string pageName, UIPage.InitData data, SubmitDelegate onSubmit)
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
		OnSubmit = onSubmit;
		showData = data;
		activePage.SetActive(true);
		activePage.GetComponent<UIPage>().Show(showData);

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
