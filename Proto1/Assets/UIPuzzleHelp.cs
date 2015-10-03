using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIPuzzleHelp : UIPage
{
	public override bool IsValid()
	{
		return true;
	}
	
	public override void Show()
	{
	}
	
	public override void Hide()
	{
	}

	public void Close()
	{
		Window.OnSubmit(this);
	}
}
