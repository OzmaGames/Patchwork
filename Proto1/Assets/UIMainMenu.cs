using UnityEngine;
using System.Collections;

public class UIMainMenu : UIPage
{
	public class SubmitDataMainMenu : SubmitData
	{
		public SubmitDataMainMenu(int menuOption)
		{
			MenuOption = menuOption;
		}
		
		public int MenuOption;
	}
	public SubmitDataMainMenu MainMenuConfig;

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

	public override void Show()
	{
	}

	public override void Hide()
	{
	}

	public int option = 0;
	public void StartSinglePlayer()
	{
		option = 1;
		Window.OnSubmit(this);
		Window.PlayNow();
	}

	public void StartMultiPlayer()
	{
		option = 2;
		Window.OnSubmit(this);
	}

	public void ShowHelp()
	{
		option = 3;
		Window.OnSubmit(this);
	}

	public void ShowOptions()
	{
		option = 4;
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