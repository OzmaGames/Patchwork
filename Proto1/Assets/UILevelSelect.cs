using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UILevelSelect : UIPage
{
	public class SubmitDataLevelSelect : SubmitData
	{
		public SubmitDataLevelSelect(int menuOption)
		{
			MenuOption = menuOption;
		}
		
		public int MenuOption;
	}
	public SubmitDataLevelSelect LevelSelectConfig;

	public Text NumberOfStars;
	
	public override bool IsValid()
	{
		return true;
	}
	
	public override void Show()
	{
		NumberOfStars.text = "9/36";
	}
	
	public override void Hide()
	{
	}

	public int option = 0;
	public int SelectedLevel = 0;
	public void StartLevel(int level)
	{
		option = 1;
		SelectedLevel = level;
		Window.OnSubmit(this);
	}
	
	public void ShowHelp()
	{
		option = 2;
		Window.OnSubmit(this);
	}
	
	public void ShowOptions()
	{
		option = 3;
		Window.OnSubmit(this);
	}
	
	public void Submit(UIPage nextPage)
	{
		if(Window.OnSubmit != null)
		{
			Window.OnSubmit(this);
		}
		Window.Show(nextPage);
	}
}
