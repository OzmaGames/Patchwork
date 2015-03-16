using UnityEngine;
using System.Collections;

public class PatchworkLogo : MonoBehaviour
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
		visible = VisibleState.Showing;
		Animator animator = GetComponent<Animator>();
		animator.SetBool("Visible", true);

	}

	public void Hide()
	{
		visible = VisibleState.Hiding;
		Animator animator = GetComponent<Animator>();
		animator.SetBool("Visible", false);
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
