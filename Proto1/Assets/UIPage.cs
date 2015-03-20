using UnityEngine;
using System.Collections;

public abstract class UIPage : MonoBehaviour
{
	public class InitData
	{
	}

	public class SubmitData
	{
	}

	protected UIWindow Window;
	public void SetWindow(UIWindow window)
	{
		Window = window;
	}

	public abstract bool IsValid();
	public abstract void Submit();
	public abstract void Show(InitData data);
	public abstract void Hide();
}
