using UnityEngine;
using System.Collections;

public class UIHelpScreen : MonoBehaviour
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
		if((visible == VisibleState.Visible) && Input.GetMouseButtonDown(0))
		{
			Hide();
		}
	}

	public void Show()
	{
		if(visible == VisibleState.Hidden)
		{
			Animator animator = GetComponent<Animator>();
			gameObject.SetActive(true);
			visible = VisibleState.Showing;
			animator.SetBool("Visible", true);
			animator.Play("help_show");
		}
	}
	
	public void Hide()
	{
		Animator animator = GetComponent<Animator>();
		visible = VisibleState.Hiding;
		animator.SetBool("Visible", false);
		animator.Play("help_hide");
	}
	
	public void OnVisible()
	{
		visible = VisibleState.Visible;
	}
	
	public void OnHidden()
	{
		visible = VisibleState.Hidden;
		gameObject.SetActive(false);
	}
}
