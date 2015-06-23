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
}
