using UnityEngine;
using System.Collections;

public abstract class UIPage : MonoBehaviour
{
	public class SubmitData
	{
	}

	protected UIWindow Window;
	public void SetWindow(UIWindow window)
	{
		Window = window;
	}

	public abstract bool IsValid();
	public abstract void Show();
	public abstract void Hide();
}
