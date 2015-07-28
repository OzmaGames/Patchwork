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
		if(NextPage != null)
		{
			if(CurrentPage != null)
			{
				CurrentPage.Hide();
				CurrentPage.gameObject.SetActive(false); // TODO: Move to page.
				CurrentPage.SetWindow(null);
			}
			CurrentPage = NextPage;
			NextPage = null;
			CurrentPage.SetWindow(this);
			CurrentPage.gameObject.SetActive(true);
			CurrentPage.Show();
		}
	}

	public delegate void SubmitDelegate(UIPage page);
	public /*event*/ SubmitDelegate OnSubmit;

	public void Quit()
	{
		Application.Quit();
	}

	UIPage CurrentPage;
	UIPage NextPage;
	public void Show(UIPage page)
	{
		NextPage = page;
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
		if(CurrentPage != null)
		{
			CurrentPage.Hide();
		}
	}
	
	public void OnVisible()
	{
		Debug.Log("OnVisible");
		visible = VisibleState.Visible;
	}
	
	public void OnHidden()
	{
		Debug.Log("OnHidden");
		CurrentPage.gameObject.SetActive(false);
		visible = VisibleState.Hidden;
	}
}
