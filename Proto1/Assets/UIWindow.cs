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
	public /*event*/ SubmitDelegate OnSubmit;
	public void Submit()
	{
		/*UIPage page = activePage.GetComponent<UIPage>();
		if(page.IsValid())
		{
			page.Submit();
			if(OnSubmit != null)
			{
				OnSubmit(page);
			}
			//if(page.DoNextPage())
			//{
			//	NextPage();
			//}
		}*/
	}

	public void PlayNow()
	{
		isDone = true;
	}
	
	public void Quit()
	{
		Application.Quit();
	}
	
	UIPage activePage;
	/*int currentPage = 0;
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
	}*/

	bool isDone = false;
	public bool IsDone
	{
		get { return isDone; }
	}

	public void Show(UIPage page)
	{
		if(activePage != null)
		{
			activePage.Hide();
			activePage.gameObject.SetActive(false);
			activePage.SetWindow(null);
		}
		activePage = page;
		if(activePage != null)
		{
			isDone = false;
			activePage = page;
			activePage.SetWindow(this);
			activePage.gameObject.SetActive(true);
			activePage.Show();
		}
		else
		{
			isDone = true;
		}

		if((visible != VisibleState.Visible) && (visible != VisibleState.Showing))
		{
			Debug.Log("DoShowing: " + visible.ToString());
			visible = VisibleState.Showing;
			Animator animator = GetComponent<Animator>();
			animator.SetBool("Visible", true);
			animator.Play("window_show");
		}
	}

	public void Hide()
	{
		Debug.Log("Hide");
		visible = VisibleState.Hiding;
		Animator animator = GetComponent<Animator>();
		animator.SetBool("Visible", false);
		animator.Play("window_hide");
	}
	
	public void OnVisible()
	{
		Debug.Log("OnVisible");
		visible = VisibleState.Visible;
	}
	
	public void OnHidden()
	{
		Debug.Log("OnHidden");
		activePage.Hide();
		activePage.gameObject.SetActive(false);

		visible = VisibleState.Hidden;
	}
}
