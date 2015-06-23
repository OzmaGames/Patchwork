using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class UIPlayerConfig : UIPage
{
	public class SubmitDataPlayerConfig : SubmitData
	{
		public SubmitDataPlayerConfig(string name, int palette)
		{
			Name = name;
			Palette = palette;
		}

		public string Name;
		public int Palette;
	}
	public SubmitDataPlayerConfig PlayerConfig;

	void Start()
	{
	}
	
	void Update()
	{
	}

	public GameObject PaletteGroup;
	public GameObject Name;
	public void DisablePalette(int paletteIndex)
	{
		UnityEngine.UI.InputField txtName = Name.GetComponent<UnityEngine.UI.InputField>();
		UnityEngine.UI.ToggleGroup tglgPalette = PaletteGroup.GetComponent<UnityEngine.UI.ToggleGroup>();
		if(paletteIndex == 0)
		{
			UnityEngine.UI.Toggle tglPalette = tglgPalette.transform.FindChild("Welcome_ThemeKaleido").GetComponent<UnityEngine.UI.Toggle>();
			tglPalette.enabled = false;
			tglPalette.gameObject.SetActive(false);
			tglgPalette.UnregisterToggle(tglPalette);

			tglPalette = tglgPalette.transform.FindChild("Welcome_ThemePeacock").GetComponent<UnityEngine.UI.Toggle>();
			tglPalette.enabled = true;
			tglPalette.gameObject.SetActive(true);
			tglgPalette.RegisterToggle(tglPalette);
		}
		else if(paletteIndex == 1)
		{
			UnityEngine.UI.Toggle tglPalette = tglgPalette.transform.FindChild("Welcome_ThemeKaleido").GetComponent<UnityEngine.UI.Toggle>();
			tglPalette.enabled = true;
			tglPalette.gameObject.SetActive(true);
			tglgPalette.RegisterToggle(tglPalette);

			tglPalette = tglgPalette.transform.FindChild("Welcome_ThemePeacock").GetComponent<UnityEngine.UI.Toggle>();
			tglPalette.enabled = false;
			tglPalette.gameObject.SetActive(false);
			tglgPalette.UnregisterToggle(tglPalette);
			
		}
		ResetUI();
	}

	public override bool IsValid()
	{
		UnityEngine.UI.InputField txtName = Name.GetComponent<UnityEngine.UI.InputField>();
		UnityEngine.UI.ToggleGroup tglgPalette = PaletteGroup.GetComponent<UnityEngine.UI.ToggleGroup>();
		if(tglgPalette.AnyTogglesOn() && (txtName.text.Length > 0))
		{
			return true;
		}

		return false;
	}

	public UIPage NextPage;
	public void Submit()
	{
		if(!IsValid())
		{
			return;
		}

		UnityEngine.UI.InputField txtName = Name.GetComponent<UnityEngine.UI.InputField>();
		UnityEngine.UI.ToggleGroup tglgPalette = PaletteGroup.GetComponent<UnityEngine.UI.ToggleGroup>();
		if(tglgPalette.AnyTogglesOn() && (txtName.text.Length > 0))
		{
			int palette = 0;
			string name = txtName.text;
			System.Collections.Generic.IEnumerable<UnityEngine.UI.Toggle> tglPalettes = tglgPalette.ActiveToggles();
			foreach(UnityEngine.UI.Toggle tglPalette in tglPalettes)
			{
				if(tglPalette.name == "Welcome_ThemeKaleido")
				{
					palette = 0;
					//tglPalette.enabled = false;
					//tglPalette.gameObject.SetActive(false);
					//tglgPalette.UnregisterToggle(tglPalette);
					break;
				}
				else if(tglPalette.name == "Welcome_ThemePeacock")
				{
					palette = 1;
					//tglPalette.enabled = false;
					//tglPalette.gameObject.SetActive(false);
					//tglgPalette.UnregisterToggle(tglPalette);
					break;
				}
			}
			PlayerConfig = new SubmitDataPlayerConfig(txtName.text, palette);
		}

		if(Window.OnSubmit != null)
		{
			Window.OnSubmit(this);
		}
		Window.Show(NextPage);
	}

	public override void Show()
	{
		ResetUI();
	}

	public override void Hide()
	{
	}

	void ResetUI()
	{
		UnityEngine.UI.ToggleGroup tglgPalette = PaletteGroup.GetComponent<UnityEngine.UI.ToggleGroup>();
		tglgPalette.SetAllTogglesOff();
		UnityEngine.UI.Toggle[] toggles = tglgPalette.GetComponentsInChildren<UnityEngine.UI.Toggle>();
		if(toggles.Length > 0)
		{
			toggles[0].isOn = true;
		}

		UnityEngine.UI.InputField txtName = Name.GetComponent<UnityEngine.UI.InputField>();
		txtName.text = "";
		//txtName.Select();
		//UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(txtName.gameObject, null);
	}
}
