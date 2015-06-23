using UnityEngine;
using System.Collections;

public class UIRules : UIPage
{
	void Start()
	{
	}
	
	void Update()
	{
	}

	public override bool IsValid()
	{
		return true;
	}

	public UIPage NextPage;
	public void Submit()
	{
		Window.Submit();
		Window.Show(NextPage);
	}
	
	public override void Show()
	{
	}

	public override void Hide()
	{
	}
}
