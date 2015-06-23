using UnityEngine;
using System.Collections;

public class UIMenuBG : MonoBehaviour
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

	public void Show()
	{
		if((visible != VisibleState.Visible) && (visible != VisibleState.Showing))
		{
			visible = VisibleState.Showing;
			Animator animator = GetComponent<Animator>();
			animator.SetBool("Visible", true);
			animator.Play("menu_bg_show");
		}
	}
	
	public void Hide()
	{
		visible = VisibleState.Hiding;
		Animator animator = GetComponent<Animator>();
		animator.SetBool("Visible", false);
		animator.Play("menu_bg_hide");
	}
	
	public void OnVisible()
	{
		visible = VisibleState.Visible;
	}
	
	public void OnHidden()
	{
		visible = VisibleState.Hidden;
	}
}
