using UnityEngine;
using System.Collections;

public abstract class UIPage : MonoBehaviour
{
	public class SubmitData
	{
	}

	public abstract bool IsValid();
	public abstract void Submit();
	public abstract void Show();
	public abstract void Hide();
}
